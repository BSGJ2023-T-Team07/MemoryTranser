using System.Collections.Generic;
using MemoryTranser.Scripts.Game.GameManagers;
using MemoryTranser.Scripts.Game.UI.Playing.Announce;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MemoryTranser.Scripts.Game.BrainEvent {
    public class BrainEventManager : MonoBehaviour, IOnStateChangedToInitializing, IOnStateChangedToResult {
        [Header("ギミックの抽選と継続の間隔(秒)")] [SerializeField]
        private float selectDurationSec;

        [Header("勉強の成果イベントのギミック継続時間")] [SerializeField]
        private float achievementOfStudyDurationSec;

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
                    TransitToNextBrainEvent(out var nextBrainEvent);

                    //イベントによって継続時間を変える
                    _remainingTimeForReSelection = nextBrainEvent switch {
                        BrainEventType.AchievementOfStudy => achievementOfStudyDurationSec,
                        _ => selectDurationSec
                    };
                }
            }
        }

        #endregion

        private void TransitToNextBrainEvent(out BrainEventType nextBrainEvent) {
            var thisNextBrainEvent = SelectNextBrainEvent();
            _brainEvents.Add(thisNextBrainEvent);
            _onBrainEventTransition.Value = thisNextBrainEvent;
            BrainEventTypeShower.SetBrainEventTypeText(thisNextBrainEvent);
            _currentBrainEventIndex++;

            nextBrainEvent = thisNextBrainEvent;
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