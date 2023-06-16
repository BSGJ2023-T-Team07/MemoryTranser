using System;
using MemoryTranser.Scripts.Game.Concentration;
using MemoryTranser.Scripts.Game.Fairy;
using MemoryTranser.Scripts.Game.GameManagers;
using MemoryTranser.Scripts.Game.MemoryBox;
using MemoryTranser.Scripts.Game.Phase;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.OutputArea {
    [RequireComponent(typeof(BoxCollider2D))]
    public class OutputManager : MonoBehaviour {
        #region コンポーネントの定義

        [SerializeField] private PhaseManager phaseManager;
        [SerializeField] private FairyCore fairyCore;
        [SerializeField] private ConcentrationManager concentrationManager;
        [SerializeField] private MemoryBoxManager memoryBoxManager;
        [SerializeField] private BoxCollider2D areaCollider;

        #endregion

        private void OnTriggerEnter2D(Collider2D other) {
            var memoryBoxCore = other.GetComponent<MemoryBoxCore>();
            if (!memoryBoxCore) return;

            memoryBoxManager.AddOutputableId(memoryBoxCore.BoxId);
        }

        private void OnTriggerExit2D(Collider2D other) {
            var memoryBoxCore = other.GetComponent<MemoryBoxCore>();
            if (!memoryBoxCore) return;

            memoryBoxManager.RemoveOutputableId(memoryBoxCore.BoxId);
        }


        public void OutputBoxes() {
            //正誤表の作成
            var outputableBoxes = memoryBoxManager.GetOutputableBoxes();
            var errataList = new bool[outputableBoxes.Length];
            var trueCount = 0;
            for (var i = 0; i < outputableBoxes.Length; i++) {
                if (outputableBoxes[i].BoxMemoryType == phaseManager.GetCurrentQuestType()) {
                    errataList[i] = true;
                    trueCount++;
                }
            }

            //MemoryBoxをDisappearさせる
            foreach (var box in memoryBoxManager.GetOutputableBoxes()) {
                box.Disappear();
            }

            memoryBoxManager.GetOutputableBoxes();

            //正誤表を元にスコア加算
            phaseManager.CalculateCurrentScore(errataList);
            phaseManager.UpdatePhaseText();

            //正誤表を元にコンボ加算
            fairyCore.ComboCount += trueCount;

            //正誤表を元に集中力アップ
            concentrationManager.AddConcentration(trueCount * 10f);
        }
    }
}