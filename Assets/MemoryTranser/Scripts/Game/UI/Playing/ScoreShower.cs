using TMPro;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.UI {
    public class ScoreShower : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI scoreText;

        public void SetScoreText(int score) {
            scoreText.text = score.ToString();
        }
    }
}