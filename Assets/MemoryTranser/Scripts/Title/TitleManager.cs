using System;
using MemoryTranser.Scripts.Game.Sound;
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

        private TitleSelection _currentTitleSelection = TitleSelection.Start;

        #region Unityから呼ばれる

        private void Start() {
            BgmManager.I.PlayIntroLoop();
        }

        #endregion

        #region 操作入力時の処理

        public void OnUpInput(InputAction.CallbackContext context) {
            if (!context.action.WasPressedThisFrame()) {
                return;
            }

            SelectionUp();
        }

        public void OnDownInput(InputAction.CallbackContext context) {
            if (!context.action.WasPressedThisFrame()) {
                return;
            }

            SelectionDown();
        }

        public void OnDecideInput(InputAction.CallbackContext context) {
            if (!context.action.WasPressedThisFrame()) {
                return;
            }

            Decide(_currentTitleSelection);
        }

        #endregion

        #region 行動の定義

        private void SelectionUp() {
            _currentTitleSelection = (TitleSelection)(((int)_currentTitleSelection - 1 + (int)TitleSelection.Count) %
                                                      (int)TitleSelection.Count);
            titleSelectionShower.UpdateTitleSelection(_currentTitleSelection);
            SeManager.I.Play(SEs.SelectionDefault1);
        }

        private void SelectionDown() {
            _currentTitleSelection = (TitleSelection)(((int)_currentTitleSelection + 1) % (int)TitleSelection.Count);
            titleSelectionShower.UpdateTitleSelection(_currentTitleSelection);
            SeManager.I.Play(SEs.SelectionDefault1);
        }

        private void Decide(TitleSelection selection) {
            SeManager.I.Play(SEs.DecisionDefault1);
            switch (selection) {
                case TitleSelection.Start:
                    TransitToGameScene();
                    break;
                case TitleSelection.Credit:
                    break;
                case TitleSelection.Exit:
                    ExitGame();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(selection), selection, null);
            }
        }

        #endregion

        private static void TransitToGameScene() {
            BgmManager.I.StopIntroLoop();
            SceneManager.LoadScene("GameScene");
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
        Start,
        Credit,
        Exit,

        //以上の要素の数を取れる
        Count
    }
}