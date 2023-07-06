using System.Collections.Generic;
using MemoryTranser.Scripts.Game.GameManagers;
using MemoryTranser.Scripts.Game.UI.Playing.Announce;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MemoryTranser.Scripts.Game.BrainEvent {
    public class BrainEventManager : MonoBehaviour, IOnStateChangedToInitializing, IOnStateChangedToResult {
        [Header("ギミックの抽選の間隔(秒)")] [SerializeField]
        private float selectDurationSec = 10f;

        private List<BrainEventType> _brainEvents = new();

        private float _remainingTimeForReSelection;
        private int _currentBrainEventIndex = 0;

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
                    TransitToNextBrainEvent();
                }
            }
        }

        #endregion

        private void TransitToNextBrainEvent() {
            var nextBrainEvent = SelectNextBrainEvent();
            _brainEvents.Add(nextBrainEvent);
            _onBrainEventTransition.Value = nextBrainEvent;
            BrainEventTypeShower.SetBrainEventTypeText(nextBrainEvent);
            _currentBrainEventIndex++;
        }

        private BrainEventType SelectNextBrainEvent() {
            BrainEventType nextBrainEvent;
            //1つ前にギミックが発生していたら、何も起きない
            if (GetCurrentBrainEvent() == BrainEventType.None) {
                nextBrainEvent = (BrainEventType)Random.Range(0, (int)BrainEventType.Count);
            }
            else {
                nextBrainEvent = BrainEventType.None;
            }

            return nextBrainEvent;
        }

        private BrainEventType GetCurrentBrainEvent() {
            return GetBrainEvent(_currentBrainEventIndex);
        }

        private BrainEventType GetBrainEvent(int index) {
            return _brainEvents[index];
        }

        #region interfaceの実装

        public void OnStateChangedToInitializing() {
            _remainingTimeForReSelection = selectDurationSec;

            const BrainEventType initialBrainEvent = BrainEventType.None;
            _onBrainEventTransition.Value = initialBrainEvent;
            _brainEvents.Add(initialBrainEvent);
            _currentBrainEventIndex = 0;
            BrainEventTypeShower.SetBrainEventTypeText(initialBrainEvent);
        }

        public void OnStateChangedToResult() {
            _remainingTimeForReSelection = -1f;
            _onBrainEventTransition.Dispose();
        }

        #endregion
    }
}