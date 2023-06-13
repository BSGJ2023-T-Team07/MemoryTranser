using UnityEngine;

namespace MemoryTranser.Scripts.Game.Desire {
    public class DesireParameters {
        private float _followSpeed;

        //Desireの初行動開始もしくは再行動開始までのインターバル
        private float _actionRecoveryTime;

        public float FollowSpeed {
            get => _followSpeed;
            set => _followSpeed = value;
        }

        //インターバルのプロパティ
        public float ActionRecoveryTime {
            get => _actionRecoveryTime;
            set => _actionRecoveryTime = value;
        }
    }
}