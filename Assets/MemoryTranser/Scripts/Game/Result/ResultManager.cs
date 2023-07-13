using DG.Tweening;
using MemoryTranser.Scripts.Game.Fairy;
using MemoryTranser.Scripts.Game.GameManagers;
using MemoryTranser.Scripts.Game.Phase;
using MemoryTranser.Scripts.Game.UI.Result;
using MemoryTranser.Scripts.SceneTransition;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace MemoryTranser.Scripts.Game.Result {
    public class ResultManager : MonoBehaviour, IOnStateChangedToResult {
        #region コンポーネントの定義

        [SerializeField] private ResultShower resultShower;
        [SerializeField] private PhaseManager phaseManager;
        [SerializeField] private FairyCore fairyCore;

        private readonly InputAction _pressAnyKeyAction =
            new(type: InputActionType.PassThrough, binding: "*/<Button>", interactions: "Press");

        private void OnDisable() {
            _pressAnyKeyAction.Disable();
        }

        #endregion

        #region 変数の定義

        private int _totalScore;
        private int _reachedPhaseCount;
        private int _reachedMaxComboCount;

        #endregion

        private void Update() {
            if (resultShower.IsAnimationCompleted && _pressAnyKeyAction.triggered) {
                SceneTransitionEffecter.I.PlayFadeEffect(DOTween.Sequence().OnPlay(() => {
                    SceneManager.LoadScene("MemoryTranser/Scenes/TitleScene");
                }));
            }
        }


        public void OnStateChangedToResult() {
            var phaseInfo = phaseManager.GetResultInformation();
            _totalScore = phaseInfo.Item1;
            _reachedPhaseCount = phaseInfo.Item2;

            _reachedMaxComboCount = fairyCore.GetResultInformation();

            resultShower.ShowResult(_totalScore, _reachedPhaseCount, _reachedMaxComboCount);
            _pressAnyKeyAction.Enable();
        }
    }
}