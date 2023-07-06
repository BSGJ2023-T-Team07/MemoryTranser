using UnityEngine;

namespace MemoryTranser.Scripts.Game.Output {
    [RequireComponent(typeof(BoxCollider2D))]
    public class OutputButton : MonoBehaviour {
        [SerializeField] private OutputManager outputManager;

        private void OnTriggerEnter2D(Collider2D other) {
            if (!other.gameObject.CompareTag("Fairy")) {
                return;
            }
            // outputManager.OutputBoxes();
        }
    }
}