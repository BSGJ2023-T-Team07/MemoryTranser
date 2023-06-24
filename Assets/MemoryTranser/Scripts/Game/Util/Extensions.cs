using System.Linq;
using MemoryTranser.Scripts.Game.MemoryBox;
using MemoryTranser.Scripts.Game.Phase;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.Util {
    public static class SpriteExtensions {
        public static Sprite ToMemoryBoxSprite(this BoxMemoryType boxMemoryType, MemoryBoxShapeType boxShape) {
            var memoryTypeName = boxMemoryType.ToString();
            var shapeName = boxShape.ToString();
            var path = $"Sprites/MemoryBox/{memoryTypeName}{shapeName}MemoryBoxSprite";
            return Resources.Load<Sprite>(path);
        }
    }

    public static class MemoryTypeExtensions {
        public static string ToJapanese(this BoxMemoryType boxMemoryType) {
            return boxMemoryType switch {
                BoxMemoryType.English => "英語",
                BoxMemoryType.Habit => "趣味",
                BoxMemoryType.Japanese => "国語",
                BoxMemoryType.Life => "生活",
                BoxMemoryType.Math => "数学",
                BoxMemoryType.Moral => "道徳",
                BoxMemoryType.Music => "音楽",
                BoxMemoryType.Science => "理科",
                BoxMemoryType.Trivia => "雑学",
                BoxMemoryType.SocialStudies => "社会",
                _ => "error"
            };
        }

        public static string ToJapanese(this PhaseMemoryType phaseMemoryType) {
            return phaseMemoryType switch {
                PhaseMemoryType.English => "英語",
                PhaseMemoryType.Habit => "趣味",
                PhaseMemoryType.Japanese => "国語",
                PhaseMemoryType.Life => "生活",
                PhaseMemoryType.Math => "数学",
                PhaseMemoryType.Moral => "道徳",
                PhaseMemoryType.Music => "音楽",
                PhaseMemoryType.Science => "理科",
                PhaseMemoryType.Trivia => "雑学",
                PhaseMemoryType.SocialStudies => "社会",
                _ => "error"
            };
        }
    }

    public static class PhaseGimmickTypeExtensions {
        public static string ToJapanese(this PhaseGimmickType phaseGimmickType) {
            return phaseGimmickType switch {
                PhaseGimmickType.Normal => "なし",
                PhaseGimmickType.Blind => "ド忘れ",
                _ => ""
            };
        }
    }

    public static class GameObjectExtensions {
        public static T[] FindObjectsByInterface<T>() where T : class {
            return Object.FindObjectsByType<Component>(FindObjectsSortMode.None).OfType<T>().ToArray();
        }
    }
}