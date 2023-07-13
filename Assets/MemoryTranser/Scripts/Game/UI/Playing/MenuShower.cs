using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MemoryTranser.Scripts.Game.Menu;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MemoryTranser.Scripts.Game.UI.Playing {
    public class MenuShower : MonoBehaviour {
        #region コンポーネントの定義

        [SerializeField] private GameObject pauseBackGround;
        [SerializeField] private GameObject menuLayer;

        [SerializeField] private Transform menuCursorTransform;

        [SerializeField] private TextMeshProUGUI resumeText;
        [SerializeField] private TextMeshProUGUI backToTitleText;

        [SerializeField] private Image currentMenuBackGroundImage;

        #endregion

        #region 素材の定義

        [Space] [SerializeField] private Sprite[] menuBackGroundSprites;

        #endregion

        #region 変数の定義

        [Space] [Header("カーソルと選択肢の文章の間隔のピクセル数")] [SerializeField]
        private float spacePixelCountBetweenCursorAndText;

        [Header("メニューの開閉アニメーション1コマにかかるフレーム数")] [SerializeField]
        private int menuToggleAnimationUnitFrameCount;

        private GameObject[] _menuLayerAndTheChildren;

        #endregion


        private void Awake() {
            pauseBackGround.SetActive(false);

            _menuLayerAndTheChildren = menuLayer.transform.GetComponentsInChildren<Transform>(false)
                .Select(x => x.gameObject)
                .ToArray();
            SetActiveMenuLayerAndTheChildren(false);
        }

        public async UniTask ToggleMenu(bool isActive) {
            if (isActive) {
                await OpenMenuAnimation();
                UnityEngine.Debug.Log("メニューを開いた");
            }
            else {
                await CloseMenuAnimation();
                UnityEngine.Debug.Log("メニューを閉じた");
            }
        }

        public void UpdateMenuSelectionShow(MenuSelection selection) {
            switch (selection) {
                case MenuSelection.Resume:
                    resumeText.color = Color.white;
                    backToTitleText.color = Color.gray;

                    menuCursorTransform.SetParent(resumeText.transform.parent);
                    menuCursorTransform.localPosition =
                        resumeText.transform.localPosition + Vector3.left *
                        (resumeText.rectTransform.sizeDelta.x / 2f + spacePixelCountBetweenCursorAndText);
                    break;
                case MenuSelection.BackToTitle:
                    resumeText.color = Color.gray;
                    backToTitleText.color = Color.white;

                    menuCursorTransform.SetParent(backToTitleText.transform.parent);
                    menuCursorTransform.localPosition =
                        backToTitleText.transform.localPosition + Vector3.left *
                        (backToTitleText.rectTransform.sizeDelta.x / 2f + spacePixelCountBetweenCursorAndText);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(selection), selection, null);
            }
        }

        private async UniTask OpenMenuAnimation() {
            pauseBackGround.SetActive(true);
            SetActiveMenuLayerAndTheChildren(false);

            menuLayer.SetActive(true);
            currentMenuBackGroundImage.gameObject.SetActive(true);

            for (var i = 0; i < menuBackGroundSprites.Length; i++) {
                currentMenuBackGroundImage.sprite = menuBackGroundSprites[i];

                if (i < menuBackGroundSprites.Length - 1) {
                    await UniTask.DelayFrame(menuToggleAnimationUnitFrameCount);
                }
                else {
                    SetActiveMenuLayerAndTheChildren(true);
                }
            }
        }

        private async UniTask CloseMenuAnimation() {
            SetActiveMenuLayerAndTheChildren(false);

            menuLayer.SetActive(true);
            currentMenuBackGroundImage.gameObject.SetActive(true);

            for (var i = 0; i < menuBackGroundSprites.Length; i++) {
                currentMenuBackGroundImage.sprite = menuBackGroundSprites[^(i + 1)];

                if (i < menuBackGroundSprites.Length - 1) {
                    await UniTask.DelayFrame(menuToggleAnimationUnitFrameCount);
                }
                else {
                    pauseBackGround.SetActive(false);
                    menuLayer.SetActive(false);
                }
            }
        }

        private void SetActiveMenuLayerAndTheChildren(bool isActive) {
            foreach (var child in _menuLayerAndTheChildren) {
                child.SetActive(isActive);
            }
        }
    }
}