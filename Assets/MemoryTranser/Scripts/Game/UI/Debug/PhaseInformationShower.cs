using System;
using MemoryTranser.Scripts.Game.Phase;
using MemoryTranser.Scripts.Game.Util;
using TMPro;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.UI.Debug {
    public class PhaseInformationShower : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private PhaseManager phaseManager;

        private float _defaultFontSize;


        private void Start() {
            _defaultFontSize = text.fontSize;
        }

        private void SetPhaseText((PhaseCore[], int, int) information) {
            var phaseCores = information.Item1;
            var currentPhaseIndex = information.Item2;
            var viewablePhaseCount = information.Item3;
            text.text = "";
            for (var i = 0; i < phaseCores.Length; i++) {
                if (i == currentPhaseIndex) {
                    text.text +=
                        $"<size={_defaultFontSize + 10}>フェイズ{i + 1}: {phaseCores[i].QuestType.ToJapanese()}, 点数: {phaseCores[i].Score}</size>\n";
                    continue;
                }

                if (i > currentPhaseIndex && i <= currentPhaseIndex + viewablePhaseCount) {
                    text.text +=
                        $"<size={_defaultFontSize + 5}>フェイズ{i + 1}: {phaseCores[i].QuestType.ToJapanese()}, 点数: {phaseCores[i].Score}</size>\n";
                    continue;
                }

                text.text += $"フェイズ{i + 1}: {phaseCores[i].QuestType.ToJapanese()}, 点数: {phaseCores[i].Score}\n";
            }
        }

        private void Update() {
            SetPhaseText(phaseManager.GetPhaseInformation());
        }
    }
}