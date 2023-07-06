using System;
using Cysharp.Threading.Tasks;
using MemoryTranser.Scripts.Game.Util;
using UnityEngine;
using UnityEngine.UI;

namespace MemoryTranser.Scripts.Game.UI.Playing {
    public class TakahideShower : SingletonMonoBehaviour<TakahideShower> {
        protected override bool DontDestroy => false;

        [SerializeField] private Image currentTakahideImage;

        [Header("表情変化が継続する時間(秒)")] [SerializeField]
        private float changeDuration;

        private float _remainingChangeDuration;

        protected override void Awake() {
            base.Awake();
        }

        private void Update() {
            if (_remainingChangeDuration > 0) {
                _remainingChangeDuration -= Time.deltaTime;

                if (_remainingChangeDuration < 0) {
                    currentTakahideImage.sprite = TakahideState.Thinking.ToTakahideSprite();
                }
            }
        }

        public void ChangeTakahideImage(TakahideState takahideState) {
            currentTakahideImage.sprite = takahideState.ToTakahideSprite();
            _remainingChangeDuration = changeDuration;
        }
    }

    public enum TakahideState {
        Thinking,
        Inspiration,
        Sad
    }
}