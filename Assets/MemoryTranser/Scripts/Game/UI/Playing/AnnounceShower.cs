using DG.Tweening;
using MemoryTranser.Scripts.Game.BrainEvent;
using MemoryTranser.Scripts.Game.Util;
using TMPro;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.UI.Playing {
    public class AnnounceShower : SingletonMonoBehaviour<AnnounceShower> {
        protected override bool DontDestroy => false;

        [SerializeField] private Transform announceTextParent;
        [SerializeField] private TextMeshProUGUI announcePrefab;

        [Header("アナウンスの流れる速さ")] [SerializeField]
        private float announceMoveSpeed;

        private Vector3 _defaultAnnounceTextPosition;

        private Sequence _announceSequence;

        protected override void Awake() {
            base.Awake();
            announcePrefab.enableWordWrapping = false;
        }

        private void AddAnnounceText(string text, float durationSec) {
            var additionAnnounceText = Instantiate(announcePrefab, announceTextParent);
            additionAnnounceText.SetText(text);
        }

        public void SetBrainEventTypeText(BrainEventType brainEventType, float durationSec) {
            if (brainEventType == BrainEventType.None) {
                return;
            }

            AddAnnounceText($"{brainEventType.ToJapanese()}！！", durationSec);
        }
    }
}