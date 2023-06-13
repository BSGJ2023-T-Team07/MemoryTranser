using System;
using TMPro;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.Result {
    public class ResultManager : MonoBehaviour {
        #region プライベート

        private int score = 100;

        #endregion

        #region テキスト格納

        [SerializeField] private TextMeshProUGUI resultScore; // スコアを表示
        [SerializeField] private TextMeshProUGUI resultQuantityDelivered; // 納品した個数を表示
        [SerializeField] private TextMeshProUGUI resultClearedIssues; // クリアした問題数の表示
        [SerializeField] private TextMeshProUGUI resultMath; // クリアした数学の問題数の表示
        [SerializeField] private TextMeshProUGUI resultJapanese; // クリアした国語の問題数の表示
        [SerializeField] private TextMeshProUGUI resultEnglish; // クリアした英語の問題数の表示
        [SerializeField] private TextMeshProUGUI resultCommunity; // クリアした社会の問題数の表示
        [SerializeField] private TextMeshProUGUI resultScience; // クリアした理科の問題数の表示

        #endregion

        private void Start() {
            // テキストの表示(テストで変数はscoreしか宣言していない)
            // TODO:scoreの部分をデータを取得して変更すれば終わり
            resultScore.text = string.Format("score : {0:0}", score);
            resultQuantityDelivered.text = string.Format("QuantityDelivered : {0:0}", score);
            resultClearedIssues.text = string.Format("ClearedIssues : {0:0}", score);
            resultMath.text = string.Format("Math : {0:0}", score);
            resultJapanese.text = string.Format("Japanese : {0:0}", score);
            resultEnglish.text = string.Format("English : {0:0}", score);
            resultCommunity.text = string.Format("Community : {0:0}", score);
            resultScience.text = string.Format("Science : {0:0}", score);
        }
    }
}