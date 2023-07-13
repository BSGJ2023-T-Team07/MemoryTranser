using System;
using TMPro;
using UnityEngine;

namespace MemoryTranser.Scripts.Title.UI {
    public class TitleSelectionShower : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI startWithIntroText;
        [SerializeField] private TextMeshProUGUI startWithoutIntroText;
        [SerializeField] private TextMeshProUGUI settingsText;
        [SerializeField] private TextMeshProUGUI creditText;
        [SerializeField] private TextMeshProUGUI exitText;

        private void Awake() {
            UpdateTitleSelection(TitleSelection.StartWithIntroduction);
        }

        public void UpdateTitleSelection(TitleSelection selection) {
            switch (selection) {
                case TitleSelection.StartWithIntroduction:
                    startWithIntroText.color = Color.white;
                    startWithoutIntroText.color = Color.gray;
                    settingsText.color = Color.gray;
                    creditText.color = Color.gray;
                    exitText.color = Color.gray;
                    break;
                case TitleSelection.StartWithoutIntroduction:
                    startWithIntroText.color = Color.gray;
                    startWithoutIntroText.color = Color.white;
                    settingsText.color = Color.gray;
                    creditText.color = Color.gray;
                    exitText.color = Color.gray;
                    break;
                case TitleSelection.Settings:
                    startWithIntroText.color = Color.gray;
                    startWithoutIntroText.color = Color.gray;
                    settingsText.color = Color.white;
                    creditText.color = Color.white;
                    exitText.color = Color.gray;
                    break;
                // case TitleSelection.Credit:
                //     startText.color = Color.gray;
                //     creditText.color = Color.white;
                //     exitText.color = Color.gray;
                //     break;
                case TitleSelection.Exit:
                    startWithIntroText.color = Color.gray;
                    startWithoutIntroText.color = Color.gray;
                    settingsText.color = Color.gray;
                    creditText.color = Color.gray;
                    exitText.color = Color.white;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(selection), selection, null);
            }
        }
    }
}