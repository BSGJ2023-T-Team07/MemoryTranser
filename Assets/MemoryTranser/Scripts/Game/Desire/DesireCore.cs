using System;
using Cysharp.Threading.Tasks;
using MemoryTranser.Scripts.Game.Fairy;
using MemoryTranser.Scripts.Game.GameManagers;
using UnityEngine;
using UnityEngine.AI;
using UniRx;

namespace MemoryTranser.Scripts.Game.Desire {
    public class DesireCore : MonoBehaviour, IOnStateChangedToInitializing, IOnStateChangedToPlaying {
        #region コンポーネントの定義

        [SerializeField] private BoxCollider2D boxCollider2D;
        [SerializeField] private Rigidbody2D rb2D;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Transform fairyTransform;
        [SerializeField] private NavMeshAgent navMeshAgent;

        #endregion

        #region 変数の定義

        private DesireState _myState;
        private DesireType _myType;
        private DesireParameters _myParameters;
        private Vector3 _followPos;

        #endregion

        #region eventの定義

        private readonly Subject<Unit> _onAttacked = new();
        public IObservable<Unit> OnAttacked => _onAttacked;

        #endregion

        #region プロパティーの定義

        private bool FollowFlag => _myState == DesireState.FollowingFairy;

        public Rigidbody2D Rb2D {
            get {
                if (!rb2D) rb2D = GetComponent<Rigidbody2D>();
                return rb2D;
            }
            set => rb2D = value;
        }

        #endregion

        #region Unityから呼ばれる

        private void FixedUpdate() {
            if (FollowFlag) {
                //DesireとFairyの距離を算出する
                Vector2 direction = (fairyTransform.position - transform.position).normalized;

                //移動処理
                Rb2D.velocity = direction * _myParameters.FollowSpeed;
            }
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.gameObject.CompareTag("Fairy")) {
                other.GetComponent<FairyCore>().BeAttackedByDesire();
                Disappear();
            }
        }

        #endregion

        #region interfaceの実装

        public void OnStateChangedToInitializing() {
            InitializeDesire();
        }

        public void OnStateChangedToPlaying() {
            RestartFollowing();
        }

        #endregion

        private void Disappear() {
            //Desireを非表示にする
            gameObject.SetActive(false);

            //ステータスを更新する
            _myState = DesireState.Freeze;

            //一時停止処理を実行させる
            RestartFollowing();
        }

        public void BeEliminated() {
            //スコアを加算する通知をだす
            _onAttacked.OnNext(Unit.Default);

            //Desireを非表示にする
            gameObject.SetActive(false);

            //ステータスを更新する
            _myState = DesireState.Freeze;

            //一時停止処理を実行させる
            RestartFollowing();
        }

        /// <summary>
        /// Desireの初期化関数
        /// </summary>
        private void InitializeDesire() {
            _myParameters = new DesireParameters();
            _myState = DesireState.Freeze;

            _myParameters.FollowSpeed = FairyParameters.INITIAL_WALK_SPEED * 0.8f;
            _myParameters.ActionRecoveryTime = 20f;
        }

        private async void RestartFollowing() {
            //指定した時間待機する
            await UniTask.Delay(TimeSpan.FromSeconds(_myParameters.ActionRecoveryTime));

            //Fairyの追跡を開始する
            gameObject.SetActive(true);
            _myState = DesireState.FollowingFairy;
        }

        private void OnDestroy() {
            _onAttacked.OnCompleted();
            _onAttacked.Dispose();
        }
    }
}