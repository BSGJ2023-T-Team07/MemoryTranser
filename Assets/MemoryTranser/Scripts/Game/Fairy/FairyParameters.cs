using UnityEngine;

namespace MemoryTranser.Scripts.Game.Fairy {
    public class FairyParameters {
        #region 変数の定義

        private float _walkSpeed;
        private float _throwPower;

        #endregion

        #region 定数の定義

        public const float MAX_WALK_SPEED = 10f;
        public const float MIN_WALK_SPEED = 0f;
        public const float INITIAL_WALK_SPEED = 8f;

        public const float MAX_THROW_POWER = 10f;
        public const float MIN_THROW_POWER = 0f;
        public const float INITIAL_THROW_POWER = 10f;

        #endregion

        #region プロパティーの定義

        public float WalkSpeed => Mathf.Clamp(_walkSpeed, MIN_WALK_SPEED, MAX_WALK_SPEED);

        public float ThrowPower => Mathf.Clamp(_throwPower, MIN_THROW_POWER, MAX_THROW_POWER);

        #endregion

        #region パラメーター変化関連

        public void InitializeParameters() {
            Debug.Log("FairyのInitializeParametersが呼ばれました");
            _walkSpeed = INITIAL_WALK_SPEED;
            _throwPower = INITIAL_THROW_POWER;
        }

        public void UpdateWalkSpeedByWeightAndCombo(float weight, int comboCount) {
            //TODO: 重さとコンボ数によって歩く速さを変化させる式の吟味
            _walkSpeed = INITIAL_WALK_SPEED - weight + comboCount / 10f;
        }

        #endregion
    }
}