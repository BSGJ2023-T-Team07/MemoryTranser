using System;
using MemoryTranser.Scripts.Game.GameManagers;
using MemoryTranser.Scripts.Game.Menu;
using MemoryTranser.Scripts.Game.Sound;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace MemoryTranser.Scripts.Game.UI {
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

            var value = context.ReadValue<float>();
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

            var value = context.ReadValue<float>();
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

            var value = context.ReadValue<float>();
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

            var value = context.ReadValue<float>();
            Decide(_currentMenuSelection);
        }

        #endregion

        #region 行動の定義

        private void OpenMenu() {
            playerInput.SwitchCurrentActionMap("UI");

            _isMenuOpened = true;
            menuShower.ToggleMenu(true);
            Time.timeScale = 0f;
            BgmManager.I.PausePlayingBgm();

            _currentMenuSelection = MenuSelection.Resume;
            menuShower.UpdateMenuSelectionShow(_currentMenuSelection);
        }

        private void CloseMenu() {
            playerInput.SwitchCurrentActionMap("Player");

            _isMenuOpened = false;
            menuShower.ToggleMenu(false);
            Time.timeScale = 1f;
            BgmManager.I.UnPausePlayingBgm();
        }

        private void SelectionUp() {
            _currentMenuSelection = (MenuSelection)(((int)_currentMenuSelection - 1 + (int)MenuSelection.Count) %
                                                    (int)MenuSelection.Count);
            menuShower.UpdateMenuSelectionShow(_currentMenuSelection);
        }

        private void SelectionDown() {
            _currentMenuSelection = (MenuSelection)(((int)_currentMenuSelection + 1) % (int)MenuSelection.Count);
            menuShower.UpdateMenuSelectionShow(_currentMenuSelection);
        }

        private void Decide(MenuSelection selection) {
            switch (selection) {
                case MenuSelection.Resume:
                    CloseMenu();
                    break;
                case MenuSelection.BackToTitle:
                    SceneManager.LoadScene("MemoryTranser/Scenes/TitleScene");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(selection), selection, null);
            }
        }

        #endregion
    }
}