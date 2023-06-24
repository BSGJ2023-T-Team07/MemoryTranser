using System;
using MemoryTranser.Scripts.Game.Desire;
using MemoryTranser.Scripts.Game.Util;
using TMPro;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.UI.Debug {
    public class DesireInformationShower : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI text;
        private string _core = "出現中の煩悩の情報";
        private float _defaultFontSize;

        private void Awake() {
            _defaultFontSize = text.fontSize;
            _core = $"<size={_defaultFontSize + 5}>{_core}</size>\n";

            text.text = _core;
            text.text += "出現中の煩悩の数: 0\n";
        }

        public void SetDesireInformationText(DesireCore[] desireCores) {
            text.text = _core;
            text.text += $"出現中の煩悩の数: {desireCores.Length}\n";

            for (var i = 0; i < desireCores.Length; i++) {
                text.text += $"煩悩{i + 1}: {desireCores[i].MyType.ToJapanese()}\n";
            }
        }
    }
}