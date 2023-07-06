using System.Collections.Generic;
using MemoryTranser.Scripts.Game.GameManagers;
using MemoryTranser.Scripts.Game.MemoryBox;
using MemoryTranser.Scripts.Game.UI.Playing;
using UniRx;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.Phase
{
    public class PhaseManager : MonoBehaviour, IOnStateChangedToInitializing, IOnStateChangedToReady
    {
        #region コンポーネントの定義

        [SerializeField] private QuestTypeShower prefabQuestTypeShower;
        [SerializeField] private Canvas uiCanvas;
        [SerializeField] private PhaseGimmickTypeShower phaseGimmickTypeShower;
        [SerializeField] private ScoreShower scoreShower;
        [SerializeField] private MemoryBoxManager memoryBoxManager;

        #endregion

        #region 変数の定義

        [Header("1つのフェイズの長さ")] public float phaseDuration = 30f;

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

        private int phaseCount = 0;

        #endregion

        #region eventの定義

        private readonly ReactiveProperty<PhaseGimmickType> _onPhaseTransition = new(PhaseGimmickType.Normal);
        public IReadOnlyReactiveProperty<PhaseGimmickType> OnPhaseTransition => _onPhaseTransition;

        #endregion

        #region Unityから呼ばれる

        private void Awake()
        {
            _phaseRemainingTime = phaseDuration;
        }

        private void Update()
        {
            if (GameFlowManager.I.NowGameState != GameState.Playing)
            {
                return;
            }

            //NowGameStateがPlayingの時のみ残り時間を減らす
            _phaseRemainingTime -= Time.deltaTime;

            //フェイズの残り時間が0になったら、次のフェイズに移行する
            if (_phaseRemainingTime <= 0)
            {
                TransitToNextPhase();
                _phaseRemainingTime = phaseDuration;
                GameFlowManager.I.ChangeGameState(GameState.Ready);
            }
        }

        #endregion


        #region interfaceの実装

        public void OnStateChangedToInitializing()
        {
            InitializePhases();
            CreateQuestTypeShower();
            _onPhaseTransition.Value = GetCurrentPhaseGimmickType();
        }

        public void OnStateChangedToReady()
        {
            ChangeQuestTypeShower();
            UpdatePhaseText();
            ResetRemainingTime();
        }

        #endregion

        #region public関数

        /// <summary>
        /// 現在のフェイズの点数を加算する
        /// </summary>
        /// <param name="score"></param>
        public void AddCurrentScore(int score)
        {
            AddScore(_currentPhaseIndex, score);
        }

        /// <summary>
        /// Desireを倒したときに点数を加算する
        /// </summary>
        public void AddCurrentScoreOnDefeatDesire()
        {
            AddScore(_currentPhaseIndex, additionalScoreOnDefeatDesire);
        }

        /// <summary>
        /// 納品されたMemoryBoxの情報をもとにスコアを計算する
        /// </summary>
        /// <param name="boxes"></param>
        /// <returns>点数、正答数、誤答数の組を返す</returns>
        public (int, int, int) CalculateScoreInformation(MemoryBoxCore[] boxes)
        {
            var currentQuest = GetCurrentQuestType();
            var nextQuest = GetNextQuestType();
            var score = 0;
            var trueCount = 0;
            var falseCount = 0;

            //先に正答数と誤答数だけ計算しておく
            foreach (var box in boxes)
                if (box.BoxMemoryType == currentQuest)
                {
                    trueCount++;
                }
                else
                {
                    falseCount++;
                }

            //正答数が多い&誤答数が少ない&納品したMemoryBoxが重いほど高得点
            foreach (var box in boxes)
                if (box.BoxMemoryType == currentQuest)
                {
                    if (box.BoxShapeType == MemoryBoxShapeType.Cube)
                    {
                        score += Mathf.FloorToInt((20 + (trueCount - 1)) * box.Weight);
                    }
                    else
                    {
                        score += Mathf.FloorToInt((20 + (trueCount - 1)) * box.Weight * 2);
                    }
                }
                else
                {
                    if (box.BoxShapeType == MemoryBoxShapeType.Cube)
                    {
                        score -= Mathf.FloorToInt((10 + (falseCount - 1)) * box.Weight);
                    }
                    else
                    {
                        score -= Mathf.FloorToInt((10 + (falseCount - 1)) * box.Weight * 2);
                    }
                }

            return (score, trueCount, falseCount);
        }

        /// <summary>
        /// UIにフェイズの情報を反映させる
        /// </summary>
        public void UpdatePhaseText()
        {
            scoreShower.SetScoreText(GetCurrentScore());
            phaseGimmickTypeShower.SetPhaseGimmickTypeText(GetCurrentPhaseGimmickType());
        }

        /// <summary>
        /// デバッグ用関数
        /// </summary>
        /// <returns></returns>
        public (PhaseCore[], int, int) GetPhaseInformation()
        {
            //実際のゲーム画面だとこっちの処理を使う
            // var viewablePhaseCores = _phaseCores.GetRange(_currentPhaseIndex, viewablePhaseCount).ToArray();
            // return (viewablePhaseCores, 0, viewablePhaseCount);

            return (_phaseCores.ToArray(), _currentPhaseIndex, viewablePhaseCount);
        }

        #endregion

        #region private関数

        /// <summary>
        /// フェイズの初期化
        /// </summary>
        private void InitializePhases()
        {
            for (var i = 0; i < initialPhaseCount; i++)
            {
                var phaseCore = ScriptableObject.CreateInstance<PhaseCore>();
                var randomPhaseType = (BoxMemoryType)Random.Range(1, (int)BoxMemoryType.Count);
                var randomPhaseGimmick = (PhaseGimmickType)Random.Range(0, (int)PhaseGimmickType.Count);
                phaseCore.QuestType = randomPhaseType;
                phaseCore.GimmickType = randomPhaseGimmick;

                _phaseCores.Add(phaseCore);
            }

            SetBoxGenerationProbability();
            memoryBoxManager.GenerateMemoryBoxes();
        }

        private void ResetRemainingTime()
        {
            _phaseRemainingTime = phaseDuration;
        }

        private int AddScore(int phaseIndex, int score)
        {
            var phaseCore = _phaseCores[phaseIndex];
            phaseCore.Score += score;
            return phaseCore.Score;
        }

        private int GetScore(int phaseIndex)
        {
            return _phaseCores[phaseIndex].Score;
        }

        private int GetCurrentScore()
        {
            return GetScore(_currentPhaseIndex);
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
        private void TransitToNextPhase()
        {
            _currentPhaseIndex = ValidAddIndex(_currentPhaseIndex, initialPhaseCount);
            ChangeQuestTypeShower();
            SetBoxGenerationProbability();
            _onPhaseTransition.Value = GetCurrentPhaseGimmickType();
        }

        private BoxMemoryType GetQuestType(int phaseIndex)
        {
            Debug.Log($"{phaseIndex}");
            return _phaseCores[phaseIndex].QuestType;
        }

        private BoxMemoryType GetCurrentQuestType()
        {
            return GetQuestType(_currentPhaseIndex);
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

        private PhaseGimmickType GetPhaseGimmickType(int phaseIndex)
        {
            return _phaseCores[phaseIndex].GimmickType;
        }

        private PhaseGimmickType GetCurrentPhaseGimmickType()
        {
            return GetPhaseGimmickType(_currentPhaseIndex);
        }

        /// <summary>
        /// 今あるフェイズを元にBoxの生成確率を設定する
        /// </summary>
        private void SetBoxGenerationProbability()
        {
            var nextQuestTypeInts = new List<int>();
            for (var i = _currentPhaseIndex; i <= _currentPhaseIndex + memoryBoxProbabilityPhaseCount; i++)
            {
                var questTypeInt = (int)GetQuestType(i);
                var questTypeInts = new List<int>();

                //直近のフェイズになるほど対応するMemoryBoxが出やすくなる
                for (var j = 0; j < memoryBoxProbabilityWeight * 2 - (i - _currentPhaseIndex); j++)
                    questTypeInts.Add(questTypeInt);

                nextQuestTypeInts.AddRange(questTypeInts);
            }

            memoryBoxManager.SetProbabilityList(nextQuestTypeInts);
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