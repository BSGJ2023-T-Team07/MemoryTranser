using System;
using MemoryTranser.Scripts.Game.GameManagers;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.Concentration {
    public class ConcentrationManager : MonoBehaviour {
        #region 変数の定義

        private float _remainingConcentration;
        private float _maxConcentration = 60f;

        private bool _decreaseFlag;

        #endregion


        #region プロパティーの定義

        public bool DecreaseFlag {
            get => _decreaseFlag;
            set => _decreaseFlag = value;
        }

        #endregion

        #region Unityから呼ばれる

        private void Update() {
            if (DecreaseFlag) DecreaseConcentration();

            if (_remainingConcentration < 0) {
                _remainingConcentration = 0;
                GameFlowManager.I.ChangeGameState(GameState.Result);
            }
        }

        #endregion

        public void InitializeConcentration() {
            _remainingConcentration = _maxConcentration;
        }

        private void DecreaseConcentration() {
            _remainingConcentration -= Time.deltaTime;
        }

        public void AddConcentration(float addition) {
            _remainingConcentration = Mathf.Min(_maxConcentration, _remainingConcentration + addition);
        }

        public float GetRemainingConcentrationValue() {
            return _remainingConcentration / _maxConcentration;
        }
    }
}