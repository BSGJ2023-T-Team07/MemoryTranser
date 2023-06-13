using MemoryTranser.Scripts.Game.Phase;
using TMPro;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.UI {
    public class PhaseShower : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI text;

        public void SetPhaseText(PhaseCore[] phaseCores, int currentPhaseIndex) {
            text.text = "";
            for (var i = 0; i < phaseCores.Length; i++) {
                if (i == currentPhaseIndex) {
                    text.text += $"CurrentPhase: {phaseCores[i].QuestType}, Score: {phaseCores[i].Score}\n";
                    continue;
                }

                text.text += $"Phase{i+1}: {phaseCores[i].QuestType}, Score: {phaseCores[i].Score}\n";
            }
        }
    }
}