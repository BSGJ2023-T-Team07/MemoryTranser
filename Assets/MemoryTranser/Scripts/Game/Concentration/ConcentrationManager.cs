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

        [SerializeField] private float maxConcentration = 60f;

        private float _remainingConcentration;

        private bool _decreaseFlag;

        #endregion


        #region Unityから呼ばれる

        private void Update() {
            concentrationShower.SetValue(_remainingConcentration / maxConcentration);

            if (_decreaseFlag) {
                _remainingConcentration -= Time.deltaTime;
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