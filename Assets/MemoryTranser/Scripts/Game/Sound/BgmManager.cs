using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MemoryTranser.Scripts.Game.GameManagers;
using MemoryTranser.Scripts.Game.Util;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.Sound {
    public class BgmManager : SingletonMonoBehaviour<BgmManager>, IOnStateChangedToInitializing,
        IOnStateChangedToResult {
        protected override bool DontDestroy => true;

        #region コンポーネントの定義

        [SerializeField] private AudioClip bgmIntro;
        [SerializeField] private AudioClip bgmMain;

        [SerializeField] private AudioSource bgmIntroSource;
        [SerializeField] private AudioSource bgmMainSource;

        [Space] [SerializeField] private float initialVolume;

        #endregion

        #region 変数の定義

        private CancellationTokenSource _cancellationTokenSourceForPlayMain;

        #endregion

        #region Unityから呼ばれる

        protected override void Awake() {
            base.Awake();

            bgmIntroSource.clip = bgmIntro;
            bgmMainSource.clip = bgmMain;

            bgmIntroSource.volume = initialVolume;
            bgmMainSource.volume = initialVolume;

            _cancellationTokenSourceForPlayMain = new CancellationTokenSource();
        }

        #endregion

        public void OnStateChangedToInitializing() {
            _cancellationTokenSourceForPlayMain = new CancellationTokenSource();
            PlayIntroAndStopAndPlayMain(_cancellationTokenSourceForPlayMain.Token).Forget(e => { });
        }

        public void OnStateChangedToResult() {
            const float fadeDuration = 0.8f;
            bgmIntroSource.DOFade(0f, fadeDuration).OnComplete(() => { bgmIntroSource.Stop(); });
            bgmMainSource.DOFade(0f, fadeDuration).OnComplete(() => { bgmMainSource.Stop(); });
            SetBgmPitch(1f);
        }

        private async UniTask PlayIntroAndStopAndPlayMain(CancellationToken cancellationToken = default) {
            bgmIntroSource.Play();

            //イントロが終わったらメイン部分の再生を開始する
            await UniTask.Delay(TimeSpan.FromSeconds(bgmIntro.length), cancellationToken: cancellationToken);

            if (GameFlowManager.I is { CurrentGameState: GameState.Playing or GameState.Ready }) {
                bgmIntroSource.Stop();
                bgmMainSource.Play();
            }
        }

        public void PlayIntroLoop() {
            bgmIntroSource.loop = true;
            bgmIntroSource.Play();
        }

        public void StopIntroLoop() {
            bgmIntroSource.loop = false;
            bgmIntroSource.Stop();
            _cancellationTokenSourceForPlayMain.Cancel();
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

        public float GetBgmVolume() {
            return bgmMainSource.volume;
        }

        public void AddBgmVolume(float volume) {
            bgmIntroSource.volume += volume;
            bgmMainSource.volume += volume;
        }

        public void SetBgmPitch(float pitch) {
            bgmIntroSource.pitch = pitch;
            bgmMainSource.pitch = pitch;
        }
    }
}