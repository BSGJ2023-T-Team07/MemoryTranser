using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MemoryTranser.Scripts.Game.Desire;
using MemoryTranser.Scripts.Game.Phase;
using MemoryTranser.Scripts.Game.UI.Debug;
using MemoryTranser.Scripts.Game.Util;
using UnityEngine;
using Random = UnityEngine.Random;
using UniRx;

namespace MemoryTranser.Scripts.Game.MemoryBox {
    public class MemoryBoxManager : MonoBehaviour {
        #region ゲームオブジェクトの定義

        [SerializeField] private GameObject memoryBoxPrefab;
        [SerializeField] private MemoryGenerationProbabilityShower memoryGenerationProbabilityShower;
        [SerializeField] private DesireManager desireManager;
        [SerializeField] private PhaseManager phaseManager;

        #endregion

        #region 変数の定義

        [Header("MemoryBoxの最大生成数")] [SerializeField]
        private int maxBoxGenerateCount = 20;

        [Header("MemoryBoxの重さの最大値")] [SerializeField]
        private float maxWeight = 2f;

        [Header("MemoryBoxの重さの最小値")] [SerializeField]
        private float minWeight = 0.8f;

        [Header("MemoryBoxが消えてから再生成されるまでの時間(秒)")] [SerializeField]
        private float generateIntervalSec = 3f;

        private MemoryBoxCore[] _allBoxes;
        private Queue<MemoryBoxCore> _appliedDisappearedBoxes = new();
        private bool[] _outputable;

        private List<int> _initialBoxTypeProbabilityList = new();
        private List<int> _boxTypeProbabilityList = new();

        #endregion

        #region Unityから呼ばれる

        private void Awake() {
            _outputable = new bool[maxBoxGenerateCount];
            InitializeGenerationProbability();

            //ステージ上のDesireの数が0になったら消えていたMemoryBoxをAppearさせる、というeventを購読する
            desireManager.ExistingDesireCount.Where(x => x == 0).Subscribe(_ => {
                for (var i = 0; i < _appliedDisappearedBoxes.Count; i++) {
                    var box = _appliedDisappearedBoxes.Dequeue();
                    MakeBoxAppear(box);
                    if (phaseManager.OnPhaseTransition.Value == PhaseGimmickType.Blind) {
                        box.SmokeParticle.Play();
                    }
                }
            });
        }

        #endregion

        private static void MakeBoxAppear(MemoryBoxCore box) {
            Debug.Log($"ID{box.BoxId}のMemoryBoxをAppearさせます");
            box.SpRr.enabled = true;
            box.Cc2D.isTrigger = false;
            box.Cc2D.enabled = true;
            box.MyState = MemoryBoxState.PlacedOnLevel;
        }


        private MemoryBoxCore ApplyRandomParameterForMemoryBox(MemoryBoxCore memoryBoxCore) {
            //ランダムにパラメーターを決める
            var randomBoxType =
                (BoxMemoryType)_initialBoxTypeProbabilityList[Random.Range(0, _initialBoxTypeProbabilityList.Count)];
            var randomWeight = Random.Range(minWeight, maxWeight);
            var randomBoxShape = (MemoryBoxShapeType)Random.Range(0, (int)MemoryBoxShapeType.Count);

            //決定したパラメーターを代入する
            memoryBoxCore.BoxMemoryType = randomBoxType;
            memoryBoxCore.BoxShapeType = randomBoxShape;
            memoryBoxCore.Weight = randomWeight;

            //決定したパラメーターから他の値に反映させる
            memoryBoxCore.SpRr.sprite = randomBoxType.ToMemoryBoxSprite(randomBoxShape);
            memoryBoxCore.transform.localScale = Vector3.one * memoryBoxCore.Weight / 1.2f;
            memoryBoxCore.Cc2D.isTrigger = false;
            memoryBoxCore.gameObject.layer = LayerMask.NameToLayer($"{randomBoxShape.ToString()}MemoryBox");

            return memoryBoxCore;
        }


