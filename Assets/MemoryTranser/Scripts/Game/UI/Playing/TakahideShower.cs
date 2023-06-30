using System;
using UnityEngine;
using UnityEngine.UI;

namespace MemoryTranser.Scripts.Game.UI.Playing {
    public class TakahideShower : MonoBehaviour {
        [SerializeField] private Image currentTakahideImage;

        private void Awake() { }

        public void ChangeTakahideImage(TakahideState takahideState) { }
    }

    public enum TakahideState {
        Thinking,
        Happy,
        Sad
    }
}