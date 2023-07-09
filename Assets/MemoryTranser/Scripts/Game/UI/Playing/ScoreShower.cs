using DG.Tweening;
using MemoryTranser.Scripts.Game.GameManagers;
using TMPro;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.UI.Playing {
    public class ScoreShower : MonoBehaviour, IOnGameAwake {
        [SerializeField] private TextMeshProUGUI scoreText;

        public void OnGameAwake() {
            scoreText.SetText("0");
        }

        public void SetScoreText(int newScore) {
            var oldScore = int.Parse(scoreText.text);
            DOVirtual.Int(oldScore, newScore, 0.5f, value => { scoreText.text = value.ToString(); })
                .SetEase(Ease.OutCubic);
        }
    }
}