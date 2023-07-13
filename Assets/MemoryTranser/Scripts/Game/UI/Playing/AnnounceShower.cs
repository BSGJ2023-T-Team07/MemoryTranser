using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MemoryTranser.Scripts.Game.BrainEvent;
using MemoryTranser.Scripts.Game.Util;
using TMPro;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.UI.Playing {
    public class AnnounceShower : SingletonMonoBehaviour<AnnounceShower> {
        protected override bool DontDestroy => false;

        [SerializeField] private Transform announceTextBackGround;
        [SerializeField] private TextMeshProUGUI announcePrefab;

        [Header("アナウンスの流れる速さ(ピクセル/秒)")] [SerializeField]
        private float announceMoveSpeed;

        [Header("アナウンスの文章間の間隔(ピクセル)")] [SerializeField]
        private float announceTextUnitIntervalDistance;

        //アナウンスの文章1つが流れきる時間
        private float _announceUnitDuration;

        private Vector3 _defaultAnnounceTextLocalPosition;
        private Sequence _announceSequence;
        private Queue<TextMeshProUGUI> _announceQueue = new();

        protected override void Awake() {
            base.Awake();

            _announceSequence = DOTween.Sequence();

            _defaultAnnounceTextLocalPosition = Vector3.right * (Constant.SCREEN_WIDTH / 2f);
            _announceUnitDuration = Constant.SCREEN_WIDTH / announceMoveSpeed;

            //改行をしないようにする
            announcePrefab.enableWordWrapping = false;
        }

        public async void AddAnnounceText(string text, float durationSec) {
            var additionalTextUnit = GenerateNewTextUnit(text);

            var unitIntervalSec =
                (additionalTextUnit.preferredWidth + announceTextUnitIntervalDistance) / announceMoveSpeed;

            var textUnitCount = (int)(durationSec / unitIntervalSec);

            for (var i = 0; i < Mathf.Max(textUnitCount - 3, 1); i++) {
                GenerateNewTextUnit(text);

                await UniTask.Delay(
                    TimeSpan.FromSeconds(unitIntervalSec));
            }
        }

        private TextMeshProUGUI GenerateNewTextUnit(string text) {
            var newTextUnit = Instantiate(announcePrefab, announceTextBackGround);
            newTextUnit.transform.localPosition = _defaultAnnounceTextLocalPosition;
            newTextUnit.SetText(text);
            _announceQueue.Enqueue(newTextUnit);

            //動かして消えるところまで定義しておく
            newTextUnit.transform.DOLocalMoveX(-Constant.SCREEN_WIDTH - newTextUnit.preferredWidth,
                _announceUnitDuration).SetRelative().SetEase(Ease.Linear).OnComplete(() => {
                Destroy(newTextUnit.gameObject);
            }).SetLink(newTextUnit.gameObject);

            return newTextUnit;
        }
    }
}