using System;
using System.Collections.Generic;
using DG.Tweening;
using JetBrains.Annotations;
using MemoryTranser.Scripts.SceneTransition;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MemoryTranser.Scripts.Story {
    public class StoryIntroductionManager : MonoBehaviour {
        #region コンポーネントの定義

        [SerializeField] private Image currentIntroductionImage;
        [SerializeField] private GameObject pauseBackGround;

        [Space] [SerializeField] private TextMeshProUGUI pausableText;
        [SerializeField] private TextMeshProUGUI unPausableText;
        [SerializeField] private TextMeshProUGUI skippableText;
        [SerializeField] private TextMeshProUGUI pausingText;

        #endregion

        #region 変数の定義

        [SerializeField] private List<Sprite> introductionImages;
        [SerializeField] private float[] intervalSecs;

        private Sequence _introductionSequence;

        private int _currentIntroductionImageIndex;
        private bool _isPausing;

        private const string INTRODUCTION_IMAGE_PATH = "Sprites/Story/Introduction/";

        #endregion

        #region Unityから呼ばれる

        // private void OnValidate() {
        //     var i = 1;
        //     while (true) {
        //         var storyImage = Resources.Load<Sprite>($"{INTRODUCTION_IMAGE_PATH}{i}");
        //
        //         if (storyImage) {
        //             introductionImages.Add(storyImage);
        //         }
        //         else {
        //             break;
        //         }
        //
        //         i++;
        //     }
        // }

        private void Awake() {
            _introductionSequence = DOTween.Sequence();
            pauseBackGround.SetActive(false);
            pausingText.enabled = false;
        }

        private void Start() {
            currentIntroductionImage.sprite = introductionImages[0];

            for (var i = 1; i < introductionImages.Count; i++) {
                _introductionSequence.AppendInterval(intervalSecs[i - 1]);
                _introductionSequence.Append(DOTween.Sequence().OnPlay(TransitToNextImage));
            }

            _introductionSequence.AppendInterval(intervalSecs[^1]);
            _introductionSequence.OnComplete(TransitToGameScene);
            _introductionSequence.SetLink(gameObject);

            _introductionSequence.Play();
        }

        #endregion

        #region 操作入力時の処理

        public void OnPauseInput(InputAction.CallbackContext context) {
            if (_isPausing) {
                return;
            }

            if (!context.action.WasPressedThisFrame()) {
                return;
            }

            IntroductionPause();
        }

        public void OnUnPauseInput(InputAction.CallbackContext context) {
            if (!_isPausing) {
                return;
            }

            if (!context.action.WasPressedThisFrame()) {
                return;
            }

            IntroductionUnPause();
        }

        public void OnSkipInput(InputAction.CallbackContext context) {
            if (!context.action.WasPressedThisFrame()) {
                return;
            }

            IntroductionSkip();
        }

        #endregion

        #region 行動の定義

        private static void IntroductionSkip() {
            TransitToGameScene();
        }

        private void IntroductionPause() {
            pauseBackGround.SetActive(true);
            pausingText.enabled = true;
            _isPausing = true;
            _introductionSequence.TogglePause();
        }

        private void IntroductionUnPause() {
            pauseBackGround.SetActive(false);
            pausingText.enabled = false;
            _isPausing = false;
            _introductionSequence.TogglePause();
        }

        #endregion

        private void TransitToNextImage() {
            _currentIntroductionImageIndex++;
            currentIntroductionImage.sprite = introductionImages[_currentIntroductionImageIndex];
        }

        private static void TransitToGameScene() {
            SceneTransitionEffecter.I.PlayFadeEffect(DOTween.Sequence().OnPlay(() => {
                SceneManager.LoadScene("MemoryTranser/Scenes/GameScene");
            }));
        }
    }
}