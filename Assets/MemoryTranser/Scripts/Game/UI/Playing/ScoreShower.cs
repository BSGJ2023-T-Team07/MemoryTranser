using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.UI.Playing {
    public class ScoreShower : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI scoreText;

        private void Awake() {
            scoreText.text = "0";
        }

        public void SetScoreText(int newScore) {
            var oldScore = int.Parse(scoreText.text);
            DOVirtual.Int(oldScore, newScore, 0.5f, value => { scoreText.text = value.ToString(); })
                .SetEase(Ease.OutCubic);
        }
    }
}