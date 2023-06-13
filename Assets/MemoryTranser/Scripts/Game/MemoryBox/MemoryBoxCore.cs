using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using MemoryTranser.Scripts.Game.Desire;
using MemoryTranser.Scripts.Game.GameManagers;
using MemoryTranser.Scripts.Game.Util;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace MemoryTranser.Scripts.Game.MemoryBox {
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class MemoryBoxCore : MonoBehaviour {
        #region コンポーネントの定義

        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private BoxCollider2D boxCollider2D;
        [SerializeField] private Rigidbody2D rigidbody2D;
        private MemoryBoxManager memoryBoxManager;

        #endregion

        #region 変数の定義

        private BoxMemoryType _boxMemoryType = new();
        private MemoryBoxState _myState = new();
        private float _weight;
        private int _boxId;


        private float _diff;

        #endregion


        #region プロパティーの定義

        public SpriteRenderer SpRr {
            get => spriteRenderer;
            set => spriteRenderer = value;
        }

        public BoxCollider2D Bc2D {
            get => boxCollider2D;
            set => boxCollider2D = value;
        }

        public BoxMemoryType BoxMemoryType {
            get => _boxMemoryType;
            set => _boxMemoryType = value;
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

        private void Awake() {
            memoryBoxManager = GameFlowManager.I.gameObject.GetComponent<MemoryBoxManager>();
        }

        private void Update() {
            if (_myState == MemoryBoxState.Held) transform.position = transform.parent.position + Vector3.up * _diff;

            if (_myState == MemoryBoxState.Thrown && rigidbody2D.velocity.magnitude < Constant.DELTA)
                _myState = MemoryBoxState.PlacedOnLevel;
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (_myState == MemoryBoxState.Thrown && other.gameObject.layer == LayerMask.NameToLayer("Desire"))
                //DesireにThrow状態のMemoryBoxが当たった時に以下の関数を呼び出す
                AttackDesire(other.gameObject.GetComponent<DesireCore>());
        }

        #endregion

        private void AttackDesire(DesireCore desire) {
            //Desireの命中時の処理を実行する
            desire.Eliminated();
        }

        public void Held(Transform holderTransform) {
            _myState = MemoryBoxState.Held;
            transform.SetParent(holderTransform);
            boxCollider2D.enabled = false;
            rigidbody2D.velocity = Vector2.zero;
        }

        public async void Thrown(float throwPower, Vector2 throwDirection) {
            _myState = MemoryBoxState.Thrown;
            transform.SetParent(null);
            rigidbody2D.velocity = throwDirection * throwPower / Weight;

            //投げてから少しは当たり判定が無いようにする
            //await UniTask.Delay(1000 * Convert.ToInt32(throwPower / Weight));

            boxCollider2D.enabled = true;
        }

        public void Put() {
            _myState = MemoryBoxState.PlacedOnLevel;
            boxCollider2D.enabled = true;
        }

        public async void Disappear() {
            gameObject.SetActive(false);

            await UniTask.Delay(5000);

            Revive();
        }

        public void Revive() {
            memoryBoxManager.GenerateRandomMemoryBox(this);
            transform.position = memoryBoxManager.GetRandomSpawnPosition();
            gameObject.SetActive(true);
        }


        public void SetDiff() {
            _diff = boxCollider2D.size.y * transform.localScale.y;
        }
    }
}