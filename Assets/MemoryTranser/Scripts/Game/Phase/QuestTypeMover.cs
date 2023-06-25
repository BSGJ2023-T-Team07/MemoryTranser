using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestTypeMover : MonoBehaviour
{
    #region コンポーネントの定義

    [SerializeField] private Image _currentQuestTypeBack;
    [SerializeField] private TextMeshProUGUI _currentQuestTypeText;
    [SerializeField] private Image _nextQuestTypeBack;
    [SerializeField] private TextMeshProUGUI _nextQuestTypeText;

    #endregion

    #region 変数の定義

    [Header("CurrnentQuestの表示時間")] public float _currentQuestDuration = 15f;
    [Header("NextQuestの表示時間")] public float _nextQuestDuration = 15f;

    #endregion

    // Start is called before the first frame update
    private void Start()
    {
        var _sequence = DOTween.Sequence();
        _sequence.Join(_currentQuestTypeBack.DOFade(0f, _currentQuestDuration).SetEase(Ease.OutSine));
        _sequence.Join(_currentQuestTypeText.DOFade(0f, _currentQuestDuration).SetEase(Ease.OutSine));
        //_sequence.AppendInterval(2.5f);
        _sequence.Insert(2.5f, _nextQuestTypeBack.DOFade(1f, _nextQuestDuration).SetEase(Ease.OutSine));
        _sequence.Insert(2.5f, _nextQuestTypeText.DOFade(1f, _nextQuestDuration).SetEase(Ease.OutSine));
    }

    // Update is called once per frame
    private void Update()
    {
    }
}