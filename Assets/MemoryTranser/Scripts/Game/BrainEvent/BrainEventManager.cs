using System;
using MemoryTranser.Scripts.Game.GameManagers;
using MemoryTranser.Scripts.Game.UI.Playing;
using MemoryTranser.Scripts.Game.UI.Playing.Announce;
using MemoryTranser.Scripts.Game.Util;
using UniRx;
using UnityEngine;
using Random = System.Random;

namespace MemoryTranser.Scripts.Game.BrainEvent {
    public class BrainEventManager : MonoBehaviour, IOnStateChangedToInitializing, IOnStateChangedToResult {
        #region コンポーネントの定義

        [SerializeField] private BrainEventTypeShower brainEventTypeShower;

        #endregion

        [Header("ギミックの抽選の間隔(秒)")] [SerializeField]
        private float selectDurationSec = 10f;

        private AliasMethod _aliasMethod;
        private BrainEventType _currentBrainEventType = BrainEventType.Normal;

        private float _remainingTimeForReSelection;

        #region eventの定義

        private readonly ReactiveProperty<BrainEventType> _onBrainEventTransition = new();

        public IReadOnlyReactiveProperty<BrainEventType> OnBrainEventTransition => _onBrainEventTransition;

        #endregion

        #region Unityから呼ばれる

        private void Update() {
            if (_remainingTimeForReSelection > 0f) {
                _remainingTimeForReSelection -= Time.deltaTime;

                if (_remainingTimeForReSelection < 0f) {
                    _remainingTimeForReSelection = selectDurationSec;
                    _currentBrainEventType = SelectNextBrainEvent();

                    brainEventTypeShower.SetBrainEventTypeText(_currentBrainEventType);
                    _onBrainEventTransition.Value = _currentBrainEventType;
                }
            }
        }

        #endregion

        private BrainEventType SelectNextBrainEvent() {
            var nextBrainEvent = (BrainEventType)UnityEngine.Random.Range(0, (int)BrainEventType.Count);
            return nextBrainEvent;
        }

        #region interfaceの実装

        public void OnStateChangedToInitializing() {
            _remainingTimeForReSelection = 1f;
        }

        public void OnStateChangedToResult() {
            _remainingTimeForReSelection = -1f;
        }

        #endregion
    }
}