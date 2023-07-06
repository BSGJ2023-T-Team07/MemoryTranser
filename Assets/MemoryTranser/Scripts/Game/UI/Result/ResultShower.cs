using System;
using DG.Tweening;
using MemoryTranser.Scripts.Game.GameManagers;
using MemoryTranser.Scripts.Game.Sound;
using MemoryTranser.Scripts.Game.Util;
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

        private void Awake() {
            resultAnimator.enabled = false;
        }

        public void OnGameAwake() {
            resultLayer.SetActive(true);

            pauseBackGround.SetActive(false);
            resultPaper.SetActive(false);
            resultPaperStamp.SetActive(false);

            totalScoreText.text = "";
            reachedPhaseCountText.text = "";
            reachedMaxComboCountText.text = "";

            totalScoreText.gameObject.SetActive(false);
            reachedPhaseCountText.gameObject.SetActive(false);
            reachedMaxComboCountText.gameObject.SetActive(false);
            backToTitleText.gameObject.SetActive(false);

            resultLayer.SetActive(false);
        }

        public void ShowResult(int totalScore, int reachedPhaseCount, int reachedMaxComboCount) {
            pauseBackGround.SetActive(true);
            resultLayer.SetActive(true);
            resultPaper.SetActive(true);

            totalScoreText.text = $"{totalScore}";
            reachedPhaseCountText.text = $"{reachedPhaseCount}";
            reachedMaxComboCountText.text = $"{reachedMaxComboCount}";

            resultAnimator.enabled = true;
            resultAnimator.Play("ResultShow");
        }

        private void ShowReachedPhaseCount() {
            reachedPhaseCountText.gameObject.SetActive(true);
            SeManager.I.Play(SEs.ResultShow1);
        }

        private void ShowReachedMaxComboCount() {
            reachedMaxComboCountText.gameObject.SetActive(true);
            SeManager.I.Play(SEs.ResultShow1);
        }

        private void ShowTotalScore() {
            totalScoreText.gameObject.SetActive(true);
            resultPaperStamp.SetActive(true);
            SeManager.I.Play(SEs.ResultShow2);
        }

        private void ShowBackToTitleText() {
            backToTitleText.gameObject.SetActive(true);
            backToTitleText.DOFade(0.1f, 1f).SetLoops(-1, LoopType.Yoyo);
        }
    }
}