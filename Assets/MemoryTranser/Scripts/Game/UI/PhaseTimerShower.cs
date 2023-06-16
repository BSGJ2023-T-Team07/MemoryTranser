using System;
using MemoryTranser.Scripts.Game.Phase;
using TMPro;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.UI {
    public class PhaseTimerShower : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private PhaseManager phaseManager;

        private void Update() {
            text.text = $"Time : {phaseManager.RemainingTime:0}";
        }
    }
}