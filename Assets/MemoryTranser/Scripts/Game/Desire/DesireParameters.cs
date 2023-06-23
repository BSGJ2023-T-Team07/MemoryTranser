using UnityEngine;
using UnityEngine.Serialization;

namespace MemoryTranser.Scripts.Game.Desire {
    [System.Serializable]
    public class DesireParameters {
        private float _followSpeed;

        [SerializeField] private float maxFollowSpeed = 10f;
        [SerializeField] private float minFollowSpeed = 0f;
        [SerializeField] private float initialFollowSpeed = 3.5f;

        public float FollowSpeed => Mathf.Clamp(_followSpeed, minFollowSpeed, maxFollowSpeed);

        public void InitializeParameters() {
            _followSpeed = initialFollowSpeed;
        }
    }
}