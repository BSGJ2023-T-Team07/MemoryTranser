using UnityEngine;

namespace MemoryTranser.Scripts.Game.Util {
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T> {
        protected abstract bool DontDestroy { get; }

        private static T _instance;

        public static T I {
            get {
                if (_instance) {
                    return _instance;
                }

                _instance = FindObjectOfType<T>();

                if (!_instance) {
                    Debug.LogError($"{typeof(T)}のインスタンスが存在しません。");
                }

                return _instance;
            }
        }

        protected virtual void Awake() {
            if (I != this) {
                Destroy(gameObject);
                return;
            }

            if (DontDestroy) {
                DontDestroyOnLoad(gameObject);
            }
        }
    }
}