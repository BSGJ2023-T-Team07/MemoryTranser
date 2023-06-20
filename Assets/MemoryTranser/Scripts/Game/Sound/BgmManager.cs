using System;
using Cysharp.Threading.Tasks;
using MemoryTranser.Scripts.Game.GameManagers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace MemoryTranser.Scripts.Game.Sound {
    public class BgmManager : MonoBehaviour, IOnStateChangedToInitializing {
        #region コンポーネントの定義

        [SerializeField] private AudioClip bgmIntro;
        [SerializeField] private AudioClip bgmMain;

        [SerializeField] private AudioSource bgmIntroSource;
        [SerializeField] private AudioSource bgmMainSource;

        #endregion

        #region Unityから呼ばれる

        private void Start() {
            DontDestroyOnLoad(gameObject);
            bgmIntroSource.volume = 0.5f;
            bgmMainSource.volume = 0.5f;
        }

        #endregion

        public void OnStateChangedToInitializing() {
            PlayIntro();
        }

        private async void PlayIntro() {
            Debug.Log("BGMイントロの再生を開始しました");
            bgmIntroSource.clip = bgmIntro;
            bgmIntroSource.Play();

            //イントロが終わったらメイン部分の再生を開始する
            await UniTask.Delay(TimeSpan.FromSeconds(bgmIntro.length));

            PlayMain();
        }

        private void PlayMain() {
            Debug.Log("BGMメインの再生を開始しました");
            bgmMainSource.loop = true;
            bgmMainSource.clip = bgmMain;
            bgmMainSource.Play();
        }

        public void SetBgmVolume(float volume) {
            bgmIntroSource.volume = volume;
            bgmMainSource.volume = volume;
        }
    }
}