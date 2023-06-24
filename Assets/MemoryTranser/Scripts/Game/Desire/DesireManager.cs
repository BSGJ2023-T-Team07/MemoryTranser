using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MemoryTranser.Scripts.Game.Fairy;
using MemoryTranser.Scripts.Game.GameManagers;
using MemoryTranser.Scripts.Game.Phase;
using MemoryTranser.Scripts.Game.Util;
using UnityEngine;
using UniRx;
using Random = UnityEngine.Random;

namespace MemoryTranser.Scripts.Game.Desire {
    public class DesireManager : MonoBehaviour, IOnStateChangedToResult, IOnStateChangedToFinished {
        [SerializeField] private PhaseManager phaseManager;
        [SerializeField] private GameObject desirePrefab;
        [SerializeField] private FairyCore fairyCore;

        [Header("Desireが最初にスポーンする間隔(秒)")] [SerializeField]
        private float initialSpawnIntervalSec;

        [Header("Desireが再スポーンするまでの時間(秒)")] [SerializeField]
        private float spawnIntervalSec;

        [Header("ステージ上に現れるDesireの最大数")] [SerializeField]
        private int maxSpawnCount;

        [Header("Desireのスポーン位置")] [SerializeField]
        private Transform[] spawnPointObjects;

        [Header("各スポーン位置からDesireがスポーンするかどうかのフラグ")] [SerializeField]
        private bool[] canSpawnFlags;

        [Header("Desireが追うTransform")] [SerializeField]
        private Transform targetTransform;

        [Header("既にDesireがステージ上に居た時にスポーンをどれだけ遅らせるか(秒)")] [SerializeField]
        private float delaySecWhenDesireExist;


        private readonly Queue<DesireCore> _desireCorePool = new();
        private Renderer[] _spawnPointRenderers;

        #region eventの定義

        private readonly ReactiveProperty<int> _existingDesireCount = new(0);
        public IReadOnlyReactiveProperty<int> ExistingDesireCount => _existingDesireCount;

        #endregion

        #region Unityから呼ばれる

        private void Awake() {
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

            for (var i = 0; i < maxSpawnCount; i++) {
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
                    CollectDesire(desireCore);
                    Vector3 spawnPos;
                    bool isResult;

                    while (true) {
                        await UniTask.Delay(TimeSpan.FromSeconds(spawnIntervalSec));
                        var info = GetCanSpawnAndSpawnPosition();

                        isResult = GameFlowManager.I.NowGameState == GameState.Result;

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
            }
        }

        private async void Start() {
            //ステージ上にDesireを生成する
            for (var i = 0; i < maxSpawnCount; i++) {
                await UniTask.Delay(TimeSpan.FromSeconds(initialSpawnIntervalSec));
                SpawnDesire(GetCanSpawnAndSpawnPosition().Item2);
            }
        }

        #endregion

        private DesireCore ApplyRandomParametersForDesire(DesireCore desireCore) {
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
        private async void SpawnDesire(Vector3 spawnPos) {
            if (_desireCorePool.Count == 0) {
                return;
            }

            if (_desireCorePool.Count < maxSpawnCount) {
                //もしステージ上に既にDesireが居たら、しばらく待つ
                await UniTask.Delay(TimeSpan.FromSeconds(delaySecWhenDesireExist));
            }

            var desireCore = _desireCorePool.Dequeue();
            _existingDesireCount.Value++;

            desireCore.gameObject.SetActive(true);
            desireCore.Appear(spawnPos);
        }

        /// <summary>
        /// 引数のDesireCoreをキューに加える
        /// </summary>
        /// <param name="desireCore"></param>
        private void CollectDesire(DesireCore desireCore) {
            _desireCorePool.Enqueue(desireCore);
            _existingDesireCount.Value--;
        }

        #region interfaceの実装

        public void OnStateChangedToResult() {
            //gameStateがResultに遷移したらどこからもスポーンしなくする
            for (var i = 0; i < canSpawnFlags.Length; i++) {
                canSpawnFlags[i] = false;
            }

            _existingDesireCount.Dispose();
        }

        public void OnStateChangedToFinished() { }

        #endregion
    }
}