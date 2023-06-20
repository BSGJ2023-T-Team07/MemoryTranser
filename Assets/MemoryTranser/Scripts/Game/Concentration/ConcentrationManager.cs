using System;
using MemoryTranser.Scripts.Game.GameManagers;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.Concentration {
    public class ConcentrationManager : MonoBehaviour, IOnStateChangedToInitializing, IOnStateChangedToReady,
        IOnStateChangedToPlaying, IOnStateChangedToResult {
        #region 変数の定義

        private float _remainingConcentration;
        private float _maxConcentration = 60f;

        private bool _decreaseFlag;

        #endregion


        #region Unityから呼ばれる

        private void Update() {
            if (_decreaseFlag) _remainingConcentration -= Time.deltaTime;

            if (_remainingConcentration < 0) {
                _remainingConcentration = 0;
                GameFlowManager.I.ChangeGameState(GameState.Result);
            }
        }

        #endregion

        #region interfaceの実装

        public void OnStateChangedToInitializing() {
            _remainingConcentration = _maxConcentration;
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
            _remainingConcentration = Mathf.Min(_maxConcentration, _remainingConcentration + addition);
        }

        public float GetRemainingConcentrationValue() {
            return _remainingConcentration / _maxConcentration;
        }
    }
}