using MemoryTranser.Scripts.Game.MemoryBox;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.Phase {
    public class PhaseCore : ScriptableObject {
        #region 変数の定義

        private BoxMemoryType _questType = new();
        private int _score;

        #endregion


        #region プロパティーの定義

        public BoxMemoryType QuestType {
            get => _questType;
            set => _questType = value;
        }

        public int Score {
            get => _score;
            set => _score = value;
        }

        #endregion
    }
}