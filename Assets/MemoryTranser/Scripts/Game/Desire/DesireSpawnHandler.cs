using System;
using System.Collections.Generic;
using MemoryTranser.Scripts.Game.GameManagers;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.Desire {
    public class DesireSpawnHandler : MonoBehaviour, IOnStateChangedToInitializing, IOnStateChangedToReady,
        IOnStateChangedToPlaying, IOnStateChangedToResult {
        [SerializeField] private GameObject desirePrefab;

        [SerializeField] private bool canSpawn;
        [SerializeField] private float spawnInterval;
        [SerializeField] private int maxSpawnCount;
        [SerializeField] private Transform[] spawnPoints;


        private List<DesireCore> _desireCores = new();

        #region Unityから呼ばれる

        private void Awake() {
            for (var i = 0; i < maxSpawnCount; i++) {
                var desire = Instantiate(desirePrefab, transform.position, Quaternion.identity);
                desire.SetActive(false);
            }
        }

        #endregion

        private void SpawnDesire() { }

        #region interfaceの実装

        public void OnStateChangedToInitializing() { }

        public void OnStateChangedToReady() {
            canSpawn = false;
        }

        public void OnStateChangedToPlaying() {
            canSpawn = true;
        }

        public void OnStateChangedToResult() {
            canSpawn = false;
        }

        #endregion
    }
}