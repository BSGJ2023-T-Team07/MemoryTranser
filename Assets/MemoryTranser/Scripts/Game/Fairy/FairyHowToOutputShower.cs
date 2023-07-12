using System;
using DG.Tweening;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MemoryTranser.Scripts.Game.Fairy {
    public class FairyHowToOutputShower : MonoBehaviour {
        #region コンポーネントの定義

        [SerializeField] private Animator outputExplanationAnimator;

        [Space] [SerializeField] private Image currentRStickImage;
        [SerializeField] private TextMeshProUGUI holdRStickText;
        [SerializeField] private TextMeshProUGUI smashRStickText;
        [SerializeField] private Image smashRStickUpEffectImage;
        [SerializeField] private Image smashRStickDownEffectImage;

        #endregion

        #region 画像素材の定義

        [SerializeField] private Sprite neutralRStickSprite;
        [SerializeField] private Sprite downerRStickSprite;
        [SerializeField] private Sprite upperRStickSprite;

        #endregion

        #region プロパティーの定義

        public bool IsShowingHoldAnimation =>
            outputExplanationAnimator.GetCurrentAnimatorStateInfo(0).IsName("HoldDown") ||
            outputExplanationAnimator.GetCurrentAnimatorStateInfo(0).IsName("HoldUp");

        #endregion


        #region Unityから呼ばれる

        private void Awake() {
            outputExplanationAnimator.enabled = false;

            currentRStickImage.sprite = neutralRStickSprite;
            currentRStickImage.enabled = false;

            holdRStickText.enabled = false;
            smashRStickText.enabled = false;

            smashRStickUpEffectImage.enabled = false;
            smashRStickDownEffectImage.enabled = false;
        }

        #endregion

        public void HideAnimation() {
            HideOutputExplanation();
            outputExplanationAnimator.enabled = false;
        }

        public void PlayAnimation(OutputActionType outputActionType) {
            outputExplanationAnimator.enabled = true;
            switch (outputActionType) {
                case OutputActionType.HoldDown:
                    outputExplanationAnimator.Play("HoldDown");
                    break;
                case OutputActionType.SmashUp:
                    outputExplanationAnimator.Play("SmashUp");
                    break;
                case OutputActionType.HoldUp:
                    outputExplanationAnimator.Play("HoldUp");
                    break;
                case OutputActionType.SmashDown:
                    outputExplanationAnimator.Play("SmashDown");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(outputActionType), outputActionType, null);
            }
        }

        // called by Animation Event
        private void HideOutputExplanation() {
            currentRStickImage.enabled = false;
            SetActiveHoldText(RStickActionType.Neutral);
            SetActiveSmashText(RStickActionType.Neutral);
            SetActiveSmashEffect(RStickImageType.Neutral);
        }

        // called by Animation Event
        private void ShowRStickNeutral() {
            currentRStickImage.sprite = neutralRStickSprite;
            currentRStickImage.enabled = true;
            SetActiveHoldText(RStickActionType.Neutral);
            SetActiveSmashText(RStickActionType.Neutral);
            SetActiveSmashEffect(RStickImageType.Neutral);
        }

        // called by Animation Event
        private void ShowRStickHoldDown() {
            currentRStickImage.sprite = downerRStickSprite;
            currentRStickImage.enabled = true;
            SetActiveHoldText(RStickActionType.Hold);
            SetActiveSmashText(RStickActionType.Hold);
            SetActiveSmashEffect(RStickImageType.Neutral);
        }

        // called by Animation Event
        private void ShowRStickHoldUp() {
            currentRStickImage.sprite = upperRStickSprite;
            currentRStickImage.enabled = true;
            SetActiveHoldText(RStickActionType.Hold);
            SetActiveSmashText(RStickActionType.Hold);
            SetActiveSmashEffect(RStickImageType.Neutral);
        }

        // called by Animation Event
        private void ShowRStickSmashDown() {
            currentRStickImage.sprite = downerRStickSprite;
            currentRStickImage.enabled = true;
            SetActiveHoldText(RStickActionType.Smash);
            SetActiveSmashText(RStickActionType.Smash);
            SetActiveSmashEffect(RStickImageType.Downer);
        }

        // called by Animation Event
        private void ShowRStickSmashUp() {
            currentRStickImage.sprite = upperRStickSprite;
            currentRStickImage.enabled = true;
            SetActiveHoldText(RStickActionType.Smash);
            SetActiveSmashText(RStickActionType.Smash);
            SetActiveSmashEffect(RStickImageType.Upper);
        }

        private void SetActiveHoldText(RStickActionType rStickActionType) {
            holdRStickText.enabled = rStickActionType switch {
                RStickActionType.Neutral => false,
                RStickActionType.Smash => false,
                RStickActionType.Hold => true,
                _ => holdRStickText.enabled
            };
        }

        private void SetActiveSmashText(RStickActionType rStickActionType) {
            smashRStickText.enabled = rStickActionType switch {
                RStickActionType.Neutral => false,
                RStickActionType.Smash => true,
                RStickActionType.Hold => false,
                _ => smashRStickText.enabled
            };
        }

        private void SetActiveSmashEffect(RStickImageType rStickImageType) {
            switch (rStickImageType) {
                case RStickImageType.Neutral:
                    smashRStickUpEffectImage.enabled = false;
                    smashRStickDownEffectImage.enabled = false;
                    break;
                case RStickImageType.Upper:
                    smashRStickUpEffectImage.enabled = true;
                    smashRStickDownEffectImage.enabled = false;
                    break;
                case RStickImageType.Downer:
                    smashRStickUpEffectImage.enabled = false;
                    smashRStickDownEffectImage.enabled = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(rStickImageType), rStickImageType, null);
            }
        }

        private enum RStickImageType {
            Neutral,
            Upper,
            Downer
        }

        private enum RStickActionType {
            Neutral,
            Smash,
            Hold
        }
    }

    public enum OutputActionType {
        HoldDown,
        SmashUp,

        HoldUp,
        SmashDown
    }
}