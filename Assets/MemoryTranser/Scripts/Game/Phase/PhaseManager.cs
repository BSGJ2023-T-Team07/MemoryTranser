using System;
using System.Collections.Generic;
using MemoryTranser.Scripts.Game.Desire;
using MemoryTranser.Scripts.Game.GameManagers;
using MemoryTranser.Scripts.Game.MemoryBox;
using MemoryTranser.Scripts.Game.UI;
using UnityEngine;
using UniRx;

namespace MemoryTranser.Scripts.Game.Phase {
    public class PhaseManager : MonoBehaviour {
        #region コンポーネントの定義

        [SerializeField] private QuestTypeShower questTypeShower;
        [SerializeField] private ScoreShower scoreShower;
        [SerializeField] private DesireCore desireCore;
        [SerializeField] private MemoryBoxManager memoryBoxManager;

        #endregion

        #region 変数の定義

        [Header("最初に生成されるフェイスの数")] [SerializeField]
        private int initialPhaseCount = 20;

        [Header("UIで見ることのできるフェイズの数")] [SerializeField]
        private int viewablePhaseCount = 3;

        [Header("MemoryBoxの発生確率に関わるフェイズの数")] [SerializeField]
        private int memoryBoxProbabilityPhaseCount = 5;

        [Header("値が大きいほど直近のフェイズに対応するMemoryBoxが増える")] [SerializeField]
        private int memoryBoxProbabilityWeight = 5;

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

        private void Awake() {
            //DesireCoreが攻撃されたらスコアを加算する
            desireCore.OnAttacked.Subscribe(_ => { scoreShower.SetScoreText(AddScore(_currentPhaseIndex, 5)); });
        }

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


        #region public関数

        /// <summary>
        /// フェイズの初期化
        /// </summary>
        public void InitializePhases() {
            _phaseCores = new List<PhaseCore>();
            for (var i = 0; i < initialPhaseCount; i++) {
                _phaseCores.Add(GenerateRandomPhase(ScriptableObject.CreateInstance<PhaseCore>()));
            }

            SetBoxGenerationProbability();
        }

        /// <summary>
        /// 現在のフェイズのスコアを正誤表から加算する
        /// </summary>
        /// <param name="errataList">正誤表</param>
        public void AddCurrentScoreByErrataList(bool[] errataList) {
            AddScore(_currentPhaseIndex, CalculateScoreByErrata(errataList));
        }

        /// <summary>
        /// Phaseの残り時間をリセットする
        /// </summary>
        public void ResetRemainingTime() {
            _phaseRemainingTime = PHASE_DURATION;
        }

        /// <summary>
        /// 現在のPhaseのQuestTypeを取得する
        /// </summary>
        /// <returns></returns>
        public BoxMemoryType GetCurrentQuestType() {
            return GetQuestType(_currentPhaseIndex);
        }

        public void UpdatePhaseText() {
            scoreShower.SetScoreText(_phaseCores[_currentPhaseIndex].Score);
            questTypeShower.SetQuestTypeText(GetCurrentQuestType());
        }

        /// <summary>
        /// デバッグ用関数
        /// </summary>
        /// <returns></returns>
        public (PhaseCore[], int) GetPhaseInformation() {
            return (_phaseCores.ToArray(), _currentPhaseIndex);
        }

        #endregion

        #region private関数

        /// <summary>
        /// 引数のPhaseCoreにランダムなQuestTypeを設定する
        /// </summary>
        /// <param name="phaseCore"></param>
        /// <returns>設定されたPhaseCoreが返ってくる</returns>
        private PhaseCore GenerateRandomPhase(PhaseCore phaseCore) {
            var randomPhaseType = (BoxMemoryType)UnityEngine.Random.Range(1, _maxPhaseTypeCount);
            phaseCore.QuestType = randomPhaseType;

            return phaseCore;
        }

        /// <summary>
        /// 正誤表からスコアを計算する
        /// </summary>
        /// <param name="errataList"></param>
        /// <returns>スコアが返ってくる</returns>
        private int CalculateScoreByErrata(bool[] errataList) {
            var trueCount = 0;
            var falseCount = 0;

            foreach (var errata in errataList) {
                if (errata) trueCount++;
                else falseCount++;
            }

            return Mathf.Clamp(trueCount * 20 - falseCount * 10, 0, 100);
        }

        /// <summary>
        /// スコアを足す
        /// </summary>
        /// <param name="phaseIndex">スコアを足すフェイズのインデックス</param>
        /// <param name="score">足すスコア</param>
        private int AddScore(int phaseIndex, int score) {
            var phaseCore = _phaseCores[phaseIndex];
            phaseCore.Score += score;
            return phaseCore.Score;
        }

        /// <summary>
        /// 次のフェイズに移行する
        /// </summary>
        private void TransitToNextPhase() {
            _currentPhaseIndex++;
        }

        /// <summary>
        /// QuestTypeを取得する
        /// </summary>
        /// <param name="phaseIndex">取得するPhaseのインデックス</param>
        /// <returns></returns>
        private BoxMemoryType GetQuestType(int phaseIndex) {
            return _phaseCores[phaseIndex].QuestType;
        }

        private void SetBoxGenerationProbability() {
            var nextQuestTypeInts = new List<int>();
            for (var i = _currentPhaseIndex; i <= _currentPhaseIndex + memoryBoxProbabilityPhaseCount; i++) {
                var questTypeInt = (int)GetQuestType(i);
                var questTypeInts = new List<int>();
                
                //直近のフェイズになるほど対応するMemoryBoxが出やすくなる
                for (var j = 0; j < memoryBoxProbabilityWeight * 2 - (i - _currentPhaseIndex); j++) {
                    questTypeInts.Add(questTypeInt);
                }

                nextQuestTypeInts.AddRange(questTypeInts);
            }

            memoryBoxManager.AddProbability(nextQuestTypeInts);
        }

        #endregion
    }
}