        private static Vector3 GetRandomSpawnPosition() {
            var randomX = Random.Range(-15f, 15f);
            var randomY = Random.Range(5f, 10f);
            return new Vector3(randomX, randomY, 0);
        }

        public void GenerateMemoryBoxes() {
            _allBoxes = new MemoryBoxCore[maxBoxGenerateCount];

            for (var i = 0; i < maxBoxGenerateCount; i++) {
                //MemoryBoxの生成
                var obj = Instantiate(memoryBoxPrefab, GetRandomSpawnPosition(), Quaternion.identity);
                var memoryBoxCore = obj.GetComponent<MemoryBoxCore>();

                //MemoryBoxにランダムなパラメーターを設定する
                ApplyRandomParameterForMemoryBox(memoryBoxCore);

                //MemoryBoxがDisappearするときのイベントに登録する
                memoryBoxCore.OnDisappear.Subscribe(async _ => {
                    //MemoryBoxが消えてからは一定時間処理を待機
                    await UniTask.Delay(TimeSpan.FromSeconds(generateIntervalSec));

                    //待機後、MemoryBoxをAppearさせる準備を完了する
                    ApplyRandomParameterForMemoryBox(memoryBoxCore);
                    memoryBoxCore.transform.position = GetRandomSpawnPosition();

                    //もしDesireが存在しないなら、MemoryBoxをAppearさせる
                    if (desireManager.ExistingDesireCount.Value == 0) {
                        MakeBoxAppear(memoryBoxCore);
                        if (phaseManager.OnPhaseTransition.Value == PhaseGimmickType.Blind) {
                            memoryBoxCore.SmokeParticle.Play();
                        }
                    }
                    else {
                        _appliedDisappearedBoxes.Enqueue(memoryBoxCore);
                    }
                });

                //IDの生成
                //_allBoxesのインデックスがIDとなる
                memoryBoxCore.BoxId = i;

                //MemoryBoxの監視
                _allBoxes[i] = memoryBoxCore;
            }

            //Phaseが遷移したら煙のパーティクルを止めるイベントを購読する
            phaseManager.OnPhaseTransition.Subscribe(_ => {
                foreach (var box in _allBoxes) {
                    box.SmokeParticle.Stop();
                }
            });

            //遷移したPhaseがド忘れなら煙のパーティクルを再生するイベントを購読する
            phaseManager.OnPhaseTransition.Where(x => x == PhaseGimmickType.Blind).Subscribe(_ => {
                foreach (var box in _allBoxes) {
                    if (box.MyState != MemoryBoxState.Disappeared) {
                        box.SmokeParticle.Play();
                    }
                }
            });
        }

        private void InitializeGenerationProbability() {
            for (var i = 1; i < (int)BoxMemoryType.Count; i++) {
                //BoxTypeの確率リストを作成
                _initialBoxTypeProbabilityList.Add(i);
                _boxTypeProbabilityList.Add(i);
            }
        }

        public void AddOutputableId(int boxId) {
            _outputable[boxId] = true;
        }

        public void RemoveOutputableId(int boxId) {
            _outputable[boxId] = false;
        }


        public MemoryBoxCore[] GetOutputableBoxes() {
            var outputableBoxes = new List<MemoryBoxCore>();
            for (var i = 0; i < maxBoxGenerateCount; i++) {
                if (_outputable[i]) {
                    outputableBoxes.Add(_allBoxes[i]);
                }
            }

            return outputableBoxes.ToArray();
        }

        public void SetProbabilityList(List<int> probabilityList) {
            _boxTypeProbabilityList = _initialBoxTypeProbabilityList;
            _boxTypeProbabilityList.AddRange(probabilityList);

            memoryGenerationProbabilityShower.SetMemoryGenerationProbabilityText(_boxTypeProbabilityList);
        }
    }
}