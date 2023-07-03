using MemoryTranser.Scripts.Game.GameManagers;
using MemoryTranser.Scripts.Game.UI.Playing;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.Concentration {
    public class ConcentrationManager : MonoBehaviour, IOnStateChangedToInitializing, IOnStateChangedToReady,
        IOnStateChangedToPlaying, IOnStateChangedToResult {
        #region コンポーネントの定義

        [SerializeField] private ConcentrationShower concentrationShower;

        #endregion

        #region 変数の定義

        [Header("完全放置で集中力が持つ時間(秒)")] [SerializeField]
        private float maxConcentration;

        [Header("減少速度が上がる間隔(秒)")] [SerializeField]
        private float additionalDecreaseInterval;

        [Header("↑の時間が経った時に加算される減少速度の割合(％)")] [SerializeField]
        private float additionalDecreasePercent;

        private float _remainingConcentration;
        private float _remainingTimeForAdditionalDecrease;
        private float _decreaseMultiplier = 1f;

        private bool _decreaseFlag;

        #endregion


        #region Unityから呼ばれる

        private void Update() {
            concentrationShower.SetValue(_remainingConcentration / maxConcentration);

            if (_decreaseFlag) {
                _remainingConcentration -= Time.deltaTime * _decreaseMultiplier;
                _remainingTimeForAdditionalDecrease -= Time.deltaTime;
            }

            if (_remainingTimeForAdditionalDecrease < 0) {
                _remainingTimeForAdditionalDecrease = additionalDecreaseInterval;
                _decreaseMultiplier += additionalDecreasePercent / 100;
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