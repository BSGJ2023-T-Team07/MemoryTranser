using System;
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

        #region プロパテｘヂーの定義

        #endregion

        #region 操作入力時の処理

        public void OnOpenMenuInput(InputAction.CallbackContext context) {
            if (!context.action.WasPressedThisFrame()) {
                return;
            }

            OpenMenu();
        }

        public void OnCloseMenuInput(InputAction.CallbackContext context) {
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

        private void OpenMenu() {
            playerInput.currentActionMap = playerInput.actions.FindActionMap("UI");

            _isMenuOpened = true;
            menuShower.ToggleMenu(true);
            Time.timeScale = 0f;

            _currentMenuSelection = MenuSelection.Resume;
            menuShower.UpdateMenuSelectionShow(_currentMenuSelection);
        }

        private void CloseMenu() {
            playerInput.currentActionMap = playerInput.actions.FindActionMap("Player");

            _isMenuOpened = false;
            menuShower.ToggleMenu(false);
            Time.timeScale = 1f;
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
    }

    public enum MenuSelection {
        Resume,
        BackToTitle,

        //以上の要素の数を取得できる
        Count
    }
}