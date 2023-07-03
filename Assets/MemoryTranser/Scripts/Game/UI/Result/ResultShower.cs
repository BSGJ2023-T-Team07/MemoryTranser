using MemoryTranser.Scripts.Game.GameManagers;
using TMPro;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.UI.Result {
    public class ResultShower : MonoBehaviour, IOnGameAwake {
        [SerializeField] private TextMeshProUGUI totalScoreText;
        [SerializeField] private TextMeshProUGUI reachedPhaseCountText;
        [SerializeField] private TextMeshProUGUI reachedMaxComboCountText;

        [SerializeField] private GameObject pauseBackGround;
        [SerializeField] private GameObject resultLayer;
        [SerializeField] private GameObject resultPaper;

        public void OnGameAwake() {
            pauseBackGround.SetActive(false);
            resultLayer.SetActive(false);
        }

        public void ShowResult(int totalScore, int reachedPhaseCount, int reachedMaxComboCount) {
            totalScoreText.text = $"総合点数: {totalScore}";
            reachedPhaseCountText.text = $"到達したフェイズ: {reachedPhaseCount}";
            reachedMaxComboCountText.text = $"最大コンボ数: {reachedMaxComboCount}";

            pauseBackGround.SetActive(true);
            resultLayer.SetActive(true);
            UnityEngine.Debug.Log("Resultを表示しました");
        }
    }
}