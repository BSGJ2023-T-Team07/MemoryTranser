using System;
using Cysharp.Threading.Tasks;
using MemoryTranser.Scripts.Game.MemoryBox;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using Constant = MemoryTranser.Scripts.Game.Util.Constant;

namespace MemoryTranser.Scripts.Game.Fairy {
    public class FairyCore : MonoBehaviour {
        #region コンポーネントの定義

        [SerializeField] private Rigidbody2D rigidbody2D;
        [SerializeField] private Animator animator;
        [SerializeField] private BoxCollider2D boxCollider2D;

        #endregion

        #region 変数の定義

        private FairyState _myState;
        private FairyParameters _myParameters = new();
        private MemoryBoxCore _holdingBox;
        private int _comboCount;

        private bool _isDash;
        private bool _hasBox;

        private bool _isControllable;

        private Vector2 _velocity;

        #endregion

        #region 定数の定義

        private const float HOLDABLE_DISTANCE = 4f;

        #endregion

        #region プロパティーの定義

        public FairyParameters MyParameters {
            get => _myParameters;
            set => _myParameters = value;
        }

        public FairyState MyState {
            get => _myState;
            set => _myState = value;
        }

        public bool IsControllable {
            get => _isControllable;
            set {
                _isControllable = value;
                if (!_isControllable) rigidbody2D.velocity = Vector2.zero;
            }
        }
        
        public int ComboCount {
            get => _comboCount;
            set {
                _comboCount = value;
                
                if(_holdingBox) MyParameters.UpdateWalkSpeedByWeightAndCombo(_holdingBox.Weight, value);
                else MyParameters.UpdateWalkSpeedByWeightAndCombo(0,value);
            }
        }

        #endregion

        #region Unityから呼ばれる

        private void FixedUpdate() {
            Move();
        }

        #endregion

        #region 操作入力時の処理

        public void OnMoveInput(InputAction.CallbackContext context) {
            if (!IsControllable && _velocity != Vector2.zero) {
                //フェーズ遷移時、速度が0
                _velocity = Vector2.zero;
                return;
            }

            if (!IsControllable) return;
            
            var moveInput = context.ReadValue<Vector2>();
            _velocity = moveInput * MyParameters.WalkSpeed;
        }


        public void OnHoldInput(InputAction.CallbackContext context) {
            if (!IsControllable) return;

            //もし既にBoxを持ってたら何もしない
            if (_holdingBox) return;

            var casts = Physics2D.CircleCastAll(transform.position, HOLDABLE_DISTANCE, Vector2.zero,
                0, Constant.MEMORY_BOX_LAYER_MASK);

            //もし近くにBoxがなかったら何もしない
            if (casts.Length == 0) return;

            var memoryBoxCore = GetNearestMemoryBoxCore(casts);
            if (memoryBoxCore) Hold(memoryBoxCore);
        }

        public void OnThrowInput(InputAction.CallbackContext context) {
            if (!IsControllable) return;

            Throw();
        }


        public void OnPutInput(InputAction.CallbackContext context) {
            if (!IsControllable) return;

            Put();
        }

        #endregion

        #region 行動の定義

        private void Move() {
            rigidbody2D.velocity = _velocity;
        }

        private void Hold(MemoryBoxCore memoryBoxCore) {
            memoryBoxCore.Held(transform);
            _holdingBox = memoryBoxCore;
            
            MyParameters.UpdateWalkSpeedByWeightAndCombo(_holdingBox.Weight,ComboCount);

            Debug.Log($"IDが{_holdingBox.BoxId}の記憶を持った");
        }

        private void Throw() {
            if (!_holdingBox) return;
            _holdingBox.Thrown(MyParameters.ThrowPower,
                rigidbody2D.velocity.normalized);

            Debug.Log($"IDが{_holdingBox.BoxId}の記憶を投げた");
            _holdingBox = null;
            MyParameters.UpdateWalkSpeedByWeightAndCombo(0,ComboCount);
        }

        private void Put() {
            if (!_holdingBox) return;
            _holdingBox.Put();

            Debug.Log($"IDが{_holdingBox.BoxId}の記憶を置いた");
            _holdingBox = null;
            MyParameters.UpdateWalkSpeedByWeightAndCombo(0,ComboCount);
        }

        #endregion

        private MemoryBoxCore GetNearestMemoryBoxCore(RaycastHit2D[] castArray) {
            var nearestDistance = float.MaxValue;
            var nearestIndex = -1;
            for (var i = 0; i < castArray.Length; i++) {
                var distance = Vector2.Distance(transform.position, castArray[i].transform.position);

                if (nearestDistance > distance) {
                    nearestDistance = distance;
                    nearestIndex = i;
                }
            }

            return castArray[nearestIndex].transform.GetComponent<MemoryBoxCore>();
        }

        public void InitializeFairy() {
            MyParameters = new FairyParameters();
            MyState = FairyState.IdlingWithoutMemory;

            MyParameters.InitializeParameters();
        }

        public async void AttackedByDesire() {
            IsControllable = false;
            ComboCount = 0;

            //Desireに当たると3秒停止
            await UniTask.Delay(3000);

            IsControllable = true;
        }
    }
}