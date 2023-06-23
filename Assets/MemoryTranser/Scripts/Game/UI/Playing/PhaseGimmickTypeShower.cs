using MemoryTranser.Scripts.Game.Phase;
using MemoryTranser.Scripts.Game.Util;
using TMPro;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.UI.Playing {
    public class PhaseGimmickTypeShower : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI text;

        public void SetPhaseGimmickTypeText(PhaseGimmickType phaseGimmickType) {
            text.text = $"現在のギミック：{phaseGimmickType.ToJapanese()}";
        }
    }
}