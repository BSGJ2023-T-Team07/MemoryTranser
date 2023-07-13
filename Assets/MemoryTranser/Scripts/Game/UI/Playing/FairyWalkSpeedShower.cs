using System;
using UnityEngine;
using UnityEngine.UI;

namespace MemoryTranser.Scripts.Game.UI.Playing {
    public class FairyWalkSpeedShower : MonoBehaviour {
        [SerializeField] private Slider fairyWalkSpeedSlider;

        [Header("初期値の何倍まで見せるか")] [SerializeField]
        private float maxWalkSpeedShowMultiplier;

        private void Awake() {
            fairyWalkSpeedSlider.value = 0f;
        }

        public void SetWalkSpeedSlider(float plainWalkSpeed, float initialWalkSpeed) {
            fairyWalkSpeedSlider.value =
                1f / (initialWalkSpeed * maxWalkSpeedShowMultiplier - initialWalkSpeed) *
                (plainWalkSpeed - initialWalkSpeed);
        }
    }
}