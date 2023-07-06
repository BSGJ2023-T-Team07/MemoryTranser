using MemoryTranser.Scripts.Game.Util;
using UnityEngine;
using UnityEngine.Serialization;

namespace MemoryTranser.Scripts.Game.Sound {
    public class SeManager : SingletonMonoBehaviour<SeManager> {
        protected override bool DontDestroy => true;

        [System.Serializable]
        public class SeData {
            public SEs se;
            public AudioClip audioClip;
        }

        [SerializeField] private SeData[] seDatas;
        [SerializeField] private AudioSource audioSource;

        public void Play(SEs seType) {
            foreach (var seData in seDatas) {
                if (seData.se != seType) {
                    continue;
                }

                audioSource.PlayOneShot(seData.audioClip);
                break;
            }
        }

        public void SetSeVolume(float volume) {
            audioSource.volume = volume;
        }
    }

    public enum SEs {
        DecisionDefault1,
        DecisionDefault2,
        PutBox,
        HoldBox,
        ThrowBox,
        FairyAttackedByDesire,
        OutputTrue,
        OutputFalse,
        ConcentrationIsLittle,
        TimeUp,
        ResultShow1,
        ResultShow2
    }
}