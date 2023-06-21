using UnityEngine;
//using UnityEngine.InputSystem;

namespace MemoryTranser.Scripts.Game.Sound {
    public class SeManager : MonoBehaviour {
        [SerializeField] public AudioClip[] seClips;
        public AudioSource audioSource;

        void Start()
        {
            audioSource = GetComponent<AudioSource>();  // AudioSourceのコンポーネントを取得
        }
        
        private void Update()
        {
            //// Eキーを押したときの処理
            //if (Input.GetKeyDown(KeyCode.E))
            //{
            //    audioSource.PlayOneShot(seClips[0]);
            //}

            //// Qキーを押したときの処理
            //if (Input.GetKeyDown(KeyCode.Q))
            //{
            //    audioSource.PlayOneShot(seClips[0]);
            //}

            //// マウスの左クリックしたときの処理
            //if (Input.GetKeyDown(KeyCode.Mouse0))
            //{
            //    audioSource.PlayOneShot(seClips[1]);
            //}
        }
    }

    public enum SEs {
        ThrowBox
    }
}