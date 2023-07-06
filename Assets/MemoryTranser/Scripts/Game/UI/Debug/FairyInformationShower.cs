using System;
using MemoryTranser.Scripts.Game.Fairy;
using TMPro;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.UI.Debug {
    public class FairyInformationShower : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private FairyCore fairyCore;

        private string _core;

        private void Awake() {
            _core = $"<size={text.fontSize + 5}>Fairyの情報</size>\n";
            text.text = _core;
        }

        private void Update() {
            UpdateFairyText();
        }

        private void UpdateFairyText() {
            text.text = $"{_core}" + $"State: {fairyCore.MyState}\n" +
                        $"WalkSpeed: {Mathf.Floor(fairyCore.MyParameters.WalkSpeed * 100) / 100}\n" +
                        $"ThrowPower: {Mathf.Floor(fairyCore.MyParameters.ThrowPower * 100) / 100}\n" +
                        $"ComboCount: {fairyCore.CurrentComboCount}\n" +
                        $"BlinkTicketCount: {fairyCore.BlinkTicketCount}\n" +
                        $"InputVelocity: {fairyCore.InputWalkDirection}\n" +
                        $"InputVelocityBeforeZero: {fairyCore.InputWalkDirectionBeforeZero}\n" +
                        $"IsBlinking: {fairyCore.IsBlinking}\n" +
                        $"IsBlinkRecovered: {fairyCore.IsBlinkRecovered}\n" +
                        $"IsTouchingSphereBox: {fairyCore.ApplyCancelingBlink}\n" +
                        $"IsControllable: {fairyCore.IsControllable}\n" +
                        $"NowInputSecToOutput: {Mathf.Floor(fairyCore.NowInputSecToOutput * 100) / 100}\n";
        }
    }
}