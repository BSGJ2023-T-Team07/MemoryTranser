using System;
using DG.Tweening;
using MemoryTranser.Scripts.Game.Sound;
using MemoryTranser.Scripts.SceneTransition;
using MemoryTranser.Scripts.Title.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MemoryTranser.Scripts.Title {
    public class TitleManager : MonoBehaviour {
        [SerializeField] private TitleSelectionShower titleSelectionShower;
        [SerializeField] private TitleSettingsShower titleSettingsShower;
        [SerializeField] private PlayerInput playerInput;

        [Space] [SerializeField] private float soundVolumeChangeSpeed;

        private TitleSelection _currentTitleSelection = TitleSelection.StartWithIntroduction;
        private bool _isSettingsOpened = false;
        private SettingsSelection _currentSettingsSelection = SettingsSelection.BgmVolume;

        private InputAction _sliderAction;

        #region Unityから呼ばれる

        private void Awake() {
            _sliderAction = playerInput.actions["Slide"];
        }

        private void OnEnable() {
            playerInput.enabled = true;
        }

        private void Start() {
            BgmManager.I.PlayIntroLoop();
        }

        private void OnDisable() {
            playerInput.enabled = false;
        }

        private void Update() {
            if (!_isSettingsOpened) {
                return;
            }

            if (!_sliderAction.IsPressed()) {
                return;
            }

            var inputValue = _sliderAction.ReadValue<float>();
            var volumeValue = inputValue * soundVolumeChangeSpeed;

            if (_currentSettingsSelection == SettingsSelection.SeVolume) {
                SeManager.I.AddSeVolume(volumeValue);
                titleSettingsShower.AddSeSliderValue(volumeValue);
            }
            else {
                BgmManager.I.AddBgmVolume(volumeValue);
                titleSettingsShower.AddBgmSliderValue(volumeValue);
            }
        }

        #endregion

        #region 操作入力時の処理

        public void OnUpInput(InputAction.CallbackContext context) {
            if (!context.performed) {
                return;
            }

            if (!_isSettingsOpened) {
                TitleSelectionUp();
            }
            else {
                SettingsSelectionUp();
            }
        }

        public void OnDownInput(InputAction.CallbackContext context) {
            if (!context.performed) {
                return;
            }

            if (!_isSettingsOpened) {
                TitleSelectionDown();
            }
            else {
                SettingsSelectionDown();
            }
        }

        public void OnDecideInput(InputAction.CallbackContext context) {
            if (!context.performed) {
                return;
            }

            if (!_isSettingsOpened) {
                TitleSelectionDecide(_currentTitleSelection);
            }
        }

        public void OnExitInput(InputAction.CallbackContext context) {
            if (!context.performed) {
                return;
            }

            if (!_isSettingsOpened) {
                ExitGame();
            }
            else {
                titleSettingsShower.CloseSettings();
                _isSettingsOpened = false;
            }
        }

        #endregion

        #region 行動の定義

        private void TitleSelectionUp() {
            _currentTitleSelection = (TitleSelection)(((int)_currentTitleSelection - 1 + (int)TitleSelection.Count) %
                                                      (int)TitleSelection.Count);
            titleSelectionShower.UpdateTitleSelection(_currentTitleSelection);
            SeManager.I.Play(SEs.SelectionDefault1);
        }

        private void TitleSelectionDown() {
            _currentTitleSelection = (TitleSelection)(((int)_currentTitleSelection + 1) % (int)TitleSelection.Count);
            titleSelectionShower.UpdateTitleSelection(_currentTitleSelection);
            SeManager.I.Play(SEs.SelectionDefault1);
        }

        private void TitleSelectionDecide(TitleSelection selection) {
            SeManager.I.Play(SEs.DecisionDefault1);
            switch (selection) {
                case TitleSelection.StartWithIntroduction:
                    TransitToStoryScene();
                    break;
                case TitleSelection.StartWithoutIntroduction:
                    TransitToGameScene();
                    break;
                case TitleSelection.Settings:
                    TransitToSettingsMode();
                    break;
                // case TitleSelection.Credit:
                //     break;
                case TitleSelection.Exit:
                    ExitGame();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(selection), selection, null);
            }
        }

        private void SettingsSelectionUp() {
            _currentSettingsSelection =
                (SettingsSelection)(((int)_currentSettingsSelection - 1 + (int)SettingsSelection.Count) %
                                    (int)SettingsSelection.Count);
            titleSettingsShower.UpdateSettingsShow(_currentSettingsSelection);
            SeManager.I.Play(SEs.SelectionDefault1);
        }

        private void SettingsSelectionDown() {
            _currentSettingsSelection =
                (SettingsSelection)(((int)_currentSettingsSelection + 1) % (int)SettingsSelection.Count);
            titleSettingsShower.UpdateSettingsShow(_currentSettingsSelection);
            SeManager.I.Play(SEs.SelectionDefault1);
        }

        #endregion

        private static void TransitToStoryScene() {
            SceneTransitionEffecter.I.PlayFadeEffect(DOTween.Sequence().OnPlay(() => {
                BgmManager.I.StopIntroLoop();
                SceneManager.LoadScene("MemoryTranser/Scenes/StoryIntroductionScene");
            }));
        }

        private static void TransitToGameScene() {
            SceneTransitionEffecter.I.PlayFadeEffect(DOTween.Sequence().OnPlay(() => {
                BgmManager.I.StopIntroLoop();
                SceneManager.LoadScene("MemoryTranser/Scenes/GameScene");
            }));
        }

        private void TransitToSettingsMode() {
            _isSettingsOpened = true;
            titleSettingsShower.OpenSettings();
        }

        private static void ExitGame() {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    public enum TitleSelection {
        StartWithIntroduction,
        StartWithoutIntroduction,
        Settings,

        // Credit,
        Exit,

        //以上の要素の数を取れる
        Count
    }

    public enum SettingsSelection {
        SeVolume,
        BgmVolume,

        //以上の要素の数を取れる
        Count
    }
}