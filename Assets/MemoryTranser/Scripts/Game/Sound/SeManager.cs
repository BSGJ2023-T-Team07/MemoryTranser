using System;
using MemoryTranser.Scripts.Game.Util;
using UnityEngine;
using UnityEngine.Serialization;

namespace MemoryTranser.Scripts.Game.Sound {
    public class SeManager : SingletonMonoBehaviour<SeManager> {
        protected override bool DontDestroy => true;

        [Serializable]
        public class SeData {
            public SEs se;
            public AudioClip audioClip;
        }

        [SerializeField] private SeData[] seDatas;
        [SerializeField] private AudioSource audioSource;

        [Space] [SerializeField] private float initialVolume;

        protected override void Awake() {
            base.Awake();

            audioSource.volume = initialVolume;
        }

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

        public float GetSeVolume() {
            return audioSource.volume;
        }

        public void AddSeVolume(float volume) {
            audioSource.volume += volume;
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
        ResultShow2,
        ResultStamp,
        SelectionDefault1,
        Blink,
        OpenMenu,
        CloseMenu,
        ResultShow3,
        PushSphereBox,
        NoticeEvent
    }
}