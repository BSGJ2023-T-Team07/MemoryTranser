using System;
using System.Collections.Generic;
using MemoryTranser.Scripts.Game.GameManagers;
using MemoryTranser.Scripts.Game.UI.Playing;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MemoryTranser.Scripts.Game.BrainEvent {
    public class BrainEventManager : MonoBehaviour, IOnStateChangedToInitializing, IOnStateChangedToResult {
        [SerializeField] private BrainEventTypeShower brainEventTypeShower;

        [Header("イベント開始の何秒前に告知を出すか")] [SerializeField]
        private float eventAnnounceSec;

        [Header("イベント無しの継続時間")] [SerializeField]
        private float noEventDurationSec;

        [Header("ド忘れイベントのギミック継続時間")] [SerializeField]
        private float blindDurationSec;

        [Header("煩悩大量発生イベントのギミック継続時間")] [SerializeField]
        private float desireOutbreakDurationSec;

        [Header("操作逆転イベントのギミック継続時間")] [SerializeField]
        private float invertControlDurationSec;

        [Header("勉強の成果イベントのギミック継続時間")] [SerializeField]
        private float achievementOfStudyDurationSec;

        [Header("フィーバータイムのギミック継続時間")] [SerializeField]
        private float feverTimeDurationSec;

        private List<BrainEventType> _brainEvents = new();

        private float _remainingTimeForReSelection;
        private float _remainingTimeForEventAnnounce;
        private int _currentBrainEventIndex = 0;
        private BrainEventType _nextBrainEvent;

        #region eventの定義

        private readonly ReactiveProperty<BrainEventType> _onBrainEventTransition = new();

        public IReadOnlyReactiveProperty<BrainEventType> OnBrainEventTransition => _onBrainEventTransition;

        private readonly ReactiveProperty<BrainEventType> _onBeforeStartEvent = new();
        public IReadOnlyReactiveProperty<BrainEventType> OnBeforeStartEvent => _onBeforeStartEvent;

        #endregion

        #region Unityから呼ばれる

        private void Update() {
            if (_remainingTimeForReSelection > 0f) {
                _remainingTimeForReSelection -= Time.deltaTime;

                if (_remainingTimeForReSelection < 0f) {
                    TransitToNextBrainEvent(_nextBrainEvent);

                    //イベントによって継続時間を変える
                    _remainingTimeForReSelection = GetSecForReselection(_nextBrainEvent);

                    //告知の準備をする
                    _remainingTimeForEventAnnounce = _remainingTimeForReSelection - eventAnnounceSec;

                    //次のイベントを生成しておく
                    _nextBrainEvent = SelectNextBrainEvent();
                }
            }

            if (_remainingTimeForEventAnnounce > 0f) {
                _remainingTimeForEventAnnounce -= Time.deltaTime;

                if (_remainingTimeForEventAnnounce < 0f) {
                    _onBeforeStartEvent.Value = _nextBrainEvent;
                }
            }
        }

        #endregion

        private void TransitToNextBrainEvent(BrainEventType nextBrainEvent) {
            _brainEvents.Add(nextBrainEvent);
            _onBrainEventTransition.Value = nextBrainEvent;
            brainEventTypeShower.SetBrainEventShow(nextBrainEvent, GetSecForReselection(nextBrainEvent));
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

        private float GetSecForReselection(BrainEventType brainEventType) {
            return brainEventType switch {
                BrainEventType.None => noEventDurationSec,
                BrainEventType.AchievementOfStudy => achievementOfStudyDurationSec,
                BrainEventType.Blind => blindDurationSec,
                BrainEventType.DesireOutbreak => desireOutbreakDurationSec,
                BrainEventType.FeverTime => feverTimeDurationSec,
                BrainEventType.InvertControl => invertControlDurationSec,
                _ => throw new ArgumentOutOfRangeException(nameof(brainEventType), brainEventType, null)
            };
        }

        private BrainEventType GetCurrentBrainEvent() {
            return GetBrainEvent(_currentBrainEventIndex);
        }

        private BrainEventType GetBrainEvent(int index) {
            return _brainEvents[index];
        }

        #region interfaceの実装

        public void OnStateChangedToInitializing() {
            var initialBrainEvent = BrainEventType.None;
            _onBrainEventTransition.Value = initialBrainEvent;
            _brainEvents.Add(initialBrainEvent);
            _currentBrainEventIndex = 0;
            brainEventTypeShower.SetBrainEventShow(initialBrainEvent, GetSecForReselection(initialBrainEvent));

            //次のイベントを生成しておく
            _remainingTimeForReSelection = GetSecForReselection(initialBrainEvent);
            _remainingTimeForEventAnnounce = GetSecForReselection(initialBrainEvent) - eventAnnounceSec;
            _nextBrainEvent = SelectNextBrainEvent();
        }

        public void OnStateChangedToResult() {
            _remainingTimeForReSelection = -1f;
            _remainingTimeForEventAnnounce = -1f;
            _onBrainEventTransition.Dispose();
            _onBeforeStartEvent.Dispose();
        }

        #endregion
    }
}