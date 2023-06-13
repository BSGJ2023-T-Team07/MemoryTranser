using System;
using MemoryTranser.Scripts.Game.GameManagers;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.Concentration {
    public class ConcentrationManager : MonoBehaviour {
        #region 変数の定義

        private float _remainingConcentration;
        private float _maxConcentration = 120f;

        private bool _decreaseFlag;

        #endregion


        #region プロパティーの定義

        public float RemainingConcentration {
            get => _remainingConcentration;
            set => _remainingConcentration = value;
        }

        public bool DecreaseFlag {
            get => _decreaseFlag;
            set => _decreaseFlag = value;
        }

        #endregion


        public void InitializeConcentration() {
            RemainingConcentration = _maxConcentration;
        }


        private void Update() {
            if (DecreaseFlag) DecreaseConcentration();

            if (RemainingConcentration <= 0) GameFlowManager.I.ChangeGameState(GameState.Result);
        }

        private void DecreaseConcentration() {
            RemainingConcentration -= Time.deltaTime;
        }

        public void AddConcentration(float addition) {
            RemainingConcentration += addition;
        }
    }
}