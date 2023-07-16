using System.Linq;
using MemoryTranser.Scripts.Game.MemoryBox;
using TMPro;
using UnityEngine;
using MemoryTranser.Scripts.Game.Util;

namespace MemoryTranser.Scripts.Game.UI.Debug {
    public class BoxTypeProbabilityShower : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI text;
        private const string FIRST = "MemoryBoxの科目別抽選確率\n";

        public void SetBoxTypeProbabilityText(float[] weights) {
            var probabilityArray = CalculateProbability(weights);

            text.text = FIRST;
            for (var i = 0; i < (int)BoxMemoryType.Count; i++) {
                text.text +=
                    $"{((BoxMemoryType)i).ToJapanese()}: {Mathf.Floor(probabilityArray[i] * 1000) / 10}%, ";
                if (i % 2 == 1) {
                    text.text += "\n";
                }
            }
        }

        private static float[] CalculateProbability(float[] weights) {
            var probabilityArray = weights.Select(x => x / weights.Sum()).ToArray();
            return probabilityArray;
        }
    }
}