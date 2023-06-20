using UnityEngine;
using UnityEngine.UI;

namespace MemoryTranser.Scripts.Game.UI.Playing {
    public class ConcentrationShower : MonoBehaviour {
        [SerializeField] private Concentration.ConcentrationManager concentrationManager;
        [SerializeField] private Slider concentrationSlider;

        private void Update() {
            concentrationSlider.value = concentrationManager.GetRemainingConcentrationValue();
        }
    }
}