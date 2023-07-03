using System;
using MemoryTranser.Scripts.Game.Concentration;
using MemoryTranser.Scripts.Game.Fairy;
using MemoryTranser.Scripts.Game.GameManagers;
using MemoryTranser.Scripts.Game.MemoryBox;
using MemoryTranser.Scripts.Game.Phase;
using MemoryTranser.Scripts.Game.UI.Playing;
using UniRx;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.OutputArea {
    [RequireComponent(typeof(BoxCollider2D))]
    public class OutputManager : MonoBehaviour, IOnGameAwake {
        #region コンポーネントの定義

        [SerializeField] private PhaseManager phaseManager;
        [SerializeField] private FairyCore fairyCore;
        [SerializeField] private ConcentrationManager concentrationManager;
        [SerializeField] private MemoryBoxManager memoryBoxManager;
        [SerializeField] private BoxCollider2D areaCollider;

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


        private void OutputBoxes() {
            _onOutput.OnNext(Unit.Default);

            //phaseManagerとmemoryBoxManagerを使って点数の情報を計算
            var (score, trueCount, falseCount) =
                phaseManager.CalculateScoreInformation(memoryBoxManager.GetOutputableBoxes());

            //MemoryBoxをDisappearさせる
            foreach (var box in memoryBoxManager.GetOutputableBoxes()) {
                box.Disappear();
            }

            //点数の情報を元にスコア加算
            phaseManager.AddCurrentScore(score);
            phaseManager.UpdatePhaseText();

            //点数の情報を元にコンボ加算
            if (falseCount > 0) {
                //1つでも誤答してたらコンボリセット
                fairyCore.ComboCount = 0;
            }
            else {
                fairyCore.ComboCount += trueCount;
            }

            //点数の情報を元に集中力アップ
            concentrationManager.AddConcentration(score);

            //点数によって高秀の表示を変える
            TakahideShower.I.ChangeTakahideImage(score > 0 ? TakahideState.Inspiration : TakahideState.Sad);
        }

        private void OnDestroy() {
            _onOutput.OnCompleted();
            _onOutput.Dispose();
        }

        #region interfaceの実装

        public void OnGameAwake() {
            fairyCore.OnOutputInput.Subscribe(_ => { OutputBoxes(); });
        }

        #endregion
    }
}