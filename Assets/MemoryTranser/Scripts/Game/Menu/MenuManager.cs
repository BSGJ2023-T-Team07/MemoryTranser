using System;
using DG.Tweening;
using MemoryTranser.Scripts.Game.GameManagers;
using MemoryTranser.Scripts.Game.Sound;
using MemoryTranser.Scripts.Game.UI.Playing;
using MemoryTranser.Scripts.SceneTransition;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace MemoryTranser.Scripts.Game.Menu {
    public class MenuManager : MonoBehaviour {
        #region コンポーネントの定義

        [SerializeField] private MenuShower menuShower;
        [SerializeField] private PlayerInput playerInput;

        #endregion

        #region 変数の定義

        private MenuSelection _currentMenuSelection = MenuSelection.Resume;
        private bool _isMenuOpened;

        #endregion

        #region プロパティーの定義

        public bool IsMenuOpened => _isMenuOpened;

        #endregion

        #region 操作入力時の処理

        public void OnOpenMenuInput(InputAction.CallbackContext context) {
            //メニューが開かれていれば何もしない
            if (_isMenuOpened) {
                return;
            }

            if (!context.action.WasPressedThisFrame()) {
                return;
            }

            if (GameFlowManager.I.CurrentGameState is not (GameState.Playing or GameState.Ready)) {
                return;
            }

            OpenMenu();
        }

        public void OnCloseMenuInput(InputAction.CallbackContext context) {
            //メニューが開かれていなければ何もしない
            if (!_isMenuOpened) {
                return;
            }

            if (!context.action.WasPressedThisFrame()) {
                return;
            }

            CloseMenu();
        }

        public void OnUpInput(InputAction.CallbackContext context) {
            //メニューが開かれていなければ何もしない
            if (!_isMenuOpened) {
                return;
            }

            if (!context.action.WasPressedThisFrame()) {
                return;
            }

            SelectionUp();
        }

        public void OnDownInput(InputAction.CallbackContext context) {
            //メニューが開かれていなければ何もしない
            if (!_isMenuOpened) {
                return;
            }

            if (!context.action.WasPressedThisFrame()) {
                return;
            }

            SelectionDown();
        }

        public void OnDecideInput(InputAction.CallbackContext context) {
            //メニューが開かれていなければ何もしない
            if (!_isMenuOpened) {
                return;
            }

            if (!context.action.WasPressedThisFrame()) {
                return;
            }

            Decide(_currentMenuSelection);
        }

        #endregion

        #region 行動の定義

        private async void OpenMenu() {
            playerInput.SwitchCurrentActionMap("UI");
            _isMenuOpened = true;
            Time.timeScale = 0f;
            BgmManager.I.PausePlayingBgm();
            SeManager.I.Play(SEs.OpenMenu);

            await menuShower.ToggleMenu(true);

            _currentMenuSelection = MenuSelection.Resume;
            menuShower.UpdateMenuSelectionShow(_currentMenuSelection);
        }

        private async void CloseMenu() {
            playerInput.SwitchCurrentActionMap("Player");
            _isMenuOpened = false;
            SeManager.I.Play(SEs.OpenMenu);

            await menuShower.ToggleMenu(false);

            Time.timeScale = 1f;
            BgmManager.I.UnPausePlayingBgm();
        }

        private void SelectionUp() {
            _currentMenuSelection = (MenuSelection)(((int)_currentMenuSelection - 1 + (int)MenuSelection.Count) %
                                                    (int)MenuSelection.Count);
            menuShower.UpdateMenuSelectionShow(_currentMenuSelection);
            SeManager.I.Play(SEs.SelectionDefault1);
        }

        private void SelectionDown() {
            _currentMenuSelection = (MenuSelection)(((int)_currentMenuSelection + 1) % (int)MenuSelection.Count);
            menuShower.UpdateMenuSelectionShow(_currentMenuSelection);
            SeManager.I.Play(SEs.SelectionDefault1);
        }

        private void Decide(MenuSelection selection) {
            SeManager.I.Play(SEs.DecisionDefault1);

            switch (selection) {
                case MenuSelection.Resume:
                    CloseMenu();
                    break;
                case MenuSelection.BackToTitle:
                    TransitToTitle();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(selection), selection, null);
            }
        }

        private static void TransitToTitle() {
            SceneTransitionEffecter.I.PlayFadeEffect(DOTween.Sequence().OnPlay(() => {
                Time.timeScale = 1f;
                BgmManager.I.StopIntroLoop();
                SceneManager.LoadScene("MemoryTranser/Scenes/TitleScene");
            }));
        }

        #endregion
    }
}