using UnityEngine;
using UnityEngine.SceneManagement;

namespace MemoryTranser.Scripts.Temp {
    public class BackToTitleScene : MonoBehaviour {
        private void Update() {
            if (Input.GetKeyDown(KeyCode.Escape)) SceneManager.LoadScene("MemoryTranser/Scenes/TitleScene");
        }
    }
}