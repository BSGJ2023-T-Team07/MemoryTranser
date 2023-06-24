using MemoryTranser.Scripts.Game.Sound;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MemoryTranser.Scripts.Temp {
    public class BackToTitleScene : MonoBehaviour {
        private void Update() {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                BgmManager.I.SetBgmVolume(0f);
                SeManager.I.SetSeVolume(0f);
                SceneManager.LoadScene("MemoryTranser/Scenes/TitleScene");
            }
        }
    }
}