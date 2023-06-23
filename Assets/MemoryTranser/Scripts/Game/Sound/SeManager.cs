using UnityEngine;
using UnityEngine.Serialization;

namespace MemoryTranser.Scripts.Game.Sound {
    public class SeManager : MonoBehaviour {
        [System.Serializable]
        public class SeData {
            public SEs se;
            public AudioClip audioClip;
        }

        [SerializeField] private SeData[] seDatas;
        [SerializeField] private AudioSource audioSource;

        public void Play(SEs seType) {
            audioSource.PlayOneShot(seDatas[(int)seType].audioClip);
        }

        public void SetSeVolume(float volume) {
            audioSource.volume = volume;
        }
    }

    public enum SEs {
        PutBox,
        HoldBox,
        ThrowBox
    }
}