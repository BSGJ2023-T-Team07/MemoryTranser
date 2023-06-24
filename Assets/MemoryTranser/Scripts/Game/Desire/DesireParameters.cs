using UnityEngine;
using UnityEngine.Serialization;

namespace MemoryTranser.Scripts.Game.Desire {
    [System.Serializable]
    public class DesireParameters {
        private float _followSpeed;
        private DesireType _desireType;

        [SerializeField] private float maxGameFollowSpeed = 10f;
        [SerializeField] private float minGameFollowSpeed = 0.1f;
        [SerializeField] private float initialGameFollowSpeed = 3.5f;

        [SerializeField] private float maxComicFollowSpeed = 10f;
        [SerializeField] private float minComicFollowSpeed = 0.1f;
        [SerializeField] private float initialComicFollowSpeed = 3.5f;

        [SerializeField] private float maxKaraokeFollowSpeed = 10f;
        [SerializeField] private float minKaraokeFollowSpeed = 0.1f;
        [SerializeField] private float initialKaraokeFollowSpeed = 3.5f;

        [SerializeField] private float maxRamenFollowSpeed = 10f;
        [SerializeField] private float minRamenFollowSpeed = 0.1f;
        [SerializeField] private float initialRamenFollowSpeed = 3.5f;


        public float FollowSpeed {
            get {
                return _desireType switch {
                    DesireType.Game => Mathf.Clamp(_followSpeed, minGameFollowSpeed, maxGameFollowSpeed),
                    DesireType.Comic => Mathf.Clamp(_followSpeed, minComicFollowSpeed, maxComicFollowSpeed),
                    DesireType.Karaoke => Mathf.Clamp(_followSpeed, minKaraokeFollowSpeed, maxKaraokeFollowSpeed),
                    DesireType.Ramen => Mathf.Clamp(_followSpeed, minRamenFollowSpeed, maxRamenFollowSpeed),
                    _ => 0f
                };
            }
        }

        public void InitializeParameters(DesireType desireType) {
            _desireType = desireType;
            _followSpeed = desireType switch {
                DesireType.Game => initialGameFollowSpeed,
                DesireType.Comic => initialComicFollowSpeed,
                DesireType.Karaoke => initialKaraokeFollowSpeed,
                DesireType.Ramen => initialRamenFollowSpeed,
                _ => 0f
            };
            ;
        }
    }
}