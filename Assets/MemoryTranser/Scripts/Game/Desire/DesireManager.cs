using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MemoryTranser.Scripts.Game.GameManagers;
using MemoryTranser.Scripts.Game.Phase;
using UnityEngine;
using UniRx;

namespace MemoryTranser.Scripts.Game.Desire {
    public class DesireManager : MonoBehaviour, IOnStateChangedToResult {
        [SerializeField] private PhaseManager phaseManager;
        [SerializeField] private GameObject desirePrefab;

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


        private Queue<DesireCore> _desireCores = new();
        private Renderer[] _spawnPointRenderers;

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

                //生成したDesireをキューに入れておく
                _desireCores.Enqueue(desireCore);

                //つくったDesireのOnDisappearを購読して、消えたらまた生成するようにする
                async void OnNext(Unit _) {
                    CollectDesire(desireCore);
                    await UniTask.Delay(TimeSpan.FromSeconds(spawnIntervalSec));
                    SpawnDesire(GetSpawnPosition());
                }

                desireCore.OnDisappear.Subscribe(OnNext);

                //つくったDesireのOnBeAttackedを購読して、
                desireCore.OnBeAttacked.Subscribe(_ => { phaseManager.AddCurrentScoreOnDefeatDesire(); });

                //最初はすべて非アクティブにしておく
                desireObj.SetActive(false);
            }
        }

        private async void Start() {
            //ステージ上にDesireを生成する
            for (var i = 0; i < maxSpawnCount; i++) {
                await UniTask.Delay(TimeSpan.FromSeconds(15f));
                SpawnDesire(GetSpawnPosition());
            }
        }

        #endregion

        /// <summary>
        /// カメラに写っていないspawnPointのうち、最もtargetTransformに近いspawnPointの座標を返す
        /// </summary>
        /// <returns></returns>
        private Vector3 GetSpawnPosition() {
            var notInCameraSpawnPoints = spawnPointObjects
                .Where((t, i) => !_spawnPointRenderers[i].isVisible && canSpawnFlags[i]).ToArray();

            var nearestSpawnPointPos = Vector3.zero;
            for (var i = 0; i < notInCameraSpawnPoints.Length; i++) {
                if (i == 0) {
                    nearestSpawnPointPos = notInCameraSpawnPoints[i].position;
                }
                else {
                    if (Vector3.Distance(targetTransform.position, notInCameraSpawnPoints[i].position) <
                        Vector3.Distance(targetTransform.position, nearestSpawnPointPos))
                        nearestSpawnPointPos = notInCameraSpawnPoints[i].position;
                }
            }

            return nearestSpawnPointPos;
        }

        /// <summary>
        /// 指定した位置にDesireをスポーンさせる(キューから取り出す)
        /// </summary>
        /// <param name="spawnPos"></param>
        private async void SpawnDesire(Vector3 spawnPos) {
            if (_desireCores.Count == 0) return;

            if (_desireCores.Count < maxSpawnCount)
                //もしステージ上に既にDesireが居たら、しばらく待つ
                await UniTask.Delay(TimeSpan.FromSeconds(delaySecWhenDesireExist));
            var desireCore = _desireCores.Dequeue();

            desireCore.gameObject.SetActive(true);
            desireCore.Appear(spawnPos);
        }

        /// <summary>
        /// 引数のDesireCoreをキューに加える
        /// </summary>
        /// <param name="desireCore"></param>
        private void CollectDesire(DesireCore desireCore) {
            _desireCores.Enqueue(desireCore);
        }

        #region interfaceの実装

        public void OnStateChangedToResult() {
            //gameStateがResultに遷移したらどこからもスポーンしなくする
            for (var i = 0; i < canSpawnFlags.Length; i++) {
                canSpawnFlags[i] = false;
            }
        }

        #endregion
    }
}