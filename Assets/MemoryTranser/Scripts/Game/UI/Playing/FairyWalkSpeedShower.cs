using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace MemoryTranser.Scripts.Game.UI.Playing {
    public class FairyWalkSpeedShower : MonoBehaviour {
        [SerializeField] private Slider fairyWalkSpeedSlider;
        [SerializeField] private Image fairyWalkSpeedPict;

        [Header("値の更新アニメーションに何秒かかるか")] [SerializeField]
        private float updateAnimationSec;

        [Header("ピクトさんのGradient")] [SerializeField]
        private Gradient fairyWalkSpeedGradient;

        [Header("×2のときの値")] [SerializeField] private float twoValue;
        [Header("×1のときの値")] [SerializeField] private float oneValue;
        [Header("×0のときの値")] [SerializeField] private float zeroValue;

        private void Awake() {
            fairyWalkSpeedSlider.value = oneValue;
        }

        public void SetWalkSpeedSlider(float currentPlainWalkSpeed, float initialWalkSpeed) {
            fairyWalkSpeedPict
                .DOGradientColor(fairyWalkSpeedGradient, updateAnimationSec * 3f)
                .SetLink(gameObject);

            if (currentPlainWalkSpeed < initialWalkSpeed) {
                fairyWalkSpeedSlider.DOValue(
                    (oneValue - zeroValue) / initialWalkSpeed * currentPlainWalkSpeed + zeroValue,
                    updateAnimationSec).SetLink(gameObject);
            }
            else {
                fairyWalkSpeedSlider.DOValue((twoValue - oneValue) / initialWalkSpeed * (currentPlainWalkSpeed -
                    initialWalkSpeed * (1 - oneValue / (twoValue - oneValue))), updateAnimationSec).SetLink(gameObject);
            }
        }
    }
}