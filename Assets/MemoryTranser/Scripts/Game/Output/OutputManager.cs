using System;
using Cysharp.Threading.Tasks;
using MemoryTranser.Scripts.Game.BrainEvent;
using MemoryTranser.Scripts.Game.Concentration;
using MemoryTranser.Scripts.Game.Fairy;
using MemoryTranser.Scripts.Game.GameManagers;
using MemoryTranser.Scripts.Game.MemoryBox;
using MemoryTranser.Scripts.Game.Phase;
using MemoryTranser.Scripts.Game.Sound;
using MemoryTranser.Scripts.Game.UI.Playing;
using UniRx;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.Output {
    public class OutputManager : MonoBehaviour, IOnGameAwake {
        #region コンポーネントの定義

        [SerializeField] private PhaseManager phaseManager;
        [SerializeField] private BrainEventManager brainEventManager;
        [SerializeField] private FairyCore fairyCore;
        [SerializeField] private ConcentrationManager concentrationManager;
        [SerializeField] private MemoryBoxManager memoryBoxManager;
        [SerializeField] private BoxCollider2D areaCollider;

        #endregion

        #region 変数の定義

        [Header("納品時に記憶が飛んでいく速さ")] [SerializeField]
        private float boxOutputSpeed;

        [Header("記憶が飛んで行ってからDisappearするまでの時間(秒)")] [SerializeField]
        private float boxDisappearTime;

        [Space] [Header("ド忘れイベント中の点数倍率")] [SerializeField]
        private float blindEventMultiplier;

        [Header("煩悩大量発生イベント中の点数倍率")] [SerializeField]
        private float desireOutbreakEventMultiplier;

        [Header("操作逆転イベント中の点数倍率")] [SerializeField]
        private float invertControlEventMultiplier;

        [Header("勉強の成果イベント中の点数倍率")] [SerializeField]
        private float studyEventMultiplier;

        [Header("フィーバータイムイベント中の点数倍率")] [SerializeField]
        private float feverTimeEventMultiplier;


        private float _scoreMultiplierByEvent;

        #endregion

        #region eventの定義

        private Subject<Unit> _onOutput = new();
        public IObservable<Unit> OnOutput => _onOutput;

        #endregion

        #region Unityから呼ばれる

        private void OnTriggerEnter2D(Collider2D other) {
            var memoryBoxCore = other.GetComponent<MemoryBoxCore>();
            if (!memoryBoxCore) {
                return;
            }

            memoryBoxManager.AddOutputableId(memoryBoxCore.BoxId);
        }

        private void OnTriggerExit2D(Collider2D other) {
            var memoryBoxCore = other.GetComponent<MemoryBoxCore>();
            if (!memoryBoxCore) {
                return;
            }

            memoryBoxManager.RemoveOutputableId(memoryBoxCore.BoxId);
        }

        #endregion


        private void OutputBoxes(MemoryBoxCore[] boxes) {
            _onOutput.OnNext(Unit.Default);

            //phaseManagerとmemoryBoxManagerを使って点数の情報を計算
            var (score, trueCount, falseCount) =
                phaseManager.CalculateScoreInformation(boxes);

            //イベントの倍率をかける
            score = Mathf.FloorToInt(score * _scoreMultiplierByEvent);

            //MemoryBoxをDisappearさせる
            foreach (var box in boxes) {
                MakeBoxFry(box);
            }

            //点数の情報を元にスコア加算
            phaseManager.AddCurrentScore(score);

            //点数の情報を元にコンボ加算
            if (falseCount > 0) {
                //1つでも誤答してたらコンボリセット
                fairyCore.CurrentComboCount = 0;
            }
            else {
                fairyCore.CurrentComboCount += trueCount;
            }

            //点数の情報を元に集中力アップ
            concentrationManager.AddConcentration(score);

            switch (score) {
                //点数によって高秀の表示を変える
                case > 0:
                    TakahideShower.I.ChangeTakahideImage(TakahideState.Inspiration);
                    SeManager.I.Play(SEs.OutputTrue);
                    break;
                case < 0:
                    TakahideShower.I.ChangeTakahideImage(TakahideState.Sad);
                    SeManager.I.Play(SEs.OutputFalse);
                    break;
                case 0:
                    if (trueCount == 0) {
                        break;
                    }

                    TakahideShower.I.ChangeTakahideImage(TakahideState.Inspiration);
                    //TODO:ちゃんと記憶を設置してるのに点数が0だったときのSEを用意したい
                    break;
            }
        }

        private async void MakeBoxFry(MemoryBoxCore box) {
            box.isOutput = true;
            box.Cc2D.enabled = false;
            box.MyState = MemoryBoxState.Flying;
            box.Rb2D.velocity = Vector2.up * boxOutputSpeed;

            await UniTask.Delay(TimeSpan.FromSeconds(boxDisappearTime));

            box.Disappear();
        }

        private void OnDestroy() {
            _onOutput.OnCompleted();
            _onOutput.Dispose();
        }

        #region interfaceの実装

        public void OnGameAwake() {
            fairyCore.OnOutputInput.Subscribe(_ => { OutputBoxes(memoryBoxManager.GetOutputableBoxes()); });
            brainEventManager.OnBrainEventTransition.Subscribe(brainEvent => {
                _scoreMultiplierByEvent = brainEvent switch {
                    BrainEventType.None => 1f,
                    BrainEventType.Blind => blindEventMultiplier,
                    BrainEventType.DesireOutbreak => desireOutbreakEventMultiplier,
                    BrainEventType.InvertControl => invertControlEventMultiplier,
                    BrainEventType.AchievementOfStudy => studyEventMultiplier,
                    BrainEventType.FeverTime => feverTimeEventMultiplier,
                    _ => throw new ArgumentOutOfRangeException(nameof(brainEvent), brainEvent, null)
                };
            });
        }

        #endregion
    }
}