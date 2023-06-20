using System.Linq;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.Util {
    public static class GameObjectExtensions {
        public static T[] FindObjectsByInterface<T>() where T : class {
            return Object.FindObjectsByType<Component>(FindObjectsSortMode.None).OfType<T>().ToArray();
        }
    }
}