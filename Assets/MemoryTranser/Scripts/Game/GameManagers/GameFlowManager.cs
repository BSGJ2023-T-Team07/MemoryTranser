using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MemoryTranser.Scripts.Game.Util;
using TMPro;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace MemoryTranser.Scripts.Game.GameManagers {
    public class GameFlowManager : SingletonMonoBehaviour<GameFlowManager> {
        protected override bool DontDestroy => false;

        [SerializeField] private GameFlowShower gameFlowShower;

        #region interfaceのインスタンス配列の定義

        private IOnGameAwake[] _onGameAwakes;
        private IOnGameStart[] _onGameStarts;
        private IOnStateChangedToInitializing[] _onStateChangedToInitializings;
        private IOnStateChangedToReady[] _onStateChangedToReadys;
        private IOnStateChangedToPlaying[] _onStateChangedToPlayings;
        private IOnStateChangedToResult[] _onStateChangedToResults;
        private IOnStateChangedToFinished[] _onStateChangedToFinisheds;

        #endregion

        #region Unityから呼ばれる

        protected override void Awake() {
            base.Awake();
            _onGameAwakes = GameObjectExtensions.FindObjectsByInterface<IOnGameAwake>();
            _onGameStarts = GameObjectExtensions.FindObjectsByInterface<IOnGameStart>();
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
            gameFlowShower.CountdownSequence.Play();
        }

        #endregion

        private GameState _gameState = GameState.Initializing;
        public GameState CurrentGameState => _gameState;


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

        private void OnStateInitializing() {
            foreach (var onGameAwake in _onGameAwakes) {
                onGameAwake.OnGameAwake();
            }

            foreach (var onGameStart in _onGameStarts) {
                onGameStart.OnGameStart();
            }

            foreach (var onStateChangedToInitializing in _onStateChangedToInitializings) {
                onStateChangedToInitializing.OnStateChangedToInitializing();
            }

            ChangeGameState(GameState.Ready);
        }

        private void OnStateReady() {
            foreach (var onStateChangedToReady in _onStateChangedToReadys) {
                onStateChangedToReady.OnStateChangedToReady();
            }

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