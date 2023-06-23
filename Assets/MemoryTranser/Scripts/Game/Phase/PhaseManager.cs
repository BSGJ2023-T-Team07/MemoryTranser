using System.Collections.Generic;
using MemoryTranser.Scripts.Game.GameManagers;
using MemoryTranser.Scripts.Game.MemoryBox;
using MemoryTranser.Scripts.Game.UI.Playing;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.Phase {
    public class PhaseManager : MonoBehaviour, IOnStateChangedToInitializing, IOnStateChangedToReady {
        #region コンポーネントの定義

        [SerializeField] private QuestTypeShower questTypeShower;
        [SerializeField] private ScoreShower scoreShower;
        [SerializeField] private MemoryBoxManager memoryBoxManager;

        #endregion

        #region 変数の定義

        [Header("Desireを倒したときに加算される点数")] [SerializeField]
        private int additionalScoreOnDefeatDesire;

        [Header("最初に生成されるフェイスの数")] [SerializeField]
        private int initialPhaseCount = 20;

        [Header("UIで見ることのできるフェイズの数")] [SerializeField]
        private int viewablePhaseCount = 3;

        [Header("MemoryBoxの発生確率に関わるフェイズの数")] [SerializeField]
        private int memoryBoxProbabilityPhaseCount = 5;

        [Header("値が大きいほど直近のフェイズに対応するMemoryBoxが増える")] [SerializeField]
        private int memoryBoxProbabilityWeight = 5;

        private List<PhaseCore> _phaseCores = new();

        private int _currentPhaseIndex = 0;
        private float _phaseRemainingTime;

        #region 定数の定義

        //フェイズの長さ
        public const float PHASE_DURATION = 30f;

        #endregion

        #endregion

        #region プロパティーの定義

        public float RemainingTime => _phaseRemainingTime;

        #endregion

        #region Unityから呼ばれる

        private void Awake() {
            _phaseRemainingTime = PHASE_DURATION;
        }

        private void Update() {
            if (GameFlowManager.I.NowGameState != GameState.Playing) return;

            //NowGameStateがPlayingの時のみ残り時間を減らす
            _phaseRemainingTime -= Time.deltaTime;

            //フェイズの残り時間が0になったら、次のフェイズに移行する
            if (_phaseRemainingTime <= 0) {
                TransitToNextPhase();
                _phaseRemainingTime = PHASE_DURATION;
                GameFlowManager.I.ChangeGameState(GameState.Ready);
            }
        }

        #endregion


        #region interfaceの実装

        public void OnStateChangedToInitializing() {
            InitializePhases();
        }

        public void OnStateChangedToReady() {
            UpdatePhaseText();
            ResetRemainingTime();
        }

        #endregion

        #region public関数

        /// <summary>
        /// 現在のフェイズの点数を加算する
        /// </summary>
        /// <param name="score"></param>
        public void AddCurrentScore(int score) {
            AddScore(_currentPhaseIndex, score);
        }

        /// <summary>
        /// Desireを倒したときに点数を加算する
        /// </summary>
        public void AddCurrentScoreOnDefeatDesire() {
            AddScore(_currentPhaseIndex, additionalScoreOnDefeatDesire);
        }

        /// <summary>
        /// 納品されたMemoryBoxの情報をもとにスコアを計算する
        /// </summary>
        /// <param name="boxes"></param>
        /// <returns>点数、正答数、誤答数の組を返す</returns>
        public (int, int, int) CalculateScoreInformation(MemoryBoxCore[] boxes) {
            var currentQuest = GetCurrentQuestType();
            var score = 0;
            var trueCount = 0;
            var falseCount = 0;

            //先に正答数と誤答数だけ計算しておく
            foreach (var box in boxes) {
                if (box.BoxMemoryType == currentQuest) trueCount++;
                else falseCount++;
            }

            //正答数が多い&誤答数が少ない&納品したMemoryBoxが重いほど高得点
            foreach (var box in boxes) {
                if (box.BoxMemoryType == currentQuest) score += Mathf.FloorToInt((20 + (trueCount - 1)) * box.Weight);
                else
                    score -= Mathf.FloorToInt((10 + (falseCount - 1)) * box.Weight);
            }

            return (score, trueCount, falseCount);
        }

        /// <summary>
        /// UIにフェイズの情報を反映させる
        /// </summary>
        public void UpdatePhaseText() {
            scoreShower.SetScoreText(_phaseCores[_currentPhaseIndex].Score);
            questTypeShower.SetQuestTypeText(GetCurrentQuestType());
        }

        /// <summary>
        /// デバッグ用関数
        /// </summary>
        /// <returns></returns>
        public (PhaseCore[], int, int) GetPhaseInformation() {
            //実際のゲーム画面だとこっちの処理を使う
            // var viewablePhaseCores = _phaseCores.GetRange(_currentPhaseIndex, viewablePhaseCount).ToArray();
            // return (viewablePhaseCores, 0, viewablePhaseCount);

            return (_phaseCores.ToArray(), _currentPhaseIndex, viewablePhaseCount);
        }

        #endregion

        #region private関数

        /// <summary>
        /// 現在のPhaseのQuestTypeを取得する
        /// </summary>
        /// <returns></returns>
        private BoxMemoryType GetCurrentQuestType() {
            return GetQuestType(_currentPhaseIndex);
        }

        /// <summary>
        /// フェイズの初期化
        /// </summary>
        private void InitializePhases() {
            for (var i = 0; i < initialPhaseCount; i++) {
                _phaseCores.Add(GenerateRandomPhase(ScriptableObject.CreateInstance<PhaseCore>()));
            }

            SetBoxGenerationProbability();
            memoryBoxManager.GenerateMemoryBoxes();
        }

        /// <summary>
        /// 引数のPhaseCoreにランダムなQuestTypeを設定する
        /// </summary>
        /// <param name="phaseCore"></param>
        /// <returns>設定されたPhaseCoreが返ってくる</returns>
        private PhaseCore GenerateRandomPhase(PhaseCore phaseCore) {
            var randomPhaseType = (BoxMemoryType)Random.Range(1, (int)BoxMemoryType.Count);
            var randomPhaseGimmick = (PhaseGimmickType)Random.Range(1, (int)PhaseGimmickType.Count);
            phaseCore.QuestType = randomPhaseType;
            phaseCore.GimmickType = randomPhaseGimmick;

            return phaseCore;
        }

        /// <summary>
        /// Phaseの残り時間をリセットする
        /// </summary>
        private void ResetRemainingTime() {
            _phaseRemainingTime = PHASE_DURATION;
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
            SetBoxGenerationProbability();
        }

        /// <summary>
        /// QuestTypeを取得する
        /// </summary>
        /// <param name="phaseIndex">取得するPhaseのインデックス</param>
        /// <returns></returns>
        private BoxMemoryType GetQuestType(int phaseIndex) {
            return _phaseCores[phaseIndex].QuestType;
        }

        /// <summary>
        /// 今あるフェイズを元にBoxの生成確率を設定する
        /// </summary>
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

            memoryBoxManager.SetProbabilityList(nextQuestTypeInts);
        }

        #endregion
    }
}