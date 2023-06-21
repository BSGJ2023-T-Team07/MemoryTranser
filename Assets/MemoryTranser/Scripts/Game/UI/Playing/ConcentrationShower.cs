using UnityEngine;
using UnityEngine.UI;

namespace MemoryTranser.Scripts.Game.UI.Playing {
    public class ConcentrationShower : MonoBehaviour {
        [SerializeField] private Slider concentrationSlider;

        public void SetValue(float value) {
            concentrationSlider.value = value;
        }
    }
}