using System;
using MemoryTranser.Scripts.Game.Fairy;
using MemoryTranser.Scripts.Game.GameManagers;
using UnityEngine;
using UnityEngine.AI;
using UniRx;

namespace MemoryTranser.Scripts.Game.Desire {
    public class DesireCore : MonoBehaviour, IOnStateChangedToResult {
        #region コンポーネントの定義

        [HideInInspector] public Transform targetTransform;

        [SerializeField] private BoxCollider2D boxCollider2D;
        [SerializeField] private Rigidbody2D rb2D;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private NavMeshAgent navMeshAgent;

        #endregion

        #region 変数の定義

        private DesireState _myState;
        private DesireType _myType;
        private DesireParameters _myParameters;
        private Vector3 _followPos;

        private bool _followFlag;

        #endregion

        #region eventの定義

        private readonly Subject<Unit> _onDisappear = new();
        public IObservable<Unit> OnDisappear => _onDisappear;

        private readonly Subject<Unit> _onBeAttacked = new();
        public IObservable<Unit> OnBeAttacked => _onBeAttacked;

        #endregion

        #region プロパティーの定義

        public Rigidbody2D Rb2D {
            get {
                if (!rb2D) rb2D = GetComponent<Rigidbody2D>();
                return rb2D;
            }
        }

        #endregion

        #region Unityから呼ばれる

        private void FixedUpdate() {
            if (_followFlag) {
                //DesireとFairyの距離を算出する
                Vector2 direction = (targetTransform.position - transform.position).normalized;

                //移動処理
                Rb2D.velocity = direction * _myParameters.FollowSpeed;
            }
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (!other.gameObject.CompareTag("Fairy")) return;

            other.GetComponent<FairyCore>().BeAttackedByDesire();
            Disappear();
        }

        #endregion

        #region interfaceの実装

        public void OnStateChangedToResult() {
            _followFlag = false;
        }

        #endregion

        public void Appear(Vector3 spawnPos) {
            InitializeDesire();
            transform.position = spawnPos;
            _myState = DesireState.FollowingFairy;
            _followFlag = true;
        }

        private void Disappear() {
            //ステータスを更新する
            _myState = DesireState.Freeze;

            //消失した通知を出す
            _onDisappear.OnNext(Unit.Default);

            //Desireを非表示にする
            gameObject.SetActive(false);
        }


        public void BeAttacked() {
            _onBeAttacked.OnNext(Unit.Default);
            Disappear();
        }

        /// <summary>
        /// Desireの初期化関数
        /// </summary>
        private void InitializeDesire() {
            _myParameters = new DesireParameters();
            _myState = DesireState.Freeze;

            _myParameters.FollowSpeed = FairyParameters.INITIAL_WALK_SPEED * 0.8f;
        }

        private void OnDestroy() {
            _onBeAttacked.OnCompleted();
            _onDisappear.OnCompleted();

            _onDisappear.Dispose();
            _onBeAttacked.Dispose();
        }
    }
}