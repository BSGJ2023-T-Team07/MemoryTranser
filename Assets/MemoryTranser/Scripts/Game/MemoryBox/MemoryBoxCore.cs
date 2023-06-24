using System;
using MemoryTranser.Scripts.Game.Desire;
using MemoryTranser.Scripts.Game.GameManagers;
using MemoryTranser.Scripts.Game.Util;
using UniRx;
using UnityEngine;
using Unit = UniRx.Unit;

namespace MemoryTranser.Scripts.Game.MemoryBox {
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class MemoryBoxCore : MonoBehaviour, IOnStateChangedToResult {
        #region コンポーネントの定義

        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private BoxCollider2D bc2D;
        [SerializeField] private Rigidbody2D rb2D;
        [SerializeField] private ParticleSystem smokeParticle;

        #endregion

        #region 変数の定義

        private BoxMemoryType _boxMemoryType = new();
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
                if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
                return spriteRenderer;
            }
            set => spriteRenderer = value;
        }

        public BoxCollider2D Bc2D {
            get {
                if (!bc2D) bc2D = GetComponent<BoxCollider2D>();
                return bc2D;
            }
            set => bc2D = value;
        }

        public Rigidbody2D Rb2D {
            get {
                if (!rb2D) rb2D = GetComponent<Rigidbody2D>();
                return rb2D;
            }
            set => rb2D = value;
        }

        public ParticleSystem SmokeParticle {
            get {
                if (!smokeParticle) smokeParticle = transform.GetComponent<ParticleSystem>();
                return smokeParticle;
            }
            set => smokeParticle = value;
        }

        public BoxMemoryType BoxMemoryType {
            get => _boxMemoryType;
            set => _boxMemoryType = value;
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
            if (_myState == MemoryBoxState.Held)
                transform.position = _holderTransform.position + (Vector3)Vector2.up * _diff;

            if (_myState == MemoryBoxState.PlacedOnLevel) {
                //地面に置かれてる状態で大きい速度を持っていたら毎フレーム減速する
                if (rb2D.velocity.magnitude > Constant.DELTA)
                    rb2D.velocity *= 0.98f;
                //速度が一定以下になったら、速度を0にする
                else
                    rb2D.velocity = Vector2.zero;
            }
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (_myState != MemoryBoxState.Thrown) return;


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

        #endregion

        #region 受動的行動の定義

        public void BeHeld(Transform holderBottomTransform) {
            _myState = MemoryBoxState.Held;
            _holderTransform = holderBottomTransform;
            var myTransform = transform;
            _diff = Bc2D.size.y / 2 * myTransform.localScale.y;
            myTransform.position = _holderTransform.position + (Vector3)Vector2.up * _diff;
            Rb2D.velocity = Vector2.zero;
            Bc2D.enabled = false;
        }

        public void BeThrown(float throwPower, Vector2 throwDirection) {
            _myState = MemoryBoxState.Thrown;
            Bc2D.enabled = true;
            Bc2D.isTrigger = true;
            Rb2D.velocity = throwDirection * throwPower / Weight * 2f;
        }

        public void BePut() {
            _myState = MemoryBoxState.PlacedOnLevel;
            Bc2D.enabled = true;
            Bc2D.isTrigger = false;
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
            Bc2D.enabled = false;
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