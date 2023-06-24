using System;
using MemoryTranser.Scripts.Game.Fairy;
using MemoryTranser.Scripts.Game.GameManagers;
using UnityEngine;
using UnityEngine.AI;
using UniRx;
using UnityEngine.Serialization;

namespace MemoryTranser.Scripts.Game.Desire {
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class DesireCore : MonoBehaviour, IOnStateChangedToResult {
        #region コンポーネントの定義

        [HideInInspector] public Transform targetTransform;

        [SerializeField] private BoxCollider2D boxCollider2D;
        [SerializeField] private Rigidbody2D rb2D;
        [SerializeField] private SpriteRenderer spRr;
        [SerializeField] private NavMeshAgent navMeshAgent;

        #endregion

        #region 変数の定義

        [SerializeField] private DesireParameters myParameters = new();

        private DesireState _myState = DesireState.Freeze;
        private DesireType _myType;
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

        public DesireParameters MyParameters {
            get => myParameters;
            set => myParameters = value;
        }

        public Rigidbody2D Rb2D {
            get {
                if (!rb2D) {
                    rb2D = GetComponent<Rigidbody2D>();
                }

                return rb2D;
            }
        }

        public SpriteRenderer SpRr {
            get {
                if (!spRr) {
                    spRr = GetComponent<SpriteRenderer>();
                }

                return spRr;
            }
        }

        public DesireType MyType {
            get => _myType;
            set => _myType = value;
        }

        #endregion

        #region Unityから呼ばれる

        private void FixedUpdate() {
            if (_followFlag) {
                //DesireとFairyの距離を算出する
                Vector2 direction = (targetTransform.position - transform.position).normalized;

                //移動処理
                Rb2D.velocity = direction * myParameters.FollowSpeed;
            }
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (!other.gameObject.CompareTag("Fairy")) {
                return;
            }

            other.GetComponent<FairyCore>().BeAttackedByDesire();
            Disappear();
        }

        #endregion

        #region interfaceの実装

        public void OnStateChangedToResult() {
            _followFlag = false;

            _onBeAttacked.OnCompleted();
            _onDisappear.OnCompleted();

            _onDisappear.Dispose();
            _onBeAttacked.Dispose();
        }

        #endregion

        public void Appear(Vector3 spawnPos) {
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
    }
}