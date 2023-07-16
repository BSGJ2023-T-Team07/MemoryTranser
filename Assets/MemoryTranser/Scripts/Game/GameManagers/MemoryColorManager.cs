using System;
using MemoryTranser.Scripts.Game.MemoryBox;
using MemoryTranser.Scripts.Game.Util;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.GameManagers {
    public class MemoryColorManager : SingletonMonoBehaviour<MemoryColorManager> {
        protected override bool DontDestroy => true;

        //記憶の色を設定
        [SerializeField] private string mathMainColorCode;
        [SerializeField] private string mathSubColorCode;
        [SerializeField] private Material mathTextMaterial;

        [Space] [SerializeField] private string japaneseMainColorCode;
        [SerializeField] private string japaneseSubColorCode;
        [SerializeField] private Material japaneseTextMaterial;

        [Space] [SerializeField] private string englishMainColorCode;
        [SerializeField] private string englishSubColorCode;
        [SerializeField] private Material englishTextMaterial;

        [Space] [SerializeField] private string socialStudiesMainColorCode;
        [SerializeField] private string socialStudiesSubColorCode;
        [SerializeField] private Material socialStudiesTextMaterial;

        [Space] [SerializeField] private string scienceMainColorCode;
        [SerializeField] private string scienceSubColorCode;
        [SerializeField] private Material scienceTextMaterial;

        [Space] [SerializeField] private string triviaMainColorCode;
        [SerializeField] private string triviaSubColorCode;
        [SerializeField] private Material triviaTextMaterial;

        [Space] [SerializeField] private string moralMainColorCode;
        [SerializeField] private string moralSubColorCode;
        [SerializeField] private Material moralTextMaterial;

        [Space] [SerializeField] private string musicMainColorCode;
        [SerializeField] private string musicSubColorCode;
        [SerializeField] private Material musicTextMaterial;

        [Space] [SerializeField] private string lifeMainColorCode;
        [SerializeField] private string lifeSubColorCode;
        [SerializeField] private Material lifeTextMaterial;

        private static readonly int FaceColor = Shader.PropertyToID("_FaceColor");
        private static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");

#if UNITY_EDITOR
        private void OnValidate() {
            mathTextMaterial.SetColor(FaceColor,
                ColorUtility.TryParseHtmlString($"#{mathMainColorCode}", out var color) ? color : Color.white);
            mathTextMaterial.SetColor(OutlineColor,
                ColorUtility.TryParseHtmlString($"#{mathSubColorCode}", out color) ? color : Color.white);

            japaneseTextMaterial.SetColor(FaceColor,
                ColorUtility.TryParseHtmlString($"#{japaneseMainColorCode}", out color) ? color : Color.white);
            japaneseTextMaterial.SetColor(OutlineColor,
                ColorUtility.TryParseHtmlString($"#{japaneseSubColorCode}", out color) ? color : Color.white);

            englishTextMaterial.SetColor(FaceColor,
                ColorUtility.TryParseHtmlString($"#{englishMainColorCode}", out color) ? color : Color.white);
            englishTextMaterial.SetColor(OutlineColor,
                ColorUtility.TryParseHtmlString($"#{englishSubColorCode}", out color) ? color : Color.white);

            socialStudiesTextMaterial.SetColor(FaceColor,
                ColorUtility.TryParseHtmlString($"#{socialStudiesMainColorCode}", out color) ? color : Color.white);
            socialStudiesTextMaterial.SetColor(OutlineColor,
                ColorUtility.TryParseHtmlString($"#{socialStudiesSubColorCode}", out color) ? color : Color.white);

            scienceTextMaterial.SetColor(FaceColor,
                ColorUtility.TryParseHtmlString($"#{scienceMainColorCode}", out color) ? color : Color.white);
            scienceTextMaterial.SetColor(OutlineColor,
                ColorUtility.TryParseHtmlString($"#{scienceSubColorCode}", out color) ? color : Color.white);

            triviaTextMaterial.SetColor(FaceColor,
                ColorUtility.TryParseHtmlString($"#{triviaMainColorCode}", out color) ? color : Color.white);
            triviaTextMaterial.SetColor(OutlineColor,
                ColorUtility.TryParseHtmlString($"#{triviaSubColorCode}", out color) ? color : Color.white);

            moralTextMaterial.SetColor(FaceColor,
                ColorUtility.TryParseHtmlString($"#{moralMainColorCode}", out color) ? color : Color.white);
            moralTextMaterial.SetColor(OutlineColor,
                ColorUtility.TryParseHtmlString($"#{moralSubColorCode}", out color) ? color : Color.white);

            musicTextMaterial.SetColor(FaceColor,
                ColorUtility.TryParseHtmlString($"#{musicMainColorCode}", out color) ? color : Color.white);
            musicTextMaterial.SetColor(OutlineColor,
                ColorUtility.TryParseHtmlString($"#{musicSubColorCode}", out color) ? color : Color.white);

            lifeTextMaterial.SetColor(FaceColor,
                ColorUtility.TryParseHtmlString($"#{lifeMainColorCode}", out color) ? color : Color.white);
            lifeTextMaterial.SetColor(OutlineColor,
                ColorUtility.TryParseHtmlString($"#{lifeSubColorCode}", out color) ? color : Color.white);
        }
#endif

        public static string GetTextMaterialName(BoxMemoryType memoryType) {
            return memoryType switch {
                BoxMemoryType.Math => Constant.MATH_TEXT_MATERIAL_NAME,
                BoxMemoryType.Japanese => Constant.JAPANESE_TEXT_MATERIAL_NAME,
                BoxMemoryType.English => Constant.ENGLISH_TEXT_MATERIAL_NAME,
                BoxMemoryType.SocialStudies => Constant.SOCIAL_STUDIES_TEXT_MATERIAL_NAME,
                BoxMemoryType.Science => Constant.SCIENCE_TEXT_MATERIAL_NAME,
                // BoxMemoryType.Trivia => Constant.TRIVIA_TEXT_MATERIAL_NAME,
                BoxMemoryType.Moral => Constant.MORAL_TEXT_MATERIAL_NAME,
                // BoxMemoryType.Music => Constant.MUSIC_TEXT_MATERIAL_NAME,
                // BoxMemoryType.Life => Constant.LIFE_TEXT_MATERIAL_NAME,
                _ => throw new ArgumentOutOfRangeException(nameof(memoryType), memoryType, null)
            };
        }
    }
}