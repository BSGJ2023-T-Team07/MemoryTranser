using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MemoryTranser.Scripts.Game.Desire;
using MemoryTranser.Scripts.Game.GameManagers;
using MemoryTranser.Scripts.Game.MemoryBox;
using MemoryTranser.Scripts.Game.Sound;
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
        [SerializeField] private Transform memoryBoxHolderBottom;
        [SerializeField] private SpriteRenderer throwDirectionArrowSpRr;
        [SerializeField] private DesireManager desireManager;

        #endregion

        #region 変数の定義

        [SerializeField] private FairyParameters myParameters;

        [Header("投げる方向の入力の閾値")] [SerializeField]
        private float selectDirectionArrowThreshold;

        [Header("MemoryBoxをHoldできる最大距離(円の半径)")] [SerializeField]
        private float holdableDistance = 4f;

        [Header("煩悩を倒したときにもらえるブリンクチケットの数")] [SerializeField]
        private int additionalBlinkTicketOnDefeatDesire = 1;

        [Header("ブリンク可能回数")] [SerializeField] private int blinkTicketCount;

        [Header("ブリンクの距離")] public float blinkDistance = 0.5f;

        [Header("ブリンクで移動しきるまでの時間(秒)")] public float blinkDurationSec = 0.5f;

        [Header("ブリンクし終わってから操作可能になるまでの時間(秒)")] public float reControllableSecAfterBlink = 0.3f;

        [Header("ブリンクし終わってから再度ブリンクできるまでの時間(秒)")]
        public float blinkRecoverSec = 0.7f;


        private FairyState _myState;
        private MemoryBoxCore _holdingBox;
        private int _comboCount;

        private bool _isControllable;
        private bool _isBlinkRecovered = true;

        private Vector2 _inputVelocity;
        private Vector2 _inputThrowDirection;

        #endregion

        #region プロパティーの定義

        private bool HasBox => _holdingBox;
        private bool CanBlink => blinkTicketCount > 0 && _isBlinkRecovered && _isControllable;

        public FairyParameters MyParameters => myParameters;

        public int BlinkTicketCount => blinkTicketCount;

        public FairyState MyState {
            get => _myState;
            set => _myState = value;
        }

        public int ComboCount {
            get => _comboCount;
            set {
                _comboCount = value;

                if (HasBox) myParameters.UpdateWalkSpeedByWeightAndCombo(_holdingBox.Weight, value);
                else myParameters.UpdateWalkSpeedByWeightAndCombo(0, value);
            }
        }

        #endregion

        #region Unityから呼ばれる

        private void Awake() {
            throwDirectionArrowSpRr.enabled = false;
        }

        private void Update() {
            UpdateFairyState();
        }

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
            _inputVelocity = moveInput * myParameters.WalkSpeed;
        }

        public void OnSelectInputDirection(InputAction.CallbackContext context) {
            if (!_isControllable) return;

            var directionInput = context.ReadValue<Vector2>();

            //MemoryBoxを持っていないか、投げるための入力が不十分だったら何もしない
            if (!HasBox || directionInput.sqrMagnitude < selectDirectionArrowThreshold) {
                _inputThrowDirection = Vector2.zero;
                throwDirectionArrowSpRr.enabled = false;
            }
            else {
                _inputThrowDirection = directionInput.normalized;
                throwDirectionArrowSpRr.transform.rotation = Quaternion.Euler(0, 0,
                    Vector2.SignedAngle(Vector2.right, _inputThrowDirection));
                throwDirectionArrowSpRr.enabled = true;
            }
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

            var casts = Physics2D.CircleCastAll(transform.position, holdableDistance, Vector2.zero,
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

        public void OnBlinkInput(InputAction.CallbackContext context) {
            //このフレームに完全に押されてなければ何もしない
            if (!context.action.WasPressedThisFrame()) return;

            //ブリンク不可能だったら何もしない
            if (!CanBlink) return;

            Blink();
        }

        #endregion

        #region 能動的行動の定義

        private void Move() {
            rb2D.velocity = _inputVelocity;
        }

        private void Hold(MemoryBoxCore memoryBoxCore) {
            memoryBoxCore.BeHeld(memoryBoxHolderBottom);
            _holdingBox = memoryBoxCore;
            throwDirectionArrowSpRr.transform.position = _holdingBox.transform.position;

            myParameters.UpdateWalkSpeedByWeightAndCombo(_holdingBox.Weight, ComboCount);

            Debug.Log($"IDが{_holdingBox.BoxId}の記憶を持った");

            SeManager.I.Play(SEs.HoldBox);
        }

        private void Throw() {
            _holdingBox.BeThrown(myParameters.ThrowPower,
                _inputThrowDirection);

            Debug.Log($"IDが{_holdingBox.BoxId}の記憶を投げた");
            _holdingBox = null;
            throwDirectionArrowSpRr.enabled = false;
            myParameters.UpdateWalkSpeedByWeightAndCombo(0, ComboCount);

            SeManager.I.Play(SEs.ThrowBox);
        }

        private void Put() {
            _holdingBox.BePut();

            Debug.Log($"IDが{_holdingBox.BoxId}の記憶を置いた");
            _holdingBox = null;
            myParameters.UpdateWalkSpeedByWeightAndCombo(0, ComboCount);

            SeManager.I.Play(SEs.PutBox);
        }

        private async void Blink() {
            _isControllable = false;
            blinkTicketCount--;
            _isBlinkRecovered = false;

            transform.DOMove(_inputVelocity * blinkDistance, blinkDurationSec)
                .SetRelative().SetEase(Ease.OutExpo);

            await UniTask.Delay(
                TimeSpan.FromSeconds(blinkDurationSec + reControllableSecAfterBlink));
            _isControllable = true;

            await UniTask.Delay(
                TimeSpan.FromSeconds(blinkRecoverSec - reControllableSecAfterBlink));
            _isBlinkRecovered = true;
        }

        #endregion

        #region 受動的行動の定義

        public async void BeAttackedByDesire() {
            _isControllable = false;
            ComboCount = 0;
            _myState = FairyState.Freeze;

            //Desireに当たると3秒停止
            await UniTask.Delay(TimeSpan.FromSeconds(3f));

            _isControllable = true;
            _myState = HasBox ? FairyState.IdlingWithBox : FairyState.IdlingWithoutBox;
        }

        #endregion

        public int AddBlinkTicketOnDefeatDesire() {
            return AddBlinkTicket(additionalBlinkTicketOnDefeatDesire);
        }

        private int AddBlinkTicket(int add) {
            blinkTicketCount += add;
            return blinkTicketCount;
        }

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
            myParameters = new FairyParameters();

            myParameters.InitializeParameters();
        }

        private void UpdateFairyState() {
            if (_myState == FairyState.Freeze) return;

            if (HasBox)
                _myState = rb2D.velocity.sqrMagnitude < Constant.DELTA
                    ? FairyState.IdlingWithBox
                    : FairyState.WalkingWithBox;
            else
                _myState = rb2D.velocity.sqrMagnitude < Constant.DELTA
                    ? FairyState.IdlingWithoutBox
                    : FairyState.WalkingWithoutBox;
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