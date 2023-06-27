using System;
using MemoryTranser.Scripts.Game.GameManagers;
using UniRx;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.BrainEvent {
    public class BrainEventManager : MonoBehaviour, IOnStateChangedToResult {
        private readonly ReactiveProperty<BrainEventType> _brainEventType = new();

        public IReadOnlyReactiveProperty<BrainEventType> BrainEventType => _brainEventType;

        #region Unityから呼ばれる

        private void Awake() { }

        #endregion

        #region interfaceの実装

        public void OnStateChangedToResult() { }

        #endregion
    }
}