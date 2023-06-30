using System;
using MemoryTranser.Scripts.Game.GameManagers;
using MemoryTranser.Scripts.Game.Util;
using UniRx;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.BrainEvent {
    public class BrainEventManager : MonoBehaviour, IOnStateChangedToResult {
        private AliasMethod _aliasMethod;

        #region eventの定義

        private readonly ReactiveProperty<BrainEventType> _onBrainEventTransition = new();

        public IReadOnlyReactiveProperty<BrainEventType> OnBrainEventTransition => _onBrainEventTransition;

        #endregion

        #region Unityから呼ばれる

        private void Awake() { }

        #endregion

        #region interfaceの実装

        public void OnStateChangedToResult() { }

        #endregion
    }
}