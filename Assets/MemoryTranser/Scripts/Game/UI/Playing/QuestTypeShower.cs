using System;
using MemoryTranser.Scripts.Game.MemoryBox;
using MemoryTranser.Scripts.Game.Util;
using TMPro;
using UnityEngine;

namespace MemoryTranser.Scripts.Game.UI.Playing
{
    public class QuestTypeShower : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI questTypeText;

        public void SetQuestTypeText(BoxMemoryType boxMemoryType)
        {
            questTypeText.text = $"{questTypeText.text}：{boxMemoryType.ToJapanese()}";
        }
    }
}