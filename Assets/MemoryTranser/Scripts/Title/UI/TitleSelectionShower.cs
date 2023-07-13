using System;
using TMPro;
using UnityEngine;

namespace MemoryTranser.Scripts.Title.UI {
    public class TitleSelectionShower : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI startText;
        [SerializeField] private TextMeshProUGUI creditText;
        [SerializeField] private TextMeshProUGUI exitText;

        private void Awake() {
            UpdateTitleSelection(TitleSelection.Start);
        }

        public void UpdateTitleSelection(TitleSelection selection) {
            switch (selection) {
                case TitleSelection.Start:
                    startText.color = Color.white;
                    creditText.color = Color.gray;
                    exitText.color = Color.gray;
                    break;
                case TitleSelection.Credit:
                    startText.color = Color.gray;
                    creditText.color = Color.white;
                    exitText.color = Color.gray;
                    break;
                case TitleSelection.Exit:
                    startText.color = Color.gray;
                    creditText.color = Color.gray;
                    exitText.color = Color.white;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(selection), selection, null);
            }
        }
    }
}