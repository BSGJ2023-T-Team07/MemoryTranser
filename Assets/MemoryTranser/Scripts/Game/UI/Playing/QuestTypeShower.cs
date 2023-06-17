using MemoryTranser.Scripts.Game.MemoryBox;
using TMPro;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.UI {
    public class QuestTypeShower : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI questTypeText;

        public void SetQuestTypeText(BoxMemoryType boxMemoryType) {
            questTypeText.text = boxMemoryType.ToString();
        }
    }
}