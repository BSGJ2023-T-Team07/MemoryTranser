using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace MemoryTranser.Scripts.Game.UI.Playing {
    public class ConcentrationShower : MonoBehaviour {
        [SerializeField] private Slider concentrationSlider;
        [SerializeField] private Transform concentrationTransform;

        public void SetValue(float value) {
            concentrationSlider.value = value;
        }
        
        public void PlayAnimationWhenPinch() {
            concentrationTransform.DOShakePosition(0.5f, 10f, 20, 90f, false, true);
        }
    }
}