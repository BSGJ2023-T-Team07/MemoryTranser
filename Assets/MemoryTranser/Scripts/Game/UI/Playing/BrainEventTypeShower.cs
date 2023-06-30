using MemoryTranser.Scripts.Game.BrainEvent;
using MemoryTranser.Scripts.Game.Phase;
using MemoryTranser.Scripts.Game.Util;
using TMPro;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.UI.Playing {
    public class BrainEventTypeShower : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI text;

        public void SetBrainEventTypeText(BrainEventType brainEventType) {
            text.text = $"現在のイベント：{brainEventType.ToJapanese()}";
        }
    }
}