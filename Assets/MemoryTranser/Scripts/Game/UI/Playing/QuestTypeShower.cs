using System;
using MemoryTranser.Scripts.Game.GameManagers;
using MemoryTranser.Scripts.Game.MemoryBox;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MemoryTranser.Scripts.Game.UI.Playing {
    public class QuestTypeShower : MonoBehaviour {
        #region コンポーネントの定義

        [Header("色付き文字が通常文字よりどれだけ大きいか")] [SerializeField]
        private float additionalFontSize;

        [SerializeField] private GameObject nextQuestTypeObject;
        [SerializeField] private TextMeshProUGUI nextQuestTypeText;

        [SerializeField] private GameObject currentQuestTypeObject;
        [SerializeField] private TextMeshProUGUI currentQuestTypeText;

        private float _defaultFontSize;

        #endregion

        #region 変数の定義

        //QuestTextの文章を設定
        [SerializeField] private QuestText[] mathTexts;
        [SerializeField] private QuestText[] japaneseTexts;
        [SerializeField] private QuestText[] englishTexts;
        [SerializeField] private QuestText[] lifeTexts;
        [SerializeField] private QuestText[] moralTexts;
        [SerializeField] private QuestText[] scienceTexts;
        [SerializeField] private QuestText[] triviaTexts;
        [SerializeField] private QuestText[] musicTexts;
        [SerializeField] private QuestText[] socialStudiesTexts;

        #endregion

        private void Awake() {
            nextQuestTypeText.text = "";
            currentQuestTypeText.text = "";

            _defaultFontSize = currentQuestTypeText.fontSize;
        }

        public void InitializeQuestText(BoxMemoryType currentMemoryType, BoxMemoryType nextMemoryType) {
            var nextQuestText = GetRandomQuestText(nextMemoryType);
            var nextQuestTextPainted =
                nextQuestText.GetPaintedText(nextMemoryType, _defaultFontSize, additionalFontSize);
            nextQuestTypeText.text = nextQuestTextPainted;

            var currentQuestText = GetRandomQuestText(currentMemoryType);
            var currentQuestTextPainted =
                currentQuestText.GetPaintedText(currentMemoryType, _defaultFontSize, additionalFontSize);
            currentQuestTypeText.text = currentQuestTextPainted;
        }

        public void UpdateQuestText(BoxMemoryType nextMemoryType) {
            currentQuestTypeText.text = nextQuestTypeText.text;

            var nextQuestText = GetRandomQuestText(nextMemoryType);
            var nextQuestTextPainted =
                nextQuestText.GetPaintedText(nextMemoryType, _defaultFontSize, additionalFontSize);
            nextQuestTypeText.text = nextQuestTextPainted;
        }

        private QuestText GetRandomQuestText(BoxMemoryType memoryType) {
            return memoryType switch {
                BoxMemoryType.Math => mathTexts[Random.Range(0, mathTexts.Length)],
                BoxMemoryType.Japanese => japaneseTexts[Random.Range(0, japaneseTexts.Length)],
                BoxMemoryType.English => englishTexts[Random.Range(0, englishTexts.Length)],
                BoxMemoryType.SocialStudies => socialStudiesTexts[Random.Range(0, socialStudiesTexts.Length)],
                BoxMemoryType.Science => scienceTexts[Random.Range(0, scienceTexts.Length)],
                BoxMemoryType.Trivia => triviaTexts[Random.Range(0, triviaTexts.Length)],
                BoxMemoryType.Moral => moralTexts[Random.Range(0, moralTexts.Length)],
                BoxMemoryType.Music => musicTexts[Random.Range(0, musicTexts.Length)],
                BoxMemoryType.Life => lifeTexts[Random.Range(0, lifeTexts.Length)]
            };
        }
    }

    [Serializable]
    public class QuestText {
        [Tooltip("出だしの文")] public string startText;
        [Tooltip("科目に関連する文")] public string mainText;
        [Tooltip("締めの文")] public string finalText;

        /// <summary>
        /// 引数の記憶の色にテキストを塗る
        /// </summary>
        /// <param name="memoryType"></param>
        /// <param name="defaultFontSize"></param>
        /// <param name="additionalFontSize"></param>
        /// <returns></returns>
        public string GetPaintedText(BoxMemoryType memoryType, float defaultFontSize, float additionalFontSize) {
            return
                $"{startText}<material={MemoryColorManager.I.GetTextMaterialName(memoryType)}><size={defaultFontSize + additionalFontSize}>{mainText}</size></material>{finalText}";
        }
    }
}