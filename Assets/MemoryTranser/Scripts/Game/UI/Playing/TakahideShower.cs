using System;
using Cysharp.Threading.Tasks;
using MemoryTranser.Scripts.Game.Util;
using UnityEngine;
using UnityEngine.UI;

namespace MemoryTranser.Scripts.Game.UI.Playing {
    public class TakahideShower : SingletonMonoBehaviour<TakahideShower> {
        protected override bool DontDestroy => false;

        [SerializeField] private Image currentTakahideImage;

        protected override void Awake() {
            base.Awake();
        }

        public async void ChangeTakahideImage(TakahideState takahideState) {
            currentTakahideImage.sprite = takahideState.ToTakahideSprite();

            await UniTask.Delay(TimeSpan.FromSeconds(2));

            currentTakahideImage.sprite = TakahideState.Thinking.ToTakahideSprite();
        }
    }

    public enum TakahideState {
        Thinking,
        Inspiration,
        Sad
    }
}