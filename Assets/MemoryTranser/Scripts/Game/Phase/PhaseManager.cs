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
        [SerializeField] private QuestTypeShower prefabQuestTypeShower;
        [SerializeField] private Canvas uiCanvas;
        [SerializeField] private PhaseGimmickTypeShower phaseGimmickTypeShower;
        [SerializeField] private ScoreShower scoreShower;
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
        
        private int phaseCount = 0;

        [Header("QuestTypeShowerの数")] [SerializeField]
        private int numQtShower;

        //author コメダ 科目の文章設定
        //QuestTypeShowerのリスト
        private List<QuestTypeShower> qtShowerList = new();

        //author コメダ 科目の文章設定
        //QuestTwxtの文章を設定
        [SerializeField] private List<QuestText> mathTxts;
        [SerializeField] private List<QuestText> jpnTxts;
        [SerializeField] private List<QuestText> engTxts;
        [SerializeField] private List<QuestText> lifeTxts;
        [SerializeField] private List<QuestText> moralTxts;
        [SerializeField] private List<QuestText> scienceTxts;
        [SerializeField] private List<QuestText> triviaTxts;
        [SerializeField] private List<QuestText> musicTxts;
        [SerializeField] private List<QuestText> socialTxts;
        [SerializeField] private List<QuestText> hobbyTxts;

        //author コメダ 科目の色設定
        //QuestTextの色を設定
        [SerializeField] private string mathColor;
        [SerializeField] private string jpnColor;
        [SerializeField] private string engColor;
        [SerializeField] private string lifeColor;
        [SerializeField] private string moralColor;
        [SerializeField] private string scienceColor;
        [SerializeField] private string triviaColor;
        [SerializeField] private string musicColor;
        [SerializeField] private string socialColor;
        [SerializeField] private string hobbyColor;

        #endregion

        #region プロパティーの定義

        public float RemainingTime => _phaseRemainingTime;

        #endregion

        #region Unityから呼ばれる

        private void Update() {
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
            CreateQuestTypeShower();
        }

        public void OnStateChangedToReady() {
            ResetPhaseRemainingTime();
            CreateQuestTypeShower();
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
            var nextQuest = GetNextQuestType();
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

            SetBoxTypeProbWeights();

            memoryBoxManager.GenerateMemoryBoxes();
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

        private void UpdateQuestTypeText() {
            questTypeShower.SetQuestTypeText(GetCurrentQuestType());
        }
        
        /// <summary>
        /// インデックスを安全に増やす
        /// </summary>
        private int ValidAddIndex(int _index, int _maxIndex)
        {
            if (_index + 1 >= _maxIndex)
            {
                return _index;
            }

            return _index + 1;
        }

        private int ValidSubIndex(int _index, int _maxIndex)
        {
            if (_index - 1 < 0)
            {
                return _index;
            }

            return _index - 1;
        }

        /// <summary>
        /// 次のフェイズに移行する
        /// </summary>
        private void TransitToNextPhase() {
            ChangeQuestTypeShower();
            
            //フェイズの内部のインデックスを足す
            _currentPhaseIndex++;

            //フェイズの残り時間をリセット
            _phaseRemainingTime = phaseDuration;

            //次のフェイズの情報を生成して追加
            var phaseCore = ScriptableObject.CreateInstance<PhaseCore>();
            var randomPhaseType = (BoxMemoryType)Random.Range(0, (int)BoxMemoryType.Count);
            phaseCore.QuestType = randomPhaseType;
            _phaseCores.Add(phaseCore);

            //MemoryBoxの生成確率を更新
            SetBoxTypeProbWeights();

            //クエストのUIを更新
            UpdateQuestTypeText();

            //GameStateをReadyに変更
            GameFlowManager.I.ChangeGameState(GameState.Ready);
        }

        private BoxMemoryType GetQuestType(int phaseIndex) {
            return _phaseCores[phaseIndex].QuestType;
        }

        private BoxMemoryType GetCurrentQuestType() {
            return GetQuestType(_currentPhaseIndex);
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
        
        //次のクエストを返す
        private BoxMemoryType GetNextQuestType()
        {
            return GetQuestType(ValidAddIndex(_currentPhaseIndex, initialPhaseCount));
        }

        private BoxMemoryType GetOldQuestType()
        {
            return GetQuestType(ValidSubIndex(_currentPhaseIndex, initialPhaseCount));
        }

        //author:コメダ QuestTypeShowerのプレハブ作成
        private void CreateQuestTypeShower()
        {
            for (var i = 0; i < numQtShower; i++)
            {
                var qtShower = Instantiate(prefabQuestTypeShower);
                qtShower.transform.position = i == 0 ? new Vector3(0, 0, 0) :
                    i == 1 ? new Vector3(0, 200, 0) : new Vector3(0, -200, 0);
                qtShower.transform.SetParent(uiCanvas.transform, false);
                qtShowerList.Add(qtShower);
            }
        }

        private void ChangeQuestTypeShower()
        {
            //for (var i = 0; i < numQtShower; i++) qtShowerList[i].SetQuestTypeText(GetCurrentQuestType());
            qtShowerList[phaseCount % 3].ChangeToNext(ChooseQuestTxt(GetNextQuestType()));
            qtShowerList[(phaseCount + 1) % 3].ChangeToNow(ChooseQuestTxt(GetCurrentQuestType()));
            qtShowerList[(phaseCount + 2) % 3].ChangeToOld(ChooseQuestTxt(GetOldQuestType()));
            phaseCount++;
        }

        private string ChooseQuestTxt(BoxMemoryType boxMemoryType)
        {
            QuestText questTextChoosed = null;
            string questTextChoosedPainted = null;
            switch (boxMemoryType)
            {
                case BoxMemoryType.Math:
                    questTextChoosed = mathTxts[Random.Range(0, mathTxts.Count - 1)];
                    questTextChoosedPainted =
                        $"{questTextChoosed.startTxt}<color=#{mathColor}>{questTextChoosed.mainTxt}</color>{questTextChoosed.finalTxt}";
                    break;
                case BoxMemoryType.Japanese:
                    questTextChoosed = jpnTxts[Random.Range(0, jpnTxts.Count - 1)];
                    questTextChoosedPainted =
                        $"{questTextChoosed.startTxt}<color=#{jpnColor}>{questTextChoosed.mainTxt}</color>{questTextChoosed.finalTxt}";
                    break;
                case BoxMemoryType.English:
                    questTextChoosed = engTxts[Random.Range(0, engTxts.Count - 1)];
                    questTextChoosedPainted =
                        $"{questTextChoosed.startTxt}<color=#{engColor}>{questTextChoosed.mainTxt}</color>{questTextChoosed.finalTxt}";
                    break;
                case BoxMemoryType.SocialStudies:
                    questTextChoosed = socialTxts[Random.Range(0, socialTxts.Count - 1)];
                    questTextChoosedPainted =
                        $"{questTextChoosed.startTxt}<color=#{socialColor}>{questTextChoosed.mainTxt}</color>{questTextChoosed.finalTxt}";
                    break;
                case BoxMemoryType.Science:
                    questTextChoosed = scienceTxts[Random.Range(0, scienceTxts.Count - 1)];
                    questTextChoosedPainted =
                        $"{questTextChoosed.startTxt}<color=#{scienceColor}>{questTextChoosed.mainTxt}</color>{questTextChoosed.finalTxt}";
                    break;
                case BoxMemoryType.Trivia:
                    questTextChoosed = triviaTxts[Random.Range(0, triviaTxts.Count - 1)];
                    questTextChoosedPainted =
                        $"{questTextChoosed.startTxt}<color=#{triviaColor}>{questTextChoosed.mainTxt}</color>{questTextChoosed.finalTxt}";
                    break;
                case BoxMemoryType.Moral:
                    questTextChoosed = moralTxts[Random.Range(0, moralTxts.Count - 1)];
                    questTextChoosedPainted =
                        $"{questTextChoosed.startTxt}<color=#{moralColor}>{questTextChoosed.mainTxt}</color>{questTextChoosed.finalTxt}";
                    break;
                case BoxMemoryType.Music:
                    questTextChoosed = musicTxts[Random.Range(0, musicTxts.Count - 1)];
                    questTextChoosedPainted =
                        $"{questTextChoosed.startTxt}<color=#{musicColor}>{questTextChoosed.mainTxt}</color>{questTextChoosed.finalTxt}";
                    break;
                case BoxMemoryType.Habit:
                    questTextChoosed = hobbyTxts[Random.Range(0, hobbyTxts.Count - 1)];
                    questTextChoosedPainted =
                        $"{questTextChoosed.startTxt}<color=#{hobbyColor}>{questTextChoosed.mainTxt}</color>{questTextChoosed.finalTxt}";
                    break;
                case BoxMemoryType.Life:
                    questTextChoosed = lifeTxts[Random.Range(0, lifeTxts.Count - 1)];
                    questTextChoosedPainted =
                        $"{questTextChoosed.startTxt}<color=#{lifeColor}>{questTextChoosed.mainTxt}</color>{questTextChoosed.finalTxt}";
                    break;
            }

            return questTextChoosedPainted;
        }

        #endregion
    }
}