using UnityEngine;

namespace MemoryTranser.Scripts.Game.Util {
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : SingletonMonoBehaviour<T> {
        // シーンを跨いで値を保持するか
        protected abstract bool DontDestroy { get; }

        private static T _instance;

        public static T I {
            get {
                // 値が参照されたタイミングで判定
                if (_instance) {
                    return _instance;
                }

                // nullだった場合は全オブジェクトを探索
                // 名前が一致するクラスがあった場合は取得する
                _instance = FindObjectOfType<T>();

                // 名前が一致するものがなかった場合
                if (!_instance) {
                    // コンソールウィンドウにエラーを出力
                    Debug.LogError($"{typeof(T)}のインスタンスが存在しません。");
                }

                return _instance;
            }
        }

        // 継承先でもAwakeを呼び出したい場合は，overrideする
        protected virtual void Awake() {
            // 既に同一名のクラスが存在していた場合
            if (I != this) {
                // ゲームオブジェクトごと削除
                Destroy(gameObject);
                return;
            }

            if (DontDestroy) {
                DontDestroyOnLoad(gameObject);
            }
        }
    }
}