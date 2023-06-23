using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MemoryTranser.Scripts.Game.Desire;
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

            //ステージ上のDesireの数が0になったら、消えていたMemoryBoxをAppearさせるeventを購読する
            desireManager.ExistingDesireCount.Where(x => x == 0).Subscribe(_ => {
                for (var i = 0; i < _appliedDisappearedBoxes.Count; i++) {
                    var box = _appliedDisappearedBoxes.Dequeue();
                    MakeBoxAppear(box);
                }
            });
        }

        #endregion

        private static void MakeBoxAppear(MemoryBoxCore box) {
            Debug.Log($"ID{box.BoxId}のMemoryBoxをAppearさせます");
            box.SpRr.enabled = true;
            box.Bc2D.isTrigger = false;
            box.Bc2D.enabled = true;
            box.MyState = MemoryBoxState.PlacedOnLevel;
        }


        private void ApplyRandomParameterForMemoryBox(MemoryBoxCore memoryBox) {
            var randomBoxType =
                (BoxMemoryType)_initialBoxTypeProbabilityList[Random.Range(0, _initialBoxTypeProbabilityList.Count)];
            var randomWeight = Random.Range(minWeight, maxWeight);

            //ランダムで科目と重さを決める
            memoryBox.BoxMemoryType = randomBoxType;
            memoryBox.Weight = randomWeight;

            //決定したパラメーターを反映させる
            memoryBox.SpRr.sprite = randomBoxType.ToMemoryBoxSprite();
            memoryBox.transform.localScale = Vector3.one * memoryBox.Weight / 1.2f;

            //コンポーネントのプロパティー初期化
            memoryBox.Bc2D.isTrigger = false;

            async void OnNext(Unit _) {
                //MemoryBoxが消えてからは一定時間処理を待機
                await UniTask.Delay(TimeSpan.FromSeconds(generateIntervalSec));

                //待機後、MemoryBoxをAppearさせる準備を完了する
                ApplyRandomParameterForMemoryBox(memoryBox);
                memoryBox.transform.position = GetRandomSpawnPosition();
                _appliedDisappearedBoxes.Enqueue(memoryBox);

                //もしDesireが存在しないなら、MemoryBoxをAppearさせる
                if (desireManager.ExistingDesireCount.Value == 0) MakeBoxAppear(memoryBox);
            }

            //↑の処理をMemoryBoxがDisappearするときのイベントに登録する
            memoryBox.OnDisappear.Subscribe(OnNext);
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

                //MemoryBoxの初期化
                ApplyRandomParameterForMemoryBox(memoryBoxCore);

                //IDの生成
                //_allBoxesのインデックスがIDとなる
                memoryBoxCore.BoxId = i;

                //MemoryBoxの監視
                _allBoxes[i] = memoryBoxCore;
            }
        }

        private void InitializeGenerationProbability() {
            for (var i = 1; i <= (int)BoxMemoryType.Count; i++) {
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
                if (_outputable[i]) outputableBoxes.Add(_allBoxes[i]);
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