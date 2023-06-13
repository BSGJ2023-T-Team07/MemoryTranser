using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MemoryTranser.Scripts.Title {
    public class TitleManager : MonoBehaviour {
        private void Update() {
            if (Input.GetKeyDown(KeyCode.Space)) TransitToGameScene();

            if (Input.GetKeyDown(KeyCode.Escape)) {
#if UNITY_EDITOR
                EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
        }

        private void TransitToGameScene() {
            SceneManager.LoadScene("GameScene");
        }
    }
}