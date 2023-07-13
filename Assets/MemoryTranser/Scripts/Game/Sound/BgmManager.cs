using System;
using Cysharp.Threading.Tasks;
using MemoryTranser.Scripts.Game.GameManagers;
using MemoryTranser.Scripts.Game.Util;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.Sound {
    public class BgmManager : SingletonMonoBehaviour<BgmManager>, IOnStateChangedToInitializing {
        protected override bool DontDestroy => true;

        #region コンポーネントの定義

        [SerializeField] private AudioClip bgmIntro;
        [SerializeField] private AudioClip bgmMain;

        [SerializeField] private AudioSource bgmIntroSource;
        [SerializeField] private AudioSource bgmMainSource;

        #endregion

        #region Unityから呼ばれる

        protected override void Awake() {
            base.Awake();

            bgmIntroSource.clip = bgmIntro;
            bgmMainSource.clip = bgmMain;
        }


        private void Start() {
            // bgmIntroSource.volume = 0.5f;
            // bgmMainSource.volume = 0.5f;
        }

        #endregion

        public void OnStateChangedToInitializing() {
            PlayIntroAndStopAndPlayMain();
        }

        private async void PlayIntroAndStopAndPlayMain() {
            bgmIntroSource.Play();

            //イントロが終わったらメイン部分の再生を開始する
            await UniTask.Delay(TimeSpan.FromSeconds(bgmIntro.length));

            bgmIntroSource.Stop();
            bgmMainSource.Play();
        }

        public void PlayIntroLoop() {
            bgmIntroSource.loop = true;
            bgmIntroSource.Play();
        }

        public void StopIntroLoop() {
            bgmIntroSource.loop = false;
            bgmIntroSource.Stop();
        }

        public void PausePlayingBgm() {
            bgmIntroSource.Pause();
            bgmMainSource.Pause();
        }

        public void UnPausePlayingBgm() {
            bgmIntroSource.UnPause();
            bgmMainSource.UnPause();
        }

        public void SetBgmVolume(float volume) {
            bgmIntroSource.volume = volume;
            bgmMainSource.volume = volume;
        }

        public void SetBgmPitch(float pitch) {
            bgmIntroSource.pitch = pitch;
            bgmMainSource.pitch = pitch;
        }
    }
}