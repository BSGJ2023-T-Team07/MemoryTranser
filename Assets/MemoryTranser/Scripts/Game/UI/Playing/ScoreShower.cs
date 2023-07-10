using System.Linq;
using DG.Tweening;
using MemoryTranser.Scripts.Game.GameManagers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MemoryTranser.Scripts.Game.UI.Playing {
    public class ScoreShower : MonoBehaviour, IOnGameAwake {
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private Image scorePaper;

        [Header("波紋アニメーションを発火させる点数")] [SerializeField]
        private int[] scoreThresholds;

        public void OnGameAwake() {
            scoreText.SetText("0");
        }


        public void SetScoreText(int newScore) {
            var oldScore = int.Parse(scoreText.text);
            DOVirtual.Int(oldScore, newScore, 0.5f, value => { scoreText.text = value.ToString(); })
                     .SetEase(Ease.OutCubic);

            #region 波紋アニメーションの処理

            var isOverThreshold = scoreThresholds.Aggregate(false,
                (current, threshold) => current || (oldScore < threshold && newScore >= threshold));

            if (oldScore < newScore && isOverThreshold) {
                PlayRippleAnimation();
            }

            #endregion
        }

        private void PlayRippleAnimation() {
            var rippleText = Instantiate(scoreText, scoreText.transform.parent);
            var ripplePaper = Instantiate(scorePaper, scorePaper.transform.parent);

            const float scaleMultiplier = 1.2f;
            const float duration = 1f;

            
            rippleText.transform.DOScale(rippleText.transform.localScale * scaleMultiplier, duration)
                      .SetEase(Ease.OutCubic);
            ripplePaper.transform.DOScale(ripplePaper.transform.localScale * scaleMultiplier, duration)
                       .SetEase(Ease.OutCubic);

            rippleText.DOFade(0f, duration).SetEase(Ease.OutCubic)
                      .OnComplete(() => { Destroy(rippleText.gameObject); });
            ripplePaper.DOFade(0f, duration).SetEase(Ease.OutCubic)
                       .OnComplete(() => { Destroy(ripplePaper.gameObject); });
        }
    }
}