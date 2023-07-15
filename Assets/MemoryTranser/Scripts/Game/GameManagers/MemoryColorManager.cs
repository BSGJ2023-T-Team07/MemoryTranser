using System;
using MemoryTranser.Scripts.Game.MemoryBox;
using MemoryTranser.Scripts.Game.Util;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.GameManagers {
    public class MemoryColorManager : SingletonMonoBehaviour<MemoryColorManager> {
        protected override bool DontDestroy => true;

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