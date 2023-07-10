using DG.Tweening;
using MemoryTranser.Scripts.Game.GameManagers;
using TMPro;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.UI.Playing {
    public class GameFlowShower : MonoBehaviour {
        [SerializeField] private GameObject pauseBackGround;
        [SerializeField] private TextMeshProUGUI countDownText;
        [SerializeField] private GameObject countDownLayer;

        private Sequence _countdownSequence;

        private float _remainingCountDown;

        private const int COUNTDOWN_SEC = 3;

        public Sequence CountdownSequence => _countdownSequence;

        private void Start() {
            pauseBackGround.SetActive(true);

            _countdownSequence = DOTween.Sequence();

            _countdownSequence.Append(DOVirtual.Int(COUNTDOWN_SEC, 0, COUNTDOWN_SEC,
                    i => { countDownText.SetText(i == 0 ? "  テスト開始！" : i.ToString()); }).OnStart(() => {
                    countDownLayer.SetActive(true);
                })
                .OnComplete(() => {
                    countDownLayer.SetActive(false);
                    pauseBackGround.SetActive(false);
                    GameFlowManager.I.ChangeGameState(GameState.Initializing);
                }));
        }
    }
}