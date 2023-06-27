using System.Linq;
using MemoryTranser.Scripts.Game.BrainEvent;
using MemoryTranser.Scripts.Game.Desire;
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

        public static Sprite ToDesireSprite(this DesireType desireType) {
            var path = $"Sprites/Desire/{desireType}DesireSprite";
            return Resources.Load<Sprite>(path);
        }
    }

    public static class StringExtensions {
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

        public static string ToJapanese(this BrainEventType brainEventType) {
            return brainEventType switch {
                BrainEventType.Normal => "なし",
                BrainEventType.Blind => "ド忘れ",
                BrainEventType.DesireOutbreak => "煩悩大量発生",
                _ => ""
            };
        }

        public static string ToJapanese(this DesireType desireType) {
            return desireType switch {
                DesireType.Game => "ゲーム",
                DesireType.Comic => "漫画",
                DesireType.Karaoke => "カラオケ",
                DesireType.Ramen => "ラーメン",
                _ => "error"
            };
        }
    }

    public static class GameObjectExtensions {
        public static T[] FindObjectsByInterface<T>() where T : class {
            return Object.FindObjectsByType<Component>(FindObjectsSortMode.None).OfType<T>().ToArray();
        }
    }
}