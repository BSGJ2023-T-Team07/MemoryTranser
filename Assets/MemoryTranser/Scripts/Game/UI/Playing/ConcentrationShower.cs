using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace MemoryTranser.Scripts.Game.UI.Playing {
    public class ConcentrationShower : MonoBehaviour {
        [SerializeField] private Slider concentrationSlider;
        [SerializeField] private Transform concentrationTransform;
        [SerializeField] private Image concentrationImage;

        public void SetValue(float value) {
            concentrationSlider.value = value;
        }

        public void PlayAnimationWhenPinch() {
            concentrationTransform.DOShakePosition(2f, 10f, 20).SetLink(gameObject);
            concentrationImage.DOColor(Color.red, 0.5f).SetLoops(4, LoopType.Yoyo).SetLink(gameObject);
        }

        public void PlayAnimationWhenIncrease() { }
        public void PlayAnimationWhenDecrease() { }
    }
}