using System;
using System.Collections.Generic;
using MemoryTranser.Scripts.Game.GameManagers;
using MemoryTranser.Scripts.Game.MemoryBox;
using MemoryTranser.Scripts.Game.UI;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.Phase {
    public class PhaseManager : MonoBehaviour {
        #region コンポーネントの定義

        [SerializeField] private PhaseShower phaseShower;

        #endregion

        #region 変数の定義

        [SerializeField] private int initialPhaseCount = 5;
        [SerializeField] private int viewablePhaseCount = 3;

        private List<PhaseCore> _phaseCores;
        private int _maxPhaseTypeCount = (int)BoxMemoryType.Count;

        private int _currentPhaseIndex = 0;
        private float _phaseRemainingTime;

        #region 定数の定義

        public const float PHASE_DURATION = 30f;

        #endregion

        #endregion

        #region プロパティーの定義

        public float RemainingTime => _phaseRemainingTime;

        #endregion

        #region Unityから呼ばれる

        private void Start() {
            _phaseRemainingTime = PHASE_DURATION;
        }

        private void Update() {
            if (GameFlowManager.I.NowGameState != GameState.Playing) return;

            _phaseRemainingTime -= Time.deltaTime;
            if (_phaseRemainingTime <= 0) {
                TransitToNextPhase();
                _phaseRemainingTime = PHASE_DURATION;
                GameFlowManager.I.ChangeGameState(GameState.Ready);
            }
        }

        #endregion


        //ゲームが始まる時の最初のフェイズ初期化
        public void InitializePhases() {
            _phaseCores = new List<PhaseCore>();
            for (var i = 0; i < initialPhaseCount; i++) {
                _phaseCores.Add(GenerateRandomPhase(ScriptableObject.CreateInstance<PhaseCore>()));
            }
        }


        private PhaseCore GenerateRandomPhase(PhaseCore phaseCore) {
            var randomPhaseType = (BoxMemoryType)UnityEngine.Random.Range(1, _maxPhaseTypeCount);
            phaseCore.QuestType = randomPhaseType;

            return phaseCore;
        }

        private void CalculateScore(int currentPhaseIndex, bool[] errataList) {
            var trueCount = 0;
            var falseCount = 0;

            foreach (var errata in errataList) {
                if (errata) trueCount++;
                else falseCount++;
            }

            _phaseCores[currentPhaseIndex].Score = Mathf.Clamp(trueCount * 20 - falseCount * 10, 0, 100);
        }

        public void CalculateCurrentScore(bool[] errataList) {
            CalculateScore(_currentPhaseIndex, errataList);
        }

        private void TransitToNextPhase() {
            _currentPhaseIndex++;
        }

        private BoxMemoryType GetQuestType(int phaseIndex) {
            return _phaseCores[phaseIndex].QuestType;
        }

        public BoxMemoryType GetCurrentQuestType() {
            return GetQuestType(_currentPhaseIndex);
        }

        public void UpdatePhaseText() {
            phaseShower.SetPhaseText(_phaseCores.ToArray(), _currentPhaseIndex);
        }

        public void ResetRemainingTime() {
            _phaseRemainingTime = PHASE_DURATION;
        }
    }
}