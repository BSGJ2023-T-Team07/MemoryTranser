using UnityEngine;
using UnityEngine.Serialization;

namespace MemoryTranser.Scripts.Game.Fairy {
    [System.Serializable]
    public class FairyParameters {
        #region 変数の定義

        private float _walkSpeed;
        private float _throwPower;

        #endregion

        #region 定数の定義

        [SerializeField] private float maxWalkSpeed = 30f;
        [SerializeField] private float minWalkSpeed = 0f;
        [SerializeField] private float initialWalkSpeed = 8f;

        [SerializeField] private float maxThrowPower = 10f;
        [SerializeField] private float minThrowPower = 0f;
        [SerializeField] private float initialThrowPower = 10f;

        #endregion

        #region プロパティーの定義

        public float WalkSpeed => Mathf.Clamp(_walkSpeed, minWalkSpeed, maxWalkSpeed);

        public float ThrowPower => Mathf.Clamp(_throwPower, minThrowPower, maxThrowPower);

        #endregion

        #region パラメーター変化関連

        public void InitializeParameters() {
            Debug.Log("FairyのInitializeParametersが呼ばれました");
            _walkSpeed = initialWalkSpeed;
            _throwPower = initialThrowPower;
        }

        public void UpdateWalkSpeedByWeightAndCombo(float weight, int comboCount) {
            //TODO: 重さとコンボ数によって歩く速さを変化させる式の吟味
            _walkSpeed = initialWalkSpeed - weight + comboCount / 10f;
        }

        #endregion
    }
}