using System;
using System.Collections.Generic;
using MemoryTranser.Scripts.Game.MemoryBox;
using TMPro;
using UnityEngine;
using MemoryTranser.Scripts.Game.Util;

namespace MemoryTranser.Scripts.Game.UI.Debug {
    public class MemoryGenerationProbabilityShower : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI text;
        private const string FIRST = "MemoryBoxの科目別抽選確率\n";

        public void SetMemoryGenerationProbabilityText(List<int> memoryGenerationList) {
            var probabilityArray = CalculateProbability(memoryGenerationList);

            text.text = FIRST;
            for (var i = 1; i < (int)BoxMemoryType.Count; i++) {
                text.text +=
                    $"{((BoxMemoryType)i).ToJapanese()}: {Mathf.Floor(probabilityArray[i] * 1000) / 10}%\n";
            }
        }

        private static float[] CalculateProbability(List<int> memoryGenerationList) {
            var memoryIntArray = new int[(int)BoxMemoryType.Count];
            var probabilityArray = new float[(int)BoxMemoryType.Count];

            for (var i = 0; i < (int)BoxMemoryType.Count; i++) {
                foreach (var type in memoryGenerationList) {
                    if (type == i) memoryIntArray[i]++;
                }
            }

            for (var i = 0; i < (int)BoxMemoryType.Count; i++) {
                probabilityArray[i] = (float)memoryIntArray[i] / memoryGenerationList.Count;
            }

            return probabilityArray;
        }
    }
}