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
    public class GameFlowManager : SingletonMonoBehaviour<GameFlowManager> {
        protected override bool DontDestroy => false;

        #region interfaceのインスタンス配列の定義

        private IOnStateChangedToInitializing[] _onStateChangedToInitializings;
        private IOnStateChangedToReady[] _onStateChangedToReadys;
        private IOnStateChangedToPlaying[] _onStateChangedToPlayings;
        private IOnStateChangedToResult[] _onStateChangedToResults;
        private IOnStateChangedToFinished[] _onStateChangedToFinisheds;

        #endregion

        #region Unityから呼ばれる

        protected override void Awake() {
            base.Awake();
            _onStateChangedToInitializings =
                GameObjectExtensions.FindObjectsByInterface<IOnStateChangedToInitializing>();
            _onStateChangedToReadys =
                GameObjectExtensions.FindObjectsByInterface<IOnStateChangedToReady>();
            _onStateChangedToPlayings =
                GameObjectExtensions.FindObjectsByInterface<IOnStateChangedToPlaying>();
            _onStateChangedToResults =
                GameObjectExtensions.FindObjectsByInterface<IOnStateChangedToResult>();
            _onStateChangedToFinisheds =
                GameObjectExtensions.FindObjectsByInterface<IOnStateChangedToFinished>();
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
            foreach (var onStateChangedToInitializing in _onStateChangedToInitializings) {
                onStateChangedToInitializing.OnStateChangedToInitializing();
            }

            await UniTask.Delay(TimeSpan.FromTicks(1));

            ChangeGameState(GameState.Ready);
        }

        private async void OnStateReady() {
            foreach (var onStateChangedToReady in _onStateChangedToReadys) {
                onStateChangedToReady.OnStateChangedToReady();
            }

            await UniTask.Delay(TimeSpan.FromSeconds(1f));

            ChangeGameState(GameState.Playing);
        }

        private void OnStatePlaying() {
            foreach (var onStateChangedToPlaying in _onStateChangedToPlayings) {
                onStateChangedToPlaying.OnStateChangedToPlaying();
            }
        }

        private void OnStateResult() {
            foreach (var onStateChangedToResult in _onStateChangedToResults) {
                onStateChangedToResult.OnStateChangedToResult();
            }
        }

        private void OnStateFinished() {
            foreach (var onStateChangedToFinished in _onStateChangedToFinisheds) {
                onStateChangedToFinished.OnStateChangedToFinished();
            }
        }
    }
}