using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MemoryTranser.Scripts.Game.GameManagers;
using MemoryTranser.Scripts.Game.MemoryBox;
using MemoryTranser.Scripts.Game.Sound;
using UniRx;
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
        [SerializeField] private SpriteRenderer spRr;
        [SerializeField] private BoxCollider2D boxCollider2D;
        [SerializeField] private Transform memoryBoxHolderBottom;
        [SerializeField] private SpriteRenderer throwDirectionArrowSpRr;

        #endregion

        #region 変数の定義

        [SerializeField] private FairyParameters myParameters;

        [Header("煩悩に当たった時に操作不能になる時間(秒)")] [SerializeField]
        private float stunDurationSec = 1f;

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

        [Header("ブリンクによるMemoryBox押し出しの強さの倍率")] [SerializeField]
        private float pushBoxPowerMultiplier = 1f;

        [Header("ブリンクの方向の先行入力の猶予時間(秒)")] [SerializeField]
        private float precedeBlinkDirectionInputSec = 0.1f;

        [Header("何秒ゲージを貯めれば納品できるか")] [SerializeField]
        private float necessaryInputSecToOutput = 1f;

        private static readonly int AnimHasBox = Animator.StringToHash("hasBox");
        private static readonly int AnimIsWalking = Animator.StringToHash("isWalking");
        private static readonly int AnimIsFreezing = Animator.StringToHash("isFreezing");

        private FairyState _myState;
        private MemoryBoxCore _holdingBox;
        private int _comboCount;

        private bool _isControllable;
        private bool _isBlinkRecovered = true;
        private bool _isBlinking;
        private bool _applyCancelingBlink;
        private bool _isInOutputArea;

        private Vector2 _inputWalkDirection;
        private Vector2 _inputWalkDirectionBeforeZero;
        private Vector2 _inputThrowDirection;
        private Vector2 _blinkDirection;

        private float _remainingPrecedeBlinkDirectionInputSec;
        private float _nowInputSecToOutput;

        #endregion

        #region eventの定義

        private readonly Subject<Unit> _onOutputInput = new();
        public IObservable<Unit> OnOutputInput => _onOutputInput;

        #endregion

        #region プロパティーの定義

        private bool HasBox => _holdingBox;
        private bool CanBlink => blinkTicketCount > 0 && _isBlinkRecovered && _isControllable;

        public FairyParameters MyParameters => myParameters;

        public int BlinkTicketCount => blinkTicketCount;

        public Vector2 InputWalkDirection => _inputWalkDirection;
        public Vector2 InputWalkDirectionBeforeZero => _inputWalkDirectionBeforeZero;

        public bool IsBlinking => _isBlinking;

        public bool IsBlinkRecovered => _isBlinkRecovered;

        public bool ApplyCancelingBlink => _applyCancelingBlink;

        public bool IsControllable => _isControllable;

        public float NowInputSecToOutput => _nowInputSecToOutput;

        public FairyState MyState {
            get => _myState;
            set => _myState = value;
        }

        public int ComboCount {
            get => _comboCount;
            set {
                _comboCount = value;

                if (HasBox) {
                    myParameters.UpdateWalkSpeedByWeightAndCombo(_holdingBox.Weight, value);
                }
                else {
                    myParameters.UpdateWalkSpeedByWeightAndCombo(0, value);
                }
            }
        }

        #endregion

        #region Unityから呼ばれる

        private void Awake() {
            throwDirectionArrowSpRr.enabled = false;
        }

        private void Update() {
            AnimationChange();

            UpdateFairyState();

            #region 先行入力の判定

            if (_inputWalkDirectionBeforeZero != Vector2.zero && _remainingPrecedeBlinkDirectionInputSec > 0f) {
                _remainingPrecedeBlinkDirectionInputSec -= Time.deltaTime;

                if (_remainingPrecedeBlinkDirectionInputSec < 0f) {
                    _inputWalkDirectionBeforeZero = Vector2.zero;
                    _remainingPrecedeBlinkDirectionInputSec = -1;
                }
            }

            #endregion
        }

        private void FixedUpdate() {
            Move();
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.gameObject.layer == LayerMask.NameToLayer("OutputArea")) {
                _isInOutputArea = true;
            }
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (other.gameObject.layer == LayerMask.NameToLayer("OutputArea")) {
                _isInOutputArea = false;
                _nowInputSecToOutput = 0f;
            }
        }

        private void OnCollisionEnter2D(Collision2D other) {
            if (other.gameObject.layer == LayerMask.NameToLayer("SphereMemoryBox")) {
                _applyCancelingBlink = true;

                if (_isBlinking) {
                    PushSphereBox(other.gameObject.GetComponent<MemoryBoxCore>());
                }
            }

            if (other.gameObject.layer == LayerMask.NameToLayer("Desire")) {
                _applyCancelingBlink = true;
            }
        }

        private void OnCollisionExit2D(Collision2D other) {
            if (other.gameObject.layer == LayerMask.NameToLayer("SphereMemoryBox")) {
                _applyCancelingBlink = false;
            }

            if (other.gameObject.layer == LayerMask.NameToLayer("Desire")) {
                _applyCancelingBlink = false;
            }
        }

        #endregion


        #region 操作入力時の処理

        public void OnMoveInput(InputAction.CallbackContext context) {
            if (!_isControllable) {
                //操作不能かつ速度が0でなかったら速度を0にする
                if (_inputWalkDirection != Vector2.zero) {
                    _inputWalkDirection = Vector2.zero;
                    return;
                }

                //単に操作不能だったら何もしない
                return;
            }

            var moveInput = context.ReadValue<Vector2>();

            if (moveInput == Vector2.zero) {
                _inputWalkDirectionBeforeZero = _inputWalkDirection;
                _remainingPrecedeBlinkDirectionInputSec = precedeBlinkDirectionInputSec;
            }

            _inputWalkDirection = moveInput.normalized;
        }

        public void OnSelectInputDirection(InputAction.CallbackContext context) {
            if (!_isControllable) {
                return;
            }

            var directionInput = context.ReadValue<Vector2>();

            if (_isInOutputArea) {
                if (Vector2.Dot(directionInput, Vector2.down) > 0.6f) {
                    _nowInputSecToOutput += Time.deltaTime;
                }

                if (_nowInputSecToOutput > necessaryInputSecToOutput &&
                    Vector2.Dot(directionInput, Vector2.up) > 0.7f) {
                    _nowInputSecToOutput = 0f;
                    _onOutputInput.OnNext(Unit.Default);
                }
            }

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
            if (!_isControllable) {
                return;
            }

            //何もMemoryBoxを持っていなければ何もしない
            if (!HasBox) {
                return;
            }

            if (_inputThrowDirection == Vector2.zero) {
                Debug.Log("もっと勢いを付けて投げてください");
                return;
            }

            Throw();
        }

        public void OnHoldInput(InputAction.CallbackContext context) {
            //このフレームに完全に押されてなければ何もしない
            if (!context.action.WasPressedThisFrame()) {
                return;
            }

            //操作不能だったら何もしない
            if (!_isControllable) {
                return;
            }

            //もし既にBoxを持ってたら何もしない
            if (HasBox) {
                return;
            }

            var casts = Physics2D.CircleCastAll(transform.position, holdableDistance, Vector2.zero,
                0, LayerMask.GetMask("CubeMemoryBox"));

            //もし近くにBoxがなかったら何もしない
            if (casts.Length == 0) {
                return;
            }

            var holdableNearestBox = GetNearestHoldableCubeBox(casts);

            //近くに地面の置かれてるBoxがあったらHoldする
            if (holdableNearestBox && holdableNearestBox.MyState == MemoryBoxState.PlacedOnLevel) {
                Hold(holdableNearestBox);
            }
        }


        public void OnPutInput(InputAction.CallbackContext context) {
            //このフレームに完全に押されてなければ何もしない
            if (!context.action.WasPressedThisFrame()) {
                return;
            }

            //操作不能だったら何もしない
            if (!_isControllable) {
                return;
            }

            //何もMemoryBoxを持っていなければ何もしない
            if (!HasBox) {
                return;
            }

            Put();
        }

        public void OnBlinkInput(InputAction.CallbackContext context) {
            //このフレームに完全に押されてなければ何もしない
            if (!context.action.WasPressedThisFrame()) {
                return;
            }

            //操作不能だったら何もしない
            if (!_isControllable) {
                return;
            }

            //ブリンク不可能だったら何もしない
            if (!CanBlink) {
                return;
            }

            //ブリンクの方向が指定されてなかったら何もしない
            if (_inputWalkDirectionBeforeZero == Vector2.zero && _inputWalkDirection == Vector2.zero) {
                return;
            }

            _blinkDirection = _inputWalkDirection;
            if (_inputWalkDirection == Vector2.zero) {
                _blinkDirection = _inputWalkDirectionBeforeZero;
            }

            Blink(_blinkDirection);
        }

        #endregion

        #region 能動的行動の定義

        private void Move() {
            rb2D.velocity = _inputWalkDirection * myParameters.WalkSpeed;
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

        private void Blink(Vector2 blinkDirection) {
            _isControllable = false;
            blinkTicketCount--;
            _isBlinkRecovered = false;
            _isBlinking = true;

            var blinkTweenerCore = rb2D.DOMove(blinkDirection * blinkDistance, blinkDurationSec)
                .SetRelative().SetEase(Ease.OutExpo).OnKill(() => {
                    _isBlinking = false;
                    rb2D.velocity = Vector2.zero;
                    IsControllableTrueAfterBlinked();
                    IsBlinkRecoveredTrueAfterBlinked();
                }).OnComplete(() => {
                    _isBlinking = false;
                    IsControllableTrueAfterBlinked();
                    IsBlinkRecoveredTrueAfterBlinked();
                });

            blinkTweenerCore.OnUpdate(() => {
                if (_applyCancelingBlink) {
                    blinkTweenerCore.Kill();
                    rb2D.velocity = Vector2.zero;
                }
            });

            async void IsControllableTrueAfterBlinked() {
                await UniTask.Delay(TimeSpan.FromSeconds(reControllableSecAfterBlink));
                if (_myState != FairyState.Freeze) {
                    _isControllable = true;
                }
            }

            async void IsBlinkRecoveredTrueAfterBlinked() {
                await UniTask.Delay(TimeSpan.FromSeconds(blinkRecoverSec));
                _isBlinkRecovered = true;
            }
        }

        private void PushSphereBox(MemoryBoxCore sphereBox) {
            sphereBox.BePushed(_blinkDirection, blinkDistance / blinkDurationSec * pushBoxPowerMultiplier);
        }

        #endregion

        #region 受動的行動の定義

        public async void BeAttackedByDesire() {
            _isControllable = false;
            rb2D.velocity = Vector2.zero;
            ComboCount = 0;
            _myState = FairyState.Freeze;

            //Desireに当たると3秒停止
            await UniTask.Delay(TimeSpan.FromSeconds(stunDurationSec));

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

        private MemoryBoxCore GetNearestHoldableCubeBox(RaycastHit2D[] castArray) {
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
            if (_myState == FairyState.Freeze) {
                return;
            }

            if (HasBox) {
                _myState = rb2D.velocity.sqrMagnitude < Constant.DELTA
                    ? FairyState.IdlingWithBox
                    : FairyState.WalkingWithBox;
            }
            else {
                _myState = rb2D.velocity.sqrMagnitude < Constant.DELTA
                    ? FairyState.IdlingWithoutBox
                    : FairyState.WalkingWithoutBox;
            }
        }

        private void AnimationChange() {
            spRr.flipX = rb2D.velocity.x switch {
                < -Constant.DELTA => true,
                > Constant.DELTA => false,
                _ => spRr.flipX
            };

            animator.SetBool(AnimHasBox, _myState is FairyState.IdlingWithBox or FairyState.WalkingWithBox);
            animator.SetBool(AnimIsWalking, _myState is FairyState.WalkingWithBox or FairyState.WalkingWithoutBox);
            animator.SetBool(AnimIsFreezing, _myState == FairyState.Freeze);
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
            _onOutputInput.OnCompleted();
            _onOutputInput.Dispose();
        }

        #endregion
    }
}