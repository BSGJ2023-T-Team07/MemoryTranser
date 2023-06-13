using MemoryTranser.Scripts.Game.MemoryBox;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.Util {
    public static class SpriteExtension {
        public static Sprite ToMemoryBoxSprite(this BoxMemoryType boxMemoryType) {
            var memoryTypeName = boxMemoryType.ToString();
            var path = $"Sprites/MemoryBox/{memoryTypeName}MemorySprite";
            return Resources.Load<Sprite>(path);
        }
    }
}