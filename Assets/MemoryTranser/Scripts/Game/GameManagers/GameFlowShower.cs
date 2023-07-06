using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.GameManagers {
    public class GameFlowShower : MonoBehaviour {
        [SerializeField] private GameObject pauseBackGround;
        [SerializeField] private TextMeshProUGUI countDownText;
        [SerializeField] private GameObject countDownLayer;

        private Sequence _countdownSequence;

        private float _remainingCountDown;

        private const int COUNTDOWN_SEC = 3;

        public Sequence CountdownSequence => _countdownSequence;

        private void Awake() {
            pauseBackGround.SetActive(true);
        }

        private void Start() {
            _countdownSequence = DOTween.Sequence();

            _countdownSequence.Append(DOVirtual.Int(COUNTDOWN_SEC, 0, COUNTDOWN_SEC,
                    i => { countDownText.text = i == 0 ? "  テスト開始！" : i.ToString(); }).OnStart(() => {
                    countDownLayer.SetActive(true);
                })
                .OnComplete(() => {
                    countDownLayer.SetActive(false);
                    pauseBackGround.SetActive(false);
                }));
        }
    }
}