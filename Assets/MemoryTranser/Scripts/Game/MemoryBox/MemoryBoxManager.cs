using System;
using System.Collections.Generic;
using MemoryTranser.Scripts.Game.GameManagers;
using MemoryTranser.Scripts.Game.UI.Debug;
using MemoryTranser.Scripts.Game.Util;
using UnityEngine;
using Random = UnityEngine.Random;
using UniRx;
using UnityEngine.Serialization;

namespace MemoryTranser.Scripts.Game.MemoryBox {
    public class MemoryBoxManager : MonoBehaviour {
        #region ゲームオブジェクトの定義

        [SerializeField] private GameObject memoryBoxPrefab;
        [SerializeField] private MemoryGenerationProbabilityShower memoryGenerationProbabilityShower;

        #endregion

        #region 変数の定義

        private MemoryBoxCore[] _allBoxes;
        private bool[] _outputable;

        private List<int> _initialBoxTypeProbabilityList = new();
        private List<int> _boxTypeProbabilityList = new();

        [Header("MemoryBoxの最大生成数")] [SerializeField]
        private int maxBoxGenerateCount = 20;

        [Header("MemoryBoxの重さの最大値")] [SerializeField]
        private float maxWeight = 2f;

        [Header("MemoryBoxの重さの最小値")] [SerializeField]
        private float minWeight = 0.8f;

        #endregion

        #region Unityから呼ばれる

        private void Awake() {
            _outputable = new bool[maxBoxGenerateCount];
            InitializeGenerationProbability();
        }

        #endregion


        private void ApplyRandomParameterForMemoryBox(MemoryBoxCore memoryBox) {
            var randomBoxType =
                (BoxMemoryType)_initialBoxTypeProbabilityList[Random.Range(0, _initialBoxTypeProbabilityList.Count)];
            var randomWeight = Random.Range(minWeight, maxWeight);

            //ランダムで科目と重さを決める
            memoryBox.BoxMemoryType = randomBoxType;
            memoryBox.Weight = randomWeight;

            //決まったパラメーターに対して色々変更する
            memoryBox.SpRr.sprite = randomBoxType.ToMemoryBoxSprite();
            memoryBox.transform.localScale = Vector3.one * memoryBox.Weight / 1.2f;
            memoryBox.SetDiff();

            //コンポーネントのプロパティー初期化
            memoryBox.Bc2D.isTrigger = false;

            //つくったMemoryBoxを監視する
            //このBoxが消えた時に新しく生成するように購読している
            memoryBox.OnDisappear.Subscribe(_ => {
                ApplyRandomParameterForMemoryBox(memoryBox);
                memoryBox.transform.position = GetRandomSpawnPosition();
            });
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