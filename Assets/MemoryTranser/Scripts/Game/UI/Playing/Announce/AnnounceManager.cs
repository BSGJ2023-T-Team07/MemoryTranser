using System;
using System.Collections.Generic;
using MemoryTranser.Scripts.Game.Util;
using TMPro;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.UI.Playing.Announce {
    public class AnnounceManager : SingletonMonoBehaviour<AnnounceManager> {
        protected override bool DontDestroy => false;

        [SerializeField] private TextMeshProUGUI announceText;
        [SerializeField] private RectTransform announceTextTransform;

        [Header("アナウンスの流れる速さ")] [SerializeField]
        private float announceMoveSpeed;

        private Queue<string> _announceTextQueue = new();

        private const int MAX_ANNOUNCE_TEXT_COUNT = 53;

        private void FixedUpdate() {
            announceTextTransform.position += Vector3.left * announceMoveSpeed;

            if (announceTextTransform.localPosition.x < -3840f) {
                announceTextTransform.localPosition = Vector3.zero;
            }
        }


        public void AddAnnounceText(string text) {
            var textUnit = text + "    ";
            var textUnitLength = textUnit.Length;
            announceText.text = "";

            while (true) {
                if (announceText.text.Length + textUnitLength > MAX_ANNOUNCE_TEXT_COUNT) {
                    break;
                }

                announceText.text += textUnit;
            }
        }
    }
}