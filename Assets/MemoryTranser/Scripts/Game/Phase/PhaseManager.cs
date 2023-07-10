using System;
using System.Collections.Generic;
using System.Linq;
using MemoryTranser.Scripts.Game.GameManagers;
using MemoryTranser.Scripts.Game.MemoryBox;
using MemoryTranser.Scripts.Game.UI.Playing;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MemoryTranser.Scripts.Game.Phase {
    public class PhaseManager : MonoBehaviour, IOnGameAwake, IOnStateChangedToInitializing, IOnStateChangedToReady {
        #region コンポーネントの定義

        [SerializeField] private QuestTypeShower questTypeShower;
        [SerializeField] private ScoreShower scoreShower;
        [SerializeField] private PhaseTimerShower phaseTimerShower;
        [SerializeField] private MemoryBoxManager memoryBoxManager;

        #endregion

        #region 変数の定義

        [Header("1つのフェイズの長さ")] public float phaseDuration;

        [Header("Desireを倒したときに加算される点数")] [SerializeField]
        private int additionalScoreOnDefeatDesire;

        [Header("四角いMemoryBoxを納品したときの基礎点数")] [SerializeField]
        private int cubeBoxBasicScore;

        [Header("丸いMemoryBoxを納品したときの基礎点数")] [SerializeField]
        private int sphereBoxBasicScore;

        [Header("最初に生成されるフェイスの数")] [SerializeField]
        private int initialPhaseCount;

        [Header("UIで見ることのできるフェイズの数")] [SerializeField]
        private int viewablePhaseCount;

        [Header("MemoryBoxの発生確率に関わるフェイズの数")] [SerializeField]
        private int boxTypeWeightRange;

        [Header("値が大きいほど直近のフェイズに対応するMemoryBoxが増える")] [SerializeField]
        private float boxTypeProbWeightMultiplier;

        private List<PhaseCore> _phaseCores = new();

        private int _currentPhaseIndex = 0;
        private int _currentTotalScore = 0;
        private float _phaseRemainingTime;

        #endregion

        #region Unityから呼ばれる

        private void Update() {
            phaseTimerShower.SetPhaseRemainingTimeText(_phaseRemainingTime);

            if (GameFlowManager.I.CurrentGameState != GameState.Playing) {
                return;
            }

            //NowGameStateがPlayingの時のみ残り時間を減らす
            _phaseRemainingTime -= Time.deltaTime;

            //フェイズの残り時間が0になったら、次のフェイズに移行する
            if (_phaseRemainingTime <= 0) {
                TransitToNextPhase();
            }
        }

        #endregion


        #region interfaceの実装

        public void OnGameAwake() {
            _phaseRemainingTime = phaseDuration;
        }

        public void OnStateChangedToInitializing() {
            InitializePhases();
        }

        public void OnStateChangedToReady() {
            ResetPhaseRemainingTime();
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
                if (box.BoxMemoryType == currentQuest) {
                    trueCount++;
                }
                else {
                    falseCount++;
                }
            }

            //正答数が多い&誤答数が少ない&納品したMemoryBoxが重いほど高得点
            foreach (var box in boxes) {
                var basicScore = box.BoxShapeType == MemoryBoxShapeType.Cube ? cubeBoxBasicScore : sphereBoxBasicScore;

                if (box.BoxMemoryType == currentQuest) {
                    score += Mathf.FloorToInt((basicScore + (trueCount - 1)) * box.Weight);
                }
                else {
                    score -= Mathf.FloorToInt((basicScore + (falseCount - 1)) * box.Weight);
                }
            }

            return (score, trueCount, falseCount);
        }

        public PhaseCore[] GetNearPhases(int count) {
            return _phaseCores
                .GetRange(_currentPhaseIndex, count)
                .ToArray();
        }

        public (int, int) GetResultInformation() {
            var totalScore = _phaseCores.Sum(phase => phase.Score);
            var reachedPhaseCount = _currentPhaseIndex + 1;

            return (totalScore, reachedPhaseCount);
        }

        /// <summary>
        /// デバッグ用関数
        /// </summary>
        /// <returns></returns>
        public (PhaseCore[], int, int) GetPhaseInformation() {
            //実際のゲーム画面だとこっちの処理を使う
            var viewablePhaseCores = _phaseCores
                .GetRange(_currentPhaseIndex, viewablePhaseCount)
                .ToArray();
            return (viewablePhaseCores, 0, viewablePhaseCount);
        }

        #endregion

        #region private関数

        /// <summary>
        /// フェイズの初期化
        /// </summary>
        private void InitializePhases() {
            for (var i = 0; i < initialPhaseCount; i++) {
                var phaseCore = ScriptableObject.CreateInstance<PhaseCore>();

                var randomPhaseType = (BoxMemoryType)Random.Range(0, (int)BoxMemoryType.Count);
                phaseCore.QuestType = randomPhaseType;

                _phaseCores.Add(phaseCore);
            }

            //MemoryBoxの発生確率に関わるフェイズの数だけ、MemoryBoxの種類の重みを設定する
            SetBoxTypeProbWeights();

            //確率に応じてMemoryBoxの生成を行う
            memoryBoxManager.GenerateMemoryBoxes();

            //クエストの文章の初期化
            questTypeShower.InitializeQuestText(GetCurrentQuestType(), GetNextQuestType());

            //経過したフェイズの数のUIを更新
            phaseTimerShower.SetPassedPhaseCountText(1);
        }

        private void ResetPhaseRemainingTime() {
            _phaseRemainingTime = phaseDuration;
        }

        private int AddScore(int phaseIndex, int score) {
            var phaseCore = _phaseCores[phaseIndex];
            phaseCore.Score += score;
            _currentTotalScore += score;
            UpdateScoreText();
            return phaseCore.Score;
        }

        private int GetScore(int phaseIndex) {
            return _phaseCores[phaseIndex].Score;
        }

        private int GetCurrentScore() {
            return GetScore(_currentPhaseIndex);
        }

        private void UpdateScoreText() {
            scoreShower.SetScoreText(_currentTotalScore);
        }

        private BoxMemoryType GetQuestType(int phaseIndex) {
            return _phaseCores[phaseIndex].QuestType;
        }

        private BoxMemoryType GetCurrentQuestType() {
            return GetQuestType(_currentPhaseIndex);
        }

        private BoxMemoryType GetNextQuestType() {
            return GetQuestType(_currentPhaseIndex + 1);
        }

        private BoxMemoryType GetAfterNextQuestType() {
            return GetQuestType(_currentPhaseIndex + 2);
        }

        /// <summary>
        /// 次のフェイズに移行する
        /// </summary>
        private void TransitToNextPhase() {
            //クエストのUIを更新
            questTypeShower.UpdateQuestText(GetAfterNextQuestType());

            //フェイズの内部のインデックスを足す
            _currentPhaseIndex++;

            //経過したフェイズの数のUIを更新
            phaseTimerShower.SetPassedPhaseCountText(_currentPhaseIndex + 1);

            //フェイズの残り時間をリセット
            _phaseRemainingTime = phaseDuration;

            //次のフェイズの情報を生成して追加
            var phaseCore = ScriptableObject.CreateInstance<PhaseCore>();
            var randomPhaseType = (BoxMemoryType)Random.Range(0, (int)BoxMemoryType.Count);
            phaseCore.QuestType = randomPhaseType;
            _phaseCores.Add(phaseCore);

            //MemoryBoxの生成確率を更新
            SetBoxTypeProbWeights();

            //GameStateをReadyに変更
            GameFlowManager.I.ChangeGameState(GameState.Ready);
        }

        private void SetBoxTypeProbWeights() {
            var boxTypeWeights = new float[(int)BoxMemoryType.Count];
            Array.Fill(boxTypeWeights, 1f);

            for (var i = _currentPhaseIndex; i <= _currentPhaseIndex + boxTypeWeightRange; i++) {
                var boxTypeWeightI = (_currentPhaseIndex + boxTypeWeightRange - (i - 1)) * boxTypeProbWeightMultiplier;
                boxTypeWeights[(int)GetQuestType(i)] += boxTypeWeightI;
            }

            memoryBoxManager.SetBoxTypeProbWeights(boxTypeWeights);
        }

        #endregion
    }
}