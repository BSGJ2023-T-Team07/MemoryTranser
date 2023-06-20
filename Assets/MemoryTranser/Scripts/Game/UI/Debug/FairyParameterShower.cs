using MemoryTranser.Scripts.Game.Fairy;
using TMPro;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.UI.Debug {
    public class FairyParameterShower : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private FairyCore fairyCore;

        private void Update() {
            UpdateFairyText();
        }

        private void UpdateFairyText() {
            text.text = $"State: {fairyCore.MyState}\n" +
                        $"WalkSpeed: {Mathf.Floor(fairyCore.MyParameters.WalkSpeed * 100) / 100}\n" +
                        $"ThrowPower: {Mathf.Floor(fairyCore.MyParameters.ThrowPower * 100) / 100}\n" +
                        $"ComboCount: {fairyCore.ComboCount}\n";
        }
    }
}