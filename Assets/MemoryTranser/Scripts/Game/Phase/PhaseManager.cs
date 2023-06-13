using System;
using System.Collections.Generic;
using MemoryTranser.Scripts.Game.MemoryBox;
using MemoryTranser.Scripts.Game.UI;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.Phase {
    public class PhaseManager : MonoBehaviour {
        #region コンポーネントの定義

        [SerializeField] private PhaseShower phaseShower;

        #endregion

        #region 変数の定義

        private List<PhaseCore> _phaseCores;

        private int _initialPhaseCount = 5;
        private int _viewablePhaseCount = 3;
        private int _maxPhaseTypeCount = Enum.GetValues(typeof(BoxMemoryType)).Length;

        private int _currentPhaseIndex = 0;

        #endregion

        #region プロパティーの定義

        public int CurrentPhaseIndex {
            get => _currentPhaseIndex;
            set => _currentPhaseIndex = value;
        }

        #endregion


        public void InitializePhases() {
            _phaseCores = new List<PhaseCore>();
            for (var i = 0; i < _initialPhaseCount; i++) {
                _phaseCores.Add(GenerateRandomPhase(ScriptableObject.CreateInstance<PhaseCore>()));
            }
        }


        //ゲームが始まる時の最初のフェイズ初期化
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

        public void TransitToNextPhase() {
            CurrentPhaseIndex++;
        }

        private BoxMemoryType GetQuestType(int phaseIndex) {
            return _phaseCores[phaseIndex].QuestType;
        }

        public BoxMemoryType GetCurrentQuestType() {
            return GetQuestType(CurrentPhaseIndex);
        }

        public void UpdatePhaseText() {
            phaseShower.SetPhaseText(_phaseCores.ToArray(), _currentPhaseIndex);
        }
    }
}