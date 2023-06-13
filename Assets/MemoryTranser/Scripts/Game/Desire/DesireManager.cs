using UnityEngine;

namespace MemoryTranser.Scripts.Game.Desire {
    public class DesireManager : MonoBehaviour {
        #region 変数の定義

        private bool _spawnFlag;

        #endregion

        #region プロパティーの定義

        public bool Spawnflag {
            get => _spawnFlag;
            set => _spawnFlag = value;
        }

        #endregion

        private void SpawnDesire() { }
    }
}