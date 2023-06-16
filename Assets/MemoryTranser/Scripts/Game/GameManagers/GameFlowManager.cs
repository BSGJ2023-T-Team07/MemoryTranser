using System;
using Cysharp.Threading.Tasks;
using MemoryTranser.Scripts.Game.Concentration;
using MemoryTranser.Scripts.Game.Desire;
using MemoryTranser.Scripts.Game.Fairy;
using MemoryTranser.Scripts.Game.MemoryBox;
using MemoryTranser.Scripts.Game.Phase;
using MemoryTranser.Scripts.Game.Sound;
using MemoryTranser.Scripts.Game.Util;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace MemoryTranser.Scripts.Game.GameManagers {
    [RequireComponent(typeof(PhaseManager))]
    [RequireComponent(typeof(ConcentrationManager))]
    [RequireComponent(typeof(MemoryBoxManager))]
    [RequireComponent(typeof(DesireManager))]
    public class GameFlowManager : SingletonMonoBehaviour<GameFlowManager> {
        protected override bool DontDestroy => false;

        #region コンポーネントの定義

        [SerializeField] private PhaseManager phaseManager;
        [SerializeField] private ConcentrationManager concentrationManager;
        [SerializeField] private MemoryBoxManager memoryBoxManager;
        [SerializeField] private DesireManager desireManager;

        [SerializeField] private FairyCore fairyCore;
        private BgmManager _bgmManager;

        #endregion

        //ゲームシーンがデザイアコアにアクセスできるようにする
        [SerializeField] private DesireCore desireCore;

        #region Unityから呼ばれる

        protected override void Awake() {
            base.Awake();
            phaseManager = GetComponent<PhaseManager>();
            concentrationManager = GetComponent<ConcentrationManager>();
            memoryBoxManager = GetComponent<MemoryBoxManager>();
            desireManager = GetComponent<DesireManager>();
            _bgmManager = FindObjectOfType<BgmManager>();
        }

        private void Start() {
            ChangeGameState(GameState.Initializing);
        }

        #endregion

        private GameState _gameState = GameState.Initializing;
        public GameState NowGameState => _gameState;


        public void ChangeGameState(GameState state) {
            _gameState = state;
            Debug.Log($"Stateが{state.ToString()}に変わりました");

            switch (state) {
                case GameState.Initializing:
                    OnStateInitializing();
                    break;
                case GameState.Ready:
                    OnStateReady();
                    break;
                case GameState.Playing:
                    OnStatePlaying();
                    break;
                case GameState.Result:
                    OnStateResult();
                    break;
                case GameState.Finished:
                    OnStateFinished();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private async void OnStateInitializing() {
            _bgmManager?.PlayIntro();
            memoryBoxManager.InitializeMemoryBoxes();
            phaseManager.InitializePhases();
            fairyCore.InitializeFairy();
            desireCore.InitializeDesire();
            concentrationManager.InitializeConcentration();

            await UniTask.Delay(TimeSpan.FromTicks(1));

            ChangeGameState(GameState.Ready);
        }

        private async void OnStateReady() {
            concentrationManager.DecreaseFlag = false;
            desireManager.Spawnflag = false;
            phaseManager.UpdatePhaseText();
            phaseManager.ResetRemainingTime();

            await UniTask.Delay(TimeSpan.FromSeconds(1f));

            ChangeGameState(GameState.Playing);
        }

        private void OnStatePlaying() {
            concentrationManager.DecreaseFlag = true;
            fairyCore.IsControllable = true;
            desireCore.RestartFollowing();
            desireManager.Spawnflag = true;
        }

        private void OnStateResult() {
            concentrationManager.DecreaseFlag = false;
            fairyCore.IsControllable = false;
            desireManager.Spawnflag = false;
        }

        private void OnStateFinished() { }
    }
}