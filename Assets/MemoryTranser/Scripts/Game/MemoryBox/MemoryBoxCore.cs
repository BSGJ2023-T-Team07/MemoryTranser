using System;
using MemoryTranser.Scripts.Game.Desire;
using MemoryTranser.Scripts.Game.GameManagers;
using MemoryTranser.Scripts.Game.Util;
using UniRx;
using UnityEngine;
using Unit = UniRx.Unit;

namespace MemoryTranser.Scripts.Game.MemoryBox {
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class MemoryBoxCore : MonoBehaviour, IOnStateChangedToResult {
        #region コンポーネントの定義

        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Rigidbody2D rb2D;
        [SerializeField] private ParticleSystem smokeParticle;

        [SerializeField] private CircleCollider2D cc2D;

        #endregion

        #region 変数の定義

        private BoxMemoryType _boxMemoryType = new();
        private MemoryBoxShapeType _boxShapeType = new();
        private MemoryBoxState _myState = new();
        private float _weight;
        private int _boxId;

        private float _diff;
        private Transform _holderTransform;

        #endregion

        #region eventの定義

        private readonly Subject<Unit> _onDisappear = new();
        public IObservable<Unit> OnDisappear => _onDisappear;

        #endregion


        #region プロパティーの定義

        public SpriteRenderer SpRr {
            get {
                if (!spriteRenderer) {
                    spriteRenderer = GetComponent<SpriteRenderer>();
                }

                return spriteRenderer;
            }
            set => spriteRenderer = value;
        }

        public CircleCollider2D Cc2D {
            get {
                if (!cc2D) {
                    cc2D = GetComponent<CircleCollider2D>();
                }

                return cc2D;
            }
            set => cc2D = value;
        }

        public Rigidbody2D Rb2D {
            get {
                if (!rb2D) {
                    rb2D = GetComponent<Rigidbody2D>();
                }

                return rb2D;
            }
            set => rb2D = value;
        }

        public ParticleSystem SmokeParticle {
            get {
                if (!smokeParticle) {
                    smokeParticle = transform.GetComponent<ParticleSystem>();
                }

                return smokeParticle;
            }
            set => smokeParticle = value;
        }

        public BoxMemoryType BoxMemoryType {
            get => _boxMemoryType;
            set => _boxMemoryType = value;
        }

        public MemoryBoxShapeType BoxShapeType {
            get => _boxShapeType;
            set => _boxShapeType = value;
        }

        public MemoryBoxState MyState {
            get => _myState;
            set => _myState = value;
        }

        public float Weight {
            get => _weight;
            set => _weight = value;
        }

        public int BoxId {
            get => _boxId;
            set => _boxId = value;
        }

        #endregion

        #region Unityから呼ばれる

        private void Update() {
            if (_myState == MemoryBoxState.Held) {
                transform.position = _holderTransform.position + (Vector3)Vector2.up * _diff;
            }

            if (_myState == MemoryBoxState.PlacedOnLevel) {
                //地面に置かれてる状態で大きい速度を持っていたら毎フレーム減速する
                if (Rb2D.velocity.sqrMagnitude > Constant.DELTA) {
                    Rb2D.velocity *= 0.98f;
                }
                //速度が一定以下になったら、速度を0にする
                else {
                    Rb2D.velocity = Vector2.zero;
                }
            }

            if (_boxShapeType == MemoryBoxShapeType.Sphere && _myState == MemoryBoxState.Flying) {
                if (Rb2D.velocity.sqrMagnitude > Constant.DELTA) {
                    Rb2D.velocity *= 0.99f;
                }
                else {
                    Rb2D.velocity = Vector2.zero;
                    _myState = MemoryBoxState.PlacedOnLevel;
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (_myState != MemoryBoxState.Flying) {
                return;
            }


            if (other.gameObject.layer == LayerMask.NameToLayer("Desire")) {
                //DesireにThrow状態のMemoryBoxが当たった時に以下の関数を呼び出す
                AttackDesire(other.gameObject.GetComponent<DesireCore>());
                return;
            }

            //消失壁に当たったら消える
            if (other.gameObject.layer == LayerMask.NameToLayer("LostWall")) {
                Disappear();
                return;
            }
        }

        private void OnCollisionEnter2D(Collision2D other) {
            if (_myState == MemoryBoxState.Flying && other.gameObject.layer == LayerMask.NameToLayer("Desire")) {
                AttackDesire(other.gameObject.GetComponent<DesireCore>());
            }
        }

        #endregion

        #region 受動的行動の定義

        public void BeHeld(Transform holderBottomTransform) {
            _myState = MemoryBoxState.Held;
            _holderTransform = holderBottomTransform;
            var myTransform = transform;
            _diff = Cc2D.radius / 2 * myTransform.localScale.y;
            myTransform.position = _holderTransform.position + (Vector3)Vector2.up * _diff;
            Rb2D.velocity = Vector2.zero;
            Cc2D.enabled = false;
        }

        public void BeThrown(float throwPower, Vector2 throwDirection) {
            _myState = MemoryBoxState.Flying;
            Cc2D.enabled = true;
            Cc2D.isTrigger = true;
            Rb2D.velocity = throwDirection * throwPower / Weight * 2f;
        }

        public void BePushed(Vector2 pushedDirection, float pushPower) {
            _myState = MemoryBoxState.Flying;
            Debug.Log($"pushedDirection: {pushedDirection}, pushedPower: {pushPower}");
            Rb2D.velocity = pushedDirection * pushPower / Weight;
        }

        public void BePut() {
            _myState = MemoryBoxState.PlacedOnLevel;
            Cc2D.enabled = true;
            Cc2D.isTrigger = false;
        }

        #endregion

        #region 能動的行動の定義

        private static void AttackDesire(DesireCore desire) {
            //Desireの命中時の処理を実行する
            desire.BeAttacked();
        }

        public void Disappear() {
            _myState = MemoryBoxState.Disappeared;
            SpRr.enabled = false;
            Cc2D.enabled = false;
            Rb2D.velocity = Vector2.zero;
            smokeParticle.Stop();

            _onDisappear.OnNext(Unit.Default);
        }

        #endregion

        #region interfaceの実装

        public void OnStateChangedToResult() {
            _onDisappear.OnCompleted();
            _onDisappear.Dispose();
        }

        #endregion
    }
}