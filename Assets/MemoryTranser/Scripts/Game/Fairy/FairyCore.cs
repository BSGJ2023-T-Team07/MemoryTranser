using System;
using Cysharp.Threading.Tasks;
using MemoryTranser.Scripts.Game.GameManagers;
using MemoryTranser.Scripts.Game.MemoryBox;
using MemoryTranser.Scripts.Game.Sound;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using Constant = MemoryTranser.Scripts.Game.Util.Constant;

namespace MemoryTranser.Scripts.Game.Fairy {
    public class FairyCore : MonoBehaviour, IOnStateChangedToInitializing, IOnStateChangedToReady,
        IOnStateChangedToPlaying,
        IOnStateChangedToResult {
        #region コンポーネントの定義

        [SerializeField] private Rigidbody2D rb2D;
        [SerializeField] private Animator animator;
        [SerializeField] private BoxCollider2D boxCollider2D;
        [SerializeField] private AudioClip[] seClips;
        GameObject SE_Manager;
        SeManager seManager;

        #endregion

        #region 変数の定義

        private FairyState _myState;
        private FairyParameters _myParameters = new();
        private MemoryBoxCore _holdingBox;
        private int _comboCount;

        private bool _isControllable;

        private Vector2 _inputVelocity;
        private Vector2 _inputThrowDirection;

        #endregion

        #region 定数の定義

        private const float HOLDABLE_DISTANCE = 4f;

        #endregion

        #region プロパティーの定義

        public bool HasBox => _holdingBox;

        public FairyParameters MyParameters {
            get => _myParameters;
            private set => _myParameters = value;
        }

        public FairyState MyState {
            get => _myState;
            set => _myState = value;
        }

        public int ComboCount {
            get => _comboCount;
            set {
                _comboCount = value;

                if (HasBox) MyParameters.UpdateWalkSpeedByWeightAndCombo(_holdingBox.Weight, value);
                else MyParameters.UpdateWalkSpeedByWeightAndCombo(0, value);
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
            if (!_isControllable) {
                //操作不能かつ速度が0でなかったら速度を0にする
                if (_inputVelocity != Vector2.zero) {
                    _inputVelocity = Vector2.zero;
                    return;
                }

                //単に操作不能だったら何もしない
                return;
            }

            var moveInput = context.ReadValue<Vector2>();
            _inputVelocity = moveInput * MyParameters.WalkSpeed;
        }

        public void OnSelectInputDirection(InputAction.CallbackContext context) {
            if (!_isControllable) return;

            var directionInput = context.ReadValue<Vector2>();
            _inputThrowDirection = directionInput;
        }

        public void OnThrowInput(InputAction.CallbackContext context) {
            //操作不能だったら何もしない
            if (!_isControllable) return;

            //何もMemoryBoxを持っていなければ何もしない
            if (!HasBox) return;
            if (_inputThrowDirection == Vector2.zero) {
                Debug.Log("もっと勢いを付けて投げてください");
                return;
            }

            Throw();
        }

        public void OnHoldInput(InputAction.CallbackContext context) {
            //このフレームに完全に押されてなければ何もしない
            if (!context.action.WasPressedThisFrame()) return;

            //操作不能だったら何もしない
            if (!_isControllable) return;

            //もし既にBoxを持ってたら何もしない
            if (HasBox) return;

            var casts = Physics2D.CircleCastAll(transform.position, HOLDABLE_DISTANCE, Vector2.zero,
                0, Constant.MEMORY_BOX_LAYER_MASK);

            //もし近くにBoxがなかったら何もしない
            if (casts.Length == 0) return;

            var holdableNearestBox = GetNearestMemoryBoxCore(casts);

            //近くに地面の置かれてるBoxがあったらHoldする
            if (holdableNearestBox && holdableNearestBox.MyState == MemoryBoxState.PlacedOnLevel)
                Hold(holdableNearestBox);
        }


        public void OnPutInput(InputAction.CallbackContext context) {
            //このフレームに完全に押されてなければ何もしない
            if (!context.action.WasPressedThisFrame()) return;

            //操作不能だったら何もしない
            if (!_isControllable) return;

            //何もMemoryBoxを持っていなければ何もしない
            if (!HasBox) return;

            Put();
        }

        #endregion

        #region 能動的行動の定義

        private void Move() {
            rb2D.velocity = _inputVelocity;
        }

        private void Hold(MemoryBoxCore memoryBoxCore) {
            memoryBoxCore.BeHeld(transform);
            _holdingBox = memoryBoxCore;

            MyParameters.UpdateWalkSpeedByWeightAndCombo(_holdingBox.Weight, ComboCount);

            Debug.Log($"IDが{_holdingBox.BoxId}の記憶を持った");

            seManager.audioSource.PlayOneShot(seManager.seClips[0]);
        }

        private void Throw() {
            _holdingBox.BeThrown(MyParameters.ThrowPower,
                _inputThrowDirection.normalized);

            Debug.Log($"IDが{_holdingBox.BoxId}の記憶を投げた");
            _holdingBox = null;
            MyParameters.UpdateWalkSpeedByWeightAndCombo(0, ComboCount);

            seManager.audioSource.PlayOneShot(seManager.seClips[1]);
        }

        private void Put() {
            _holdingBox.BePut();

            Debug.Log($"IDが{_holdingBox.BoxId}の記憶を置いた");
            _holdingBox = null;
            MyParameters.UpdateWalkSpeedByWeightAndCombo(0, ComboCount);

            seManager.audioSource.PlayOneShot(seManager.seClips[0]);
        }

        #endregion

        #region 受動的行動の定義

        public async void BeAttackedByDesire() {
            _isControllable = false;
            ComboCount = 0;

            //Desireに当たると3秒停止
            await UniTask.Delay(TimeSpan.FromSeconds(3f));

            _isControllable = true;
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

        private void InitializeFairy() {
            MyParameters = new FairyParameters();

            MyParameters.InitializeParameters();

            SE_Manager = GameObject.Find("SE_Manager");
            seManager = SE_Manager.GetComponent<SeManager>();
        }

        #region interfaceの実装

        public void OnStateChangedToInitializing() {
            InitializeFairy();
        }

        public void OnStateChangedToReady() { }

        public void OnStateChangedToPlaying() {
            _isControllable = true;
        }

        public void OnStateChangedToResult() {
            _isControllable = false;
        }

        #endregion
    }
}