using MemoryTranser.Scripts.Game.Fairy;
using MemoryTranser.Scripts.Game.GameManagers;
using MemoryTranser.Scripts.Game.Phase;
using MemoryTranser.Scripts.Game.Sound;
using MemoryTranser.Scripts.Game.UI.Result;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.Result {
    public class ResultManager : MonoBehaviour, IOnStateChangedToResult {
        #region コンポーネントの定義

        [SerializeField] private ResultShower resultShower;
        [SerializeField] private PhaseManager phaseManager;
        [SerializeField] private FairyCore fairyCore;

        #endregion

        #region 変数の定義

        private int _totalScore;
        private int _reachedPhaseCount;
        private int _reachedMaxComboCount;

        #endregion


        public void OnStateChangedToResult() {
            BgmManager.I.StopPlayingBgm();
            var phaseInfo = phaseManager.GetResultInformation();
            _totalScore = phaseInfo.Item1;
            _reachedPhaseCount = phaseInfo.Item2;

            _reachedMaxComboCount = fairyCore.GetResultInformation();

            resultShower.ShowResult(_totalScore, _reachedPhaseCount, _reachedMaxComboCount);
        }
    }
}