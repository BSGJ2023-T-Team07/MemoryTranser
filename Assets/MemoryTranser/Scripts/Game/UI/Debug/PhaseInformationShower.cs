using System;
using MemoryTranser.Scripts.Game.Phase;
using TMPro;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.UI.Debug {
    public class PhaseInformationShower : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private PhaseManager phaseManager;

        private void SetPhaseText((PhaseCore[], int) information) {
            var phaseCores = information.Item1;
            var currentPhaseIndex = information.Item2;
            text.text = "";
            for (var i = 0; i < phaseCores.Length; i++) {
                if (i == currentPhaseIndex) {
                    text.text += $"CurrentPhase: {phaseCores[i].QuestType}, Score: {phaseCores[i].Score}\n";
                    continue;
                }

                text.text += $"Phase{i + 1}: {phaseCores[i].QuestType}, Score: {phaseCores[i].Score}\n";
            }
        }

        private void Update() {
            SetPhaseText(phaseManager.GetPhaseInformation());
        }
    }
}