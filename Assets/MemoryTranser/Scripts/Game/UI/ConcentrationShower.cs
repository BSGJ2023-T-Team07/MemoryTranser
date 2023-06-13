using TMPro;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.UI {
    public class ConcentrationShower : MonoBehaviour {
        [SerializeField] private Concentration.ConcentrationManager concentrationManager;
        [SerializeField] private TextMeshProUGUI concentrationText;

        private void Update() {
            concentrationText.text = $"RemainingConcentration: {concentrationManager.RemainingConcentration}";
        }
    }
}