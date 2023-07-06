using System;
using TMPro;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.UI {
    public class MenuShower : MonoBehaviour {
        [SerializeField] private GameObject pauseBackGround;

        [SerializeField] private GameObject menuLayer;

        [SerializeField] private TextMeshProUGUI resumeText;
        [SerializeField] private TextMeshProUGUI backToTitleText;

        private void Awake() {
            pauseBackGround.SetActive(false);
            menuLayer.SetActive(false);
        }

        public void ToggleMenu(bool isActive) {
            pauseBackGround.SetActive(isActive);
            menuLayer.SetActive(isActive);
        }

        public void UpdateMenuSelectionShow(MenuSelection selection) {
            switch (selection) {
                case MenuSelection.Resume:
                    resumeText.color = Color.white;
                    backToTitleText.color = Color.gray;
                    break;
                case MenuSelection.BackToTitle:
                    resumeText.color = Color.gray;
                    backToTitleText.color = Color.white;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(selection), selection, null);
            }
        }
    }
}