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
    public class MemoryBoxCore : MonoBehaviour, IOnStateChangedToFinished {
        #region コンポーネントの定義

        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Rigidbody2D rb2D;
        [SerializeField] private ParticleSystem smokeParticle;
        [SerializeField] private TrailRenderer trail;
        [SerializeField] private CircleCollider2D cc2D;

        #endregion

        #region 変数の定義

        [HideInInspector] public bool isOutput;

        private Renderer _particleRenderer;

        private BoxMemoryType _boxMemoryType = new();
        private MemoryBoxShapeType _boxShapeType = new();
        private MemoryBoxState _myState = new();
        private float _weight;
        private int _boxId;

        private float _diffFromHolder;
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
        }

        public CircleCollider2D Cc2D {
            get {
                if (!cc2D) {
                    cc2D = GetComponent<CircleCollider2D>();
                }

                return cc2D;
            }
        }

        public Rigidbody2D Rb2D {
            get {
                if (!rb2D) {
                    rb2D = GetComponent<Rigidbody2D>();
                }

                return rb2D;
            }
        }

        public ParticleSystem SmokeParticle {
            get {
                if (!smokeParticle) {
                    smokeParticle = transform.GetComponent<ParticleSystem>();
                }

                return smokeParticle;
            }
        }

        public TrailRenderer Trail {
            get {
                if (!trail) {
                    trail = transform.GetComponent<TrailRenderer>();
                }

                return trail;
            }
        }

        public Renderer ParticleRenderer {
            get {
                if (!_particleRenderer) {
                    _particleRenderer = SmokeParticle.GetComponent<Renderer>();
                }

                return _particleRenderer;
            }
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

        private void FixedUpdate() {
            if (isOutput) {
                return;
            }

            if (_myState == MemoryBoxState.Held) {
                transform.position = _holderTransform.position + (Vector3)Vector2.up * _diffFromHolder;
            }

            if (_myState == MemoryBoxState.PlacedOnLevel) {
                //地面に置かれてる状態で大きい速度を持っていたら毎フレーム減速する
                if (Rb2D.velocity.sqrMagnitude > Constant.DELTA) {
                    Rb2D.velocity *= 0.95f;
                }
                //速度が一定以下になったら、速度を0にする
                else {
                    Rb2D.velocity = Vector2.zero;
                }
            }

            if (_boxShapeType == MemoryBoxShapeType.Sphere && _myState == MemoryBoxState.Flying) {
                if (Rb2D.velocity.sqrMagnitude > Constant.DELTA) {
                    Rb2D.velocity *= 0.95f;
                }
                else {
                    Rb2D.velocity = Vector2.zero;
                    Trail.enabled = false;
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

        public void BeHeld(Transform holderTransform) {
            _myState = MemoryBoxState.Held;
            _holderTransform = holderTransform;
            var myTransform = transform;
            _diffFromHolder = Cc2D.radius * myTransform.localScale.y;
            myTransform.position = _holderTransform.position + Vector3.up * _diffFromHolder;
            Rb2D.velocity = Vector2.zero;
            Cc2D.enabled = false;
            SpRr.sortingLayerID = SortingLayer.NameToID("ForeFairy");
            ParticleRenderer.sortingLayerID = SortingLayer.NameToID("ForeFairy");
        }

        public void BeThrown(float throwPower, Vector2 throwDirection) {
            _myState = MemoryBoxState.Flying;
            Cc2D.enabled = true;
            Cc2D.isTrigger = true;
            Trail.enabled = true;
            Rb2D.velocity = throwDirection * throwPower / Weight * 2f;
            SpRr.sortingLayerID = SortingLayer.NameToID("MemoryBox");
        }

        public void BePushed(Vector2 pushedDirection, float pushPower) {
            _myState = MemoryBoxState.Flying;
            Trail.enabled = true;
            Rb2D.velocity = pushedDirection * pushPower / Weight;
            SpRr.sortingLayerID = SortingLayer.NameToID("ForeMemoryBox");
        }

        public void BePut() {
            transform.position = _holderTransform.position +
                                 Vector3.down * (_holderTransform.localPosition.y / transform.localScale.y - 0.2f);
            _myState = MemoryBoxState.PlacedOnLevel;
            Cc2D.enabled = true;
            Cc2D.isTrigger = false;
            SpRr.sortingLayerID = SortingLayer.NameToID("MemoryBox");
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
            Trail.enabled = false;

            _onDisappear.OnNext(Unit.Default);
        }

        #endregion

        #region interfaceの実装

        public void OnStateChangedToFinished() {
            _onDisappear.OnCompleted();
            _onDisappear.Dispose();
        }

        #endregion
    }
}