using MemoryTranser.Scripts.Game.BrainEvent;
using MemoryTranser.Scripts.Game.Util;
using TMPro;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.UI.Playing.Announce {
    public class BrainEventTypeShower : MonoBehaviour {
        public void SetBrainEventTypeText(BrainEventType brainEventType) {
            AnnounceManager.I.AddAnnounceText($"現在のイベント：{brainEventType.ToJapanese()}");
        }
    }
}