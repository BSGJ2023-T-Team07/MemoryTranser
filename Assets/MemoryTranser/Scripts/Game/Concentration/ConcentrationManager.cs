using MemoryTranser.Scripts.Game.GameManagers;
using MemoryTranser.Scripts.Game.Sound;
using MemoryTranser.Scripts.Game.UI.Playing;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.Concentration {
    public class ConcentrationManager : MonoBehaviour, IOnStateChangedToInitializing, IOnStateChangedToReady,
        IOnStateChangedToPlaying, IOnStateChangedToResult {
        #region コンポーネントの定義

        [SerializeField] private ConcentrationShower concentrationShower;

        #endregion

        #region 変数の定義

        [Header("完全放置で集中力が持つ時間(秒)(つまりゲージの長さ)")] [SerializeField]
        private float maxConcentration;

        [Header("減少速度が上がる間隔(秒)")] [SerializeField]
        private float additionalDecreaseInterval;

        [Header("↑の時間が経った時に加算される減少速度の割合(％)")] [SerializeField]
        private float additionalDecreasePercent;

        [Header("減少速度の最大倍率(％)")] [SerializeField]
        private float maxDecreaseMultiplier;

        [Header("集中力がどの時にピンチSEを鳴らすか")] [SerializeField]
        private float pinchSeThreshold;

        [Header("集中力がピンチの時のBGMの速度倍率")] [SerializeField]
        private float pinchBgmPitch;

        private float _remainingConcentration;
        private float _remainingTimeForAdditionalDecrease;
        private float _decreaseMultiplier = 1f;

        private bool _decreaseFlag;
        private bool _isPinchEffectPlayed;

        #endregion


        #region Unityから呼ばれる

        private void Update() {
            concentrationShower.SetValue(_remainingConcentration / maxConcentration);

            //ピンチになったあと回復する時に対応
            if (_isPinchEffectPlayed && _remainingConcentration > pinchSeThreshold) {
                _isPinchEffectPlayed = false;
                BgmManager.I.SetBgmPitch(1f);
            }

            if (_decreaseFlag) {
                _remainingConcentration -= Time.deltaTime * _decreaseMultiplier;

                if (!_isPinchEffectPlayed && _remainingConcentration < pinchSeThreshold) {
                    SeManager.I.Play(SEs.ConcentrationIsLittle);
                    BgmManager.I.SetBgmPitch(pinchBgmPitch);
                    concentrationShower.PlayAnimationWhenPinch();
                    _isPinchEffectPlayed = true;
                }

                _remainingTimeForAdditionalDecrease -= Time.deltaTime;
            }

            if (_remainingTimeForAdditionalDecrease < 0) {
                _remainingTimeForAdditionalDecrease = additionalDecreaseInterval;
                _decreaseMultiplier =
                    Mathf.Min(_decreaseMultiplier + additionalDecreasePercent / 100, maxDecreaseMultiplier / 100);
            }

            if (_remainingConcentration < 0) {
                _remainingConcentration = 0;
                GameFlowManager.I.ChangeGameState(GameState.Result);
            }
        }

        #endregion

        #region interfaceの実装

        public void OnStateChangedToInitializing() {
            _remainingConcentration = maxConcentration;
            _remainingTimeForAdditionalDecrease = additionalDecreaseInterval;
        }

        public void OnStateChangedToReady() {
            _decreaseFlag = false;
        }

        public void OnStateChangedToPlaying() {
            _decreaseFlag = true;
        }

        public void OnStateChangedToResult() {
            _decreaseFlag = false;
        }

        #endregion


        public void AddConcentration(float addition) {
            _remainingConcentration = Mathf.Min(maxConcentration, _remainingConcentration + addition);
        }
    }
}