using System;
using MemoryTranser.Scripts.Game.Fairy;
using TMPro;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.UI {
    public class FairyParameterShower : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private FairyCore fairyCore;

        private void Update() {
            UpdateFairyText();
        }

        private void UpdateFairyText() {
            text.text = $"State: {fairyCore.MyState}\n" +
                        $"WalkSpeed: {fairyCore.MyParameters.WalkSpeed}\n" +
                        $"ThrowPower: {fairyCore.MyParameters.ThrowPower}\n" +
                        $"ComboCount: {fairyCore.ComboCount}\n";
        }
    }
}