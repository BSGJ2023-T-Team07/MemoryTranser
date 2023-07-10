using System;
using MemoryTranser.Scripts.Game.Fairy;
using MemoryTranser.Scripts.Game.GameManagers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MemoryTranser.Scripts.Game.UI.Playing {
    public class BlinkTicketCountShower : MonoBehaviour, IOnGameAwake {
        [SerializeField] private FairyCore fairyCore;

        [SerializeField] private GameObject blinkTicketCountObjectsParent;
        [SerializeField] private Image[] blinkTicketCountImages;

        private void OnValidate() {
            if (blinkTicketCountImages == null || !blinkTicketCountObjectsParent) {
                return;
            }

            blinkTicketCountImages = blinkTicketCountObjectsParent.GetComponentsInChildren<Image>();
        }

        public void OnGameAwake() {
            SetBlinkTicketCountShow(0);
        }

        public void SetBlinkTicketCountShow(int blinkTicketCount) {
            for (var i = 0; i < fairyCore.MaxBlinkTicketCount; i++) {
                blinkTicketCountImages[i].enabled = i < blinkTicketCount;
            }
        }
    }
}