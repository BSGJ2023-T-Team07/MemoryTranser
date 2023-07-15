using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MemoryTranser.Scripts.Game.BrainEvent;
using MemoryTranser.Scripts.Game.Fairy;
using MemoryTranser.Scripts.Game.GameManagers;
using MemoryTranser.Scripts.Game.Phase;
using MemoryTranser.Scripts.Game.Util;
using UnityEngine;
using UniRx;
using Random = UnityEngine.Random;

namespace MemoryTranser.Scripts.Game.Desire {
    public class DesireManager : MonoBehaviour, IOnGameAwake, IOnGameStart, IOnStateChangedToResult,
        IOnStateChangedToFinished {
        [SerializeField] private PhaseManager phaseManager;
        [SerializeField] private BrainEventManager brainEventManager;
        [SerializeField] private GameObject desirePrefab;
        [SerializeField] private FairyCore fairyCore;

        [Header("Desireが最初にスポーンする間隔(秒)")] [SerializeField]
        private float initialSpawnIntervalSec;

        [Header("Desireが再スポーンするまでの初期の時間(秒)")] [SerializeField]
        private float spawnIntervalSec;

        [Header("ステージ上に現れるDesireの初期の最大数")] [SerializeField]
        private int maxDefaultSpawnCount;

        [Header("既にDesireがステージ上に居た時にスポーンをどれだけ遅らせるかの初期の時間(秒)")] [SerializeField]
        private float delaySecWhenDesireExist;

        [Header("煩悩大量発生時に新たに現れるDesireの数")] [SerializeField]
        private int spawnCountOnDesireOutbreak;

        [Header("大量発生するDesireのスポーン間隔(秒)")] [SerializeField]
        private float spawnIntervalSecOnDesireOutbreak;

        [Header("Desireのスポーン位置")] [SerializeField]
        private Transform[] spawnPointObjects;

        [Header("各スポーン位置からDesireがスポーンするかどうかのフラグ")] [SerializeField]
        private bool[] canSpawnFlags;

        [Header("Desireが追うTransform")] [SerializeField]
        private Transform targetTransform;


        private Queue<DesireCore> _desireCorePool = new();
        private Queue<DesireCore> _desireCorePoolOnDesireOutbreak = new();
        private Renderer[] _spawnPointRenderers;

        #region eventの定義

        //これ単体でいじることは無い。常に_desirePoolと付随していじる
        private readonly ReactiveCollection<DesireCore> _existingDesireCores = new();

        public IReadOnlyReactiveCollection<DesireCore> ExistingDesireCores => _existingDesireCores;

        #endregion

        private DesireCore GenerateNewDefaultDesire() {
            //予め最大数だけDesireをつくっておく
            var desireObj = Instantiate(desirePrefab, transform.position, Quaternion.identity);
            var desireCore = desireObj.GetComponent<DesireCore>();

            //Desireの追跡対象を指定
            desireCore.targetTransform = targetTransform;

            //Desireにランダムなパラメーターを設定する
            ApplyRandomParametersForDesire(desireCore);
            //生成したDesireをキューに入れておく
            _desireCorePool.Enqueue(desireCore);

            //つくったDesireのOnDisappearを購読して、消えたらまた生成するようにする
            desireCore.OnDisappear.Subscribe(async _ => {
                CollectDefaultDesire(desireCore);


                Vector3 spawnPos;
                bool isResult;

                while (true) {
                    await UniTask.Delay(TimeSpan.FromSeconds(spawnIntervalSec));
                    if (_existingDesireCores.Count > maxDefaultSpawnCount) {
                        continue;
                    }

                    isResult = GameFlowManager.I.CurrentGameState == GameState.Result;

                    var info = GetCanSpawnAndSpawnPosition();

                    if (isResult) {
                        spawnPos = Vector3.zero;
                        break;
                    }

                    if (info.Item1) {
                        spawnPos = info.Item2;
                        break;
                    }
                }

                if (!isResult) {
                    ApplyRandomParametersForDesire(desireCore);

                    if (_existingDesireCores.Count > 0) {
                        //もしステージ上に既にDesireが居たら、しばらく待つ
                        await UniTask.Delay(TimeSpan.FromSeconds(delaySecWhenDesireExist));
                    }

                    if (_desireCorePool.Count == 0) {
                        return;
                    }

                    SpawnDesire(spawnPos);
                }
            });


            //つくったDesireのOnBeAttackedを購読して、そのDesireが倒されたらスコアを加算する
            desireCore.OnBeAttacked.Subscribe(_ => {
                phaseManager.AddCurrentScoreOnDefeatDesire();
                fairyCore.AddBlinkTicketOnDefeatDesire();
            });

            //最初はすべて非アクティブにしておく
            desireObj.SetActive(false);

            return desireCore;
        }

        private DesireCore GenerateNewOutBreakDesire() {
            //予め最大数だけDesireをつくっておく
            var desireObj = Instantiate(desirePrefab, transform.position, Quaternion.identity);
            var desireCore = desireObj.GetComponent<DesireCore>();

            //Desireの追跡対象を指定
            desireCore.targetTransform = targetTransform;

            //Desireにランダムなパラメーターを設定する
            ApplyRandomParametersForDesire(desireCore);

            //生成したDesireをキューに入れておく
            _desireCorePoolOnDesireOutbreak.Enqueue(desireCore);

            desireCore.OnDisappear.Subscribe(_ => { CollectOutBreakDesire(desireCore); });

            //つくったDesireのOnBeAttackedを購読して、そのDesireが倒されたらスコアを加算する
            desireCore.OnBeAttacked.Subscribe(_ => {
                phaseManager.AddCurrentScoreOnDefeatDesire();
                fairyCore.AddBlinkTicketOnDefeatDesire();
            });

            //最初はすべて非アクティブにしておく
            desireObj.SetActive(false);

            return desireCore;
        }

        private static DesireCore ApplyRandomParametersForDesire(DesireCore desireCore) {
            //ランダムにDesireのパラメータを決める
            var randomDesireType = (DesireType)Random.Range(0, (int)DesireType.Count);

            //決定したパラメーターを代入する
            desireCore.MyType = randomDesireType;

            //決定したパラメーターから他の値に反映させる
            desireCore.SpRr.sprite = randomDesireType.ToDesireSprite();
            desireCore.MyParameters.InitializeParameters(randomDesireType);


            return desireCore;
        }

        /// <summary>
        /// カメラに写っていないspawnPointのうち、最もtargetTransformに近いspawnPointの座標を返す
        /// </summary>
        /// <returns></returns>
        private (bool, Vector3) GetCanSpawnAndSpawnPosition() {
            var notInCameraSpawnPoints = spawnPointObjects
                .Where((t, i) => !_spawnPointRenderers[i].isVisible && canSpawnFlags[i]).ToArray();

            if (brainEventManager.OnBrainEventTransition.Value == BrainEventType.DesireOutbreak) {
                int randomIndex;

                if (notInCameraSpawnPoints.Length == 0) {
                    randomIndex = Random.Range(0, spawnPointObjects.Length);
                    return (true, spawnPointObjects[randomIndex].position);
                }
                else {
                    randomIndex = Random.Range(0, notInCameraSpawnPoints.Length);
                    return (true, notInCameraSpawnPoints[randomIndex].position);
                }
            }

            var nearestSpawnPointPos = Vector3.zero;
            for (var i = 0; i < notInCameraSpawnPoints.Length; i++) {
                if (i == 0) {
                    nearestSpawnPointPos = notInCameraSpawnPoints[i].position;
                }
                else {
                    if (Vector3.Distance(targetTransform.position, notInCameraSpawnPoints[i].position) <
                        Vector3.Distance(targetTransform.position, nearestSpawnPointPos)) {
                        nearestSpawnPointPos = notInCameraSpawnPoints[i].position;
                    }
                }
            }

            return (nearestSpawnPointPos != Vector3.zero, nearestSpawnPointPos);
        }

        /// <summary>
        /// 指定した位置にDesireをスポーンさせる(キューから取り出す)
        /// </summary>
        /// <param name="spawnPos"></param>
        private void SpawnDesire(Vector3 spawnPos) {
            var desireCore = _desireCorePool.Dequeue();
            _existingDesireCores.Add(desireCore);

            desireCore.Appear(spawnPos);
        }

        private void SpawnOutBreakDesire(Vector3 spawnPos) {
            var desireCore = _desireCorePoolOnDesireOutbreak.Dequeue();
            desireCore.Appear(spawnPos);
        }

        /// <summary>
        /// 引数のDesireCoreをキューに加える
        /// </summary>
        /// <param name="desireCore"></param>
        private void CollectDefaultDesire(DesireCore desireCore) {
            _desireCorePool.Enqueue(desireCore);
            _existingDesireCores.Remove(desireCore);
        }

        private void CollectOutBreakDesire(DesireCore desireCore) {
            _desireCorePoolOnDesireOutbreak.Enqueue(desireCore);
        }

        #region interfaceの実装

        public void OnGameAwake() {
            //spawnPointの数だけRendererを取得しておく
            _spawnPointRenderers = new Renderer[spawnPointObjects.Length];
            for (var i = 0; i < spawnPointObjects.Length; i++) {
                _spawnPointRenderers[i] = spawnPointObjects[i].GetComponent<Renderer>();
            }

            //spawnPointの数だけcanSpawnFlagsを作る。初期状態だとすべてtrue
            canSpawnFlags = new bool[spawnPointObjects.Length];
            for (var i = 0; i < canSpawnFlags.Length; i++) {
                canSpawnFlags[i] = true;
            }

            for (var i = 0; i < maxDefaultSpawnCount; i++) {
                GenerateNewDefaultDesire();
            }

            for (var i = 0; i < spawnCountOnDesireOutbreak; i++) {
                GenerateNewOutBreakDesire();
            }

            brainEventManager.OnBrainEventTransition.Where(value => value == BrainEventType.DesireOutbreak).Subscribe(
                async _ => {
                    for (var i = 0; i < spawnCountOnDesireOutbreak; i++) {
                        SpawnOutBreakDesire(GetCanSpawnAndSpawnPosition().Item2);

                        await UniTask.Delay(TimeSpan.FromSeconds(spawnIntervalSecOnDesireOutbreak));
                    }
                });
        }

        public async void OnGameStart() {
            //ステージ上にDesireを生成する
            for (var i = 0; i < maxDefaultSpawnCount; i++) {
                await UniTask.Delay(TimeSpan.FromSeconds(initialSpawnIntervalSec));

                if (_desireCorePool.Count == 0) {
                    continue;
                }

                SpawnDesire(GetCanSpawnAndSpawnPosition().Item2);
            }
        }

        public void OnStateChangedToResult() {
            //gameStateがResultに遷移したらどこからもスポーンしなくする
            for (var i = 0; i < canSpawnFlags.Length; i++) {
                canSpawnFlags[i] = false;
            }

            if (_existingDesireCores.Count > 0) {
                foreach (var desire in _existingDesireCores) {
                    desire.gameObject.SetActive(false);
                }
            }
        }

        public void OnStateChangedToFinished() {
            if (_desireCorePool.Count > 0) {
                foreach (var desire in _desireCorePool) {
                    Destroy(desire);
                }
            }

            _desireCorePool = null;


            if (_existingDesireCores.Count > 0) {
                foreach (var desire in _existingDesireCores) {
                    Destroy(desire);
                }
            }

            _existingDesireCores.Dispose();

            if (_desireCorePoolOnDesireOutbreak.Count > 0) {
                foreach (var desire in _desireCorePoolOnDesireOutbreak) {
                    Destroy(desire);
                }
            }

            _desireCorePoolOnDesireOutbreak = null;
        }

        #endregion
    }
}