using System;
using System.Collections.Generic;
using MemoryTranser.Scripts.Game.OutputArea;
using MemoryTranser.Scripts.Game.Util;
using UnityEngine;
using Random = UnityEngine.Random;
using UniRx;

namespace MemoryTranser.Scripts.Game.MemoryBox {
    public class MemoryBoxManager : MonoBehaviour {
        #region ゲームオブジェクトの定義

        [SerializeField] private GameObject memoryBoxPrefab;

        #endregion

        #region 変数の定義

        private MemoryBoxCore[] _allBoxes;
        private bool[] _outputable;

        private readonly int _maxBoxType = Enum.GetValues(typeof(BoxMemoryType)).Length;

        #endregion

        #region 定数の定義

        private const int MAX_BOX_GENERATE_COUNT = 20;
        private const float MAX_WEIGHT = 2f;
        private const float MIN_WEIGHT = 0.8f;

        #endregion


        private void GenerateRandomMemoryBox(MemoryBoxCore memoryBox) {
            var randomBoxType = (BoxMemoryType)Random.Range(1, _maxBoxType);
            var randomWeight = Random.Range(MIN_WEIGHT, MAX_WEIGHT);

            //ランダムで科目と重さを決める
            memoryBox.BoxMemoryType = randomBoxType;
            memoryBox.Weight = randomWeight;

            //決まったパラメーターに対して色々変更する
            memoryBox.SpRr.sprite = randomBoxType.ToMemoryBoxSprite();
            memoryBox.transform.localScale *= memoryBox.Weight / (MAX_WEIGHT - MIN_WEIGHT);
            memoryBox.SetDiff();

            //つくったMemoryBoxを監視する
            //このBoxが消えた時に新しく生成するように購読している
            memoryBox.OnDisappear.Subscribe(_ => {
                GenerateRandomMemoryBox(memoryBox);
                memoryBox.transform.position = GetRandomSpawnPosition();
            });
        }


        private Vector3 GetRandomSpawnPosition() {
            var randomX = Random.Range(-15f, 15f);
            var randomY = Random.Range(5f, 10f);
            return new Vector3(randomX, randomY, 0);
        }

        public void InitializeMemoryBoxes() {
            _allBoxes = new MemoryBoxCore[MAX_BOX_GENERATE_COUNT];
            _outputable = new bool[MAX_BOX_GENERATE_COUNT];

            for (var i = 0; i < MAX_BOX_GENERATE_COUNT; i++) {
                //MemoryBoxの生成
                var obj = Instantiate(memoryBoxPrefab, GetRandomSpawnPosition(), Quaternion.identity);
                var memoryBoxCore = obj.GetComponent<MemoryBoxCore>();

                //MemoryBoxの初期化
                GenerateRandomMemoryBox(memoryBoxCore);

                //IDの生成
                //_allBoxesのインデックスがIDとなる
                memoryBoxCore.BoxId = i;

                //MemoryBoxの監視
                _allBoxes[i] = memoryBoxCore;
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
            for (var i = 0; i < MAX_BOX_GENERATE_COUNT; i++) {
                if (_outputable[i]) outputableBoxes.Add(_allBoxes[i]);
            }

            return outputableBoxes.ToArray();
        }
    }
}