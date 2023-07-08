using MemoryTranser.Scripts.Game.Phase;
using TMPro;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.UI.Playing {
    public class PhaseTimerShower : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI phaseRemainingTimeText;
        [SerializeField] private TextMeshProUGUI passedPhaseCountText;

        public void SetPhaseRemainingTimeText(float remainingTime) {
            phaseRemainingTimeText.SetText($"{remainingTime:0}");
        }

        public void SetPassedPhaseCountText(int passedPhaseCount) {
            passedPhaseCountText.SetText($"{passedPhaseCount}");
        }
    }
}