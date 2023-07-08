using System.Collections.Generic;
using MemoryTranser.Scripts.Game.Util;
using TMPro;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.UI.Playing.Announce {
    public class AnnounceShower : SingletonMonoBehaviour<AnnounceShower> {
        protected override bool DontDestroy => false;

        [SerializeField] private TextMeshProUGUI announceText0;
        [SerializeField] private RectTransform announceTextTransform;

        [Header("アナウンスの流れる速さ")] [SerializeField]
        private float announceMoveSpeed;

        private Vector3 _defaultAnnounceTextPosition;

        private Queue<string> _announceTextQueue = new();

        private const int MAX_ANNOUNCE_TEXT_COUNT = 53;

        protected override void Awake() {
            base.Awake();
            announceText0.enableWordWrapping = false;

            _defaultAnnounceTextPosition = announceTextTransform.localPosition;
        }

        private void FixedUpdate() {
            announceTextTransform.position += Vector3.left * announceMoveSpeed;

            if (announceTextTransform.localPosition.x < -Constant.SCREEN_WIDTH) {
                announceTextTransform.localPosition = Vector3.zero;
            }
        }


        public void AddAnnounceText(string text) {
            var textUnit = text + "    ";
            var textUnitLength = textUnit.Length;
            announceText0.text = "";

            while (true) {
                if (announceText0.text.Length + textUnitLength > MAX_ANNOUNCE_TEXT_COUNT * 10) {
                    break;
                }

                announceText0.text += textUnit;
            }
        }
    }
}