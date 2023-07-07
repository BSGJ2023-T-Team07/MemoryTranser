using MemoryTranser.Scripts.Game.Phase;
using TMPro;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.UI.Playing {
    public class PhaseTimerShower : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private PhaseManager phaseManager;

        private void Update() {
            text.SetText($"現在のフェイズの残り時間 : {phaseManager.RemainingTime:0}");
        }
    }
}