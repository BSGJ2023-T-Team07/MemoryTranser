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
            //スコアが変化しきるのにかかる時間(秒)
            const float duration = 0.5f;

            //通常の波紋アニメーションの大きさの倍率
            const float normalScaleMultiplier = 1.2f;

            //大きな波紋アニメーションの大きさの倍率
            const float bigScaleMultiplier = 1.5f;

            var oldScore = int.Parse(scoreText.text);
            DOVirtual.Int(oldScore, newScore, duration, value => { scoreText.text = value.ToString(); })
                .SetEase(Ease.OutCubic);

            if (oldScore < newScore) {
                PlayRippleAnimation(normalScaleMultiplier);
            }

            #region 波紋アニメーションの処理

            var isOverThreshold = scoreThresholds.Aggregate(false,
                (current, threshold) => current || (oldScore < threshold && newScore >= threshold));

            if (oldScore < newScore && isOverThreshold) {
                PlayRippleAnimation(bigScaleMultiplier);
            }

            #endregion
        }

        private void PlayRippleAnimation(float scaleMultiplier) {
            var rippleText = Instantiate(scoreText, scoreText.transform.parent);
            var ripplePaper = Instantiate(scorePaper, scorePaper.transform.parent);

            //波紋アニメーションの時間(秒)
            const float duration = 1f;

            rippleText.transform.DOScale(rippleText.transform.localScale * scaleMultiplier, duration)
                .SetEase(Ease.OutCubic).SetLink(gameObject);
            ripplePaper.transform.DOScale(ripplePaper.transform.localScale * scaleMultiplier, duration)
                .SetEase(Ease.OutCubic).SetLink(gameObject);

            rippleText.DOFade(0f, duration).SetEase(Ease.OutCubic)
                .OnComplete(() => { Destroy(rippleText.gameObject); }).SetLink(gameObject);
            ripplePaper.DOFade(0f, duration).SetEase(Ease.OutCubic)
                .OnComplete(() => { Destroy(ripplePaper.gameObject); }).SetLink(gameObject);
        }
    }
}