using DG.Tweening;
using MemoryTranser.Scripts.Game.GameManagers;
using MemoryTranser.Scripts.Game.Sound;
using TMPro;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.UI.Result {
    public class ResultShower : MonoBehaviour, IOnGameAwake {
        [SerializeField] private Animator resultAnimator;

        [SerializeField] private TextMeshProUGUI totalScoreText;
        [SerializeField] private TextMeshProUGUI reachedPhaseCountText;
        [SerializeField] private TextMeshProUGUI reachedMaxComboCountText;
        [SerializeField] private TextMeshProUGUI backToTitleText;

        [SerializeField] private GameObject pauseBackGround;
        [SerializeField] private GameObject resultLayer;
        [SerializeField] private GameObject resultPaper;
        [SerializeField] private GameObject resultPaperStamp;

        private const float SCALE_MULTIPLIER = 2f;


        public bool IsAnimationCompleted => !resultAnimator.enabled;

        private void Awake() {
            //勝手に再生されないように止めておく
            resultAnimator.enabled = false;
        }

        public void OnGameAwake() {
            pauseBackGround.SetActive(false);
            resultPaper.SetActive(false);
            resultPaperStamp.SetActive(false);

            totalScoreText.SetText("");
            reachedPhaseCountText.SetText("");
            reachedMaxComboCountText.SetText("");

            totalScoreText.gameObject.SetActive(false);
            reachedPhaseCountText.gameObject.SetActive(false);
            reachedMaxComboCountText.gameObject.SetActive(false);
            backToTitleText.gameObject.SetActive(false);
        }

        public void ShowResult(int totalScore, int reachedPhaseCount, int reachedMaxComboCount) {
            pauseBackGround.SetActive(true);
            resultLayer.SetActive(true);
            resultPaper.SetActive(true);

            totalScoreText.SetText($"{totalScore}");
            reachedPhaseCountText.SetText($"{reachedPhaseCount}");
            reachedMaxComboCountText.SetText($"{reachedMaxComboCount}");

            resultAnimator.enabled = true;
            resultAnimator.Play("ResultShow");
        }


        // Called by Animation Event
        private void ShowReachedPhaseCount() {
            reachedPhaseCountText.gameObject.SetActive(true);

            var theTransform = reachedPhaseCountText.transform;
            theTransform.localScale *= SCALE_MULTIPLIER;

            theTransform.DOScale(theTransform.localScale / SCALE_MULTIPLIER, 0.1f).OnComplete(() => {
                SeManager.I.Play(SEs.ResultShow3);
            });
        }

        // Called by Animation Event
        private void ShowReachedMaxComboCount() {
            reachedMaxComboCountText.gameObject.SetActive(true);

            var theTransform = reachedMaxComboCountText.transform;
            theTransform.localScale *= SCALE_MULTIPLIER;

            theTransform.DOScale(theTransform.localScale / SCALE_MULTIPLIER, 0.1f).OnComplete(() => {
                SeManager.I.Play(SEs.ResultShow3);
            });
        }

        // Called by Animation Event
        private void ShowTotalScore() {
            totalScoreText.gameObject.SetActive(true);
            resultPaperStamp.SetActive(true);

            var theTransform0 = totalScoreText.transform;
            var theTransform1 = resultPaperStamp.transform;

            theTransform0.localScale *= SCALE_MULTIPLIER;
            theTransform1.localScale *= SCALE_MULTIPLIER;

            theTransform0.DOScale(theTransform0.localScale / SCALE_MULTIPLIER, 0.1f).SetLink(gameObject);
            theTransform1.DOScale(theTransform1.localScale / SCALE_MULTIPLIER, 0.1f).OnComplete(() => {
                SeManager.I.Play(SEs.ResultStamp);
            }).SetLink(gameObject);
        }


        // Called by Animation Event
        private void ShowBackToTitleText() {
            backToTitleText.gameObject.SetActive(true);
            backToTitleText.DOFade(0.1f, 1f).SetLoops(-1, LoopType.Yoyo);
            resultAnimator.enabled = false;
        }
    }
}