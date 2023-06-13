using System;
using Cysharp.Threading.Tasks;
using MemoryTranser.Scripts.Game.Fairy;
using MemoryTranser.Scripts.Game.MemoryBox;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace MemoryTranser.Scripts.Game.Desire {
    public class DesireCore : MonoBehaviour {
        #region コンポーネントの定義

        [SerializeField] private BoxCollider2D boxCollider2D;
        [SerializeField] private Rigidbody2D rigidbody2D;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Transform fairyTransform;
        [SerializeField] private NavMeshAgent navMeshAgent;

        #endregion

        #region 変数の定義

        private DesireState _myState;
        private DesireType _myType;
        private DesireParameters _myParameters;
        private Vector3 _followPos;

        private bool _followFlag;

        #endregion

        #region プロパティーの定義

        public DesireState MyState {
            get => _myState;
            set => _myState = value;
        }

        public DesireType MyType {
            get => _myType;
            set => _myType = value;
        }

        public DesireParameters MyParameters {
            get => _myParameters;
            set => _myParameters = value;
        }

        public bool FollowFlag {
            get => _followFlag;
            set => _followFlag = value;
        }

        #endregion

        #region Unityから呼ばれる

        private void FixedUpdate() {
            if (FollowFlag) {
                //DesireとFairyの距離を算出する
                Vector2 Distance = fairyTransform.position - transform.position;
                //算出した距離を単位ベクトルする
                Distance = Distance.normalized;
                //移動処理
                rigidbody2D.velocity = Distance * MyParameters.FollowSpeed;
            }
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (other.gameObject.CompareTag("Fairy")) {
                other.GetComponent<FairyCore>().AttackedByDesire();
                AttackFairy();
            }
        }

        #endregion

        private void AttackFairy() {
            //Desireを非表示にする
            gameObject.SetActive(false);
            //追跡を停止する
            _followFlag = false;
            //ステータスを更新する
            _myState = DesireState.Freeze;
            //一時停止処理を実行させる
            RestartFllowing();
        }

        public void Eliminated() {
            //Desireを非表示にする
            gameObject.SetActive(false);
            //追跡を停止する
            _followFlag = false;
            //ステータスを更新する
            _myState = DesireState.Freeze;
            //一時停止処理を実行させる
            RestartFllowing();
        }

        /// <summary>
        /// Desireの初期化関数
        /// </summary>
        public void InitializeDesire() {
            MyParameters = new DesireParameters();
            MyState = DesireState.Freeze;

            MyParameters.FollowSpeed = FairyParameters.INITIAL_WALK_SPEED * 1.25f;
            MyParameters.ActionRecoveryTime = 20f;
        }

        public async void RestartFllowing() {
            //指定した時間待機する
            await UniTask.Delay(1000 * (int)MyParameters.ActionRecoveryTime);

            //Fairyの追跡を開始する
            gameObject.SetActive(true);
            _followFlag = true;
            _myState = DesireState.FollowingFairy;
        }
    }
}