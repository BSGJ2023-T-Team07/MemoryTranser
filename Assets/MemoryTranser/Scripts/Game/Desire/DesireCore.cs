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
        
        //author コメダ デザイア消失時のエフェクト
            //[SerializeField] private 
            
        #endregion

        #region 変数の定義

        [SerializeField] private DesireParameters myParameters = new();

        [Header("MemoryBoxを押し出す強さ")] [SerializeField]
        private float pushBoxPower = 20f;

        private DesireState _myState = DesireState.Freeze;
        private DesireType _myType;
        private Vector3 _followPos;

        #endregion

        #region eventの定義

        private readonly Subject<Unit> _onDisappear = new();
        public IObservable<Unit> OnDisappear => _onDisappear;

        private readonly Subject<Unit> _onBeAttacked = new();
        public IObservable<Unit> OnBeAttacked => _onBeAttacked;

        #endregion

        #region プロパティーの定義

        public DesireParameters MyParameters => myParameters;

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

        public bool FollowFlag { get; set; }

        #endregion

        #region Unityから呼ばれる

        private void FixedUpdate() {
            if (!FollowFlag) {
                return;
            }

            //DesireとFairyの距離を算出する
            Vector2 direction = (targetTransform.position - transform.position).normalized;

            //移動処理
            Rb2D.velocity = direction * myParameters.FollowSpeed;
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (!other.gameObject.CompareTag("Fairy")) {
                return;
            }

            other.GetComponent<FairyCore>().BeAttackedByDesire();
            Disappear();
        }

        private void OnCollisionEnter2D(Collision2D other) {
            if (!other.gameObject.CompareTag("MemoryBox")) {
                return;
            }

            var distanceVector = (other.transform.position - transform.position).normalized;
            var myVel = Rb2D.velocity.normalized;
            var dot = Vector3.Dot(myVel, distanceVector);
            var cross = Vector3.Cross(myVel, distanceVector);

            var angle = 0f;

            //十分に正面から当たっている場合、押し出すように条件分岐
            if (dot > 0.2f) {
                angle = cross.z > 0 ? 90f : -90f;
            }

            //重い箱は、押し出しがより遅くなる
            // other.rigidbody.AddForce((Vector2)(Quaternion.Euler(0, 0, angle) * myVel) * pushBoxPower,
            //     ForceMode2D.Impulse);


            //重さに関わらず、一定の力で押し出す
            other.rigidbody.velocity = (Vector2)(Quaternion.Euler(0, 0, angle) * myVel) * pushBoxPower;
        }

        #endregion

        #region interfaceの実装

        public void OnStateChangedToResult() {
            Rb2D.velocity = Vector2.zero;
            FollowFlag = false;

            _onBeAttacked.OnCompleted();
            _onDisappear.OnCompleted();

            _onDisappear.Dispose();
            _onBeAttacked.Dispose();
        }

        #endregion

        public void Appear(Vector3 spawnPos) {
            gameObject.SetActive(true);
            transform.position = spawnPos;
            _myState = DesireState.FollowingFairy;
            FollowFlag = true;
        }

        public void Disappear() {
            //ステータスを更新する
            _myState = DesireState.Freeze;

            //消失した通知を出す
            _onDisappear.OnNext(Unit.Default);

            //Desireを非表示にする
            gameObject.SetActive(false);
        }


        public void BeAttacked() {
            _onBeAttacked.OnNext(Unit.Default);
            //エフェクト処理を書く
            Disappear();
        }
    }
}