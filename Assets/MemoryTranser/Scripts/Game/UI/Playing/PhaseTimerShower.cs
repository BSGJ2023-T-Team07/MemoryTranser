using System;
using MemoryTranser.Scripts.Game.Phase;
using TMPro;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.UI {
    public class PhaseTimerShower : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private PhaseManager phaseManager;

        private void Update() {
            text.text = $"PhaseRemainingTime : {phaseManager.RemainingTime:0}";
        }
    }
}