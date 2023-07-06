using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using System.Text;
using DG.Tweening;
using MemoryTranser.Scripts.Game.MemoryBox;
using MemoryTranser.Scripts.Game.Util;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace MemoryTranser.Scripts.Game.UI.Playing
{
    public class QuestTypeShower : MonoBehaviour
    {
        #region コンポーネントの定義

        [SerializeField] private TextMeshProUGUI questTypeText;
        [SerializeField] private Image questTypeBack;

        #endregion

        public void ChangeToOld(string questTxtPainted)
        {
            //ここにアニメーション記述
            questTypeText.text = questTxtPainted + "昔";
        }

        public void ChangeToNow(string questTxtPainted)
        {
            //ここにアニメーション記述
            questTypeText.text = questTxtPainted + "今";
        }

        public void ChangeToNext(string questTxtPainted)
        {
            //ここにアニメーション記述
            questTypeText.text = questTxtPainted + "次";
        }
    }

    [Serializable]
    public class QuestText
    {
        [Tooltip("出だしの文")] public string startTxt;
        [Tooltip("科目に関連する文")] public string mainTxt;
        [Tooltip("締めの文")] public string finalTxt;
    }
}