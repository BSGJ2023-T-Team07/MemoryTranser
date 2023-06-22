using System.Collections;
using System.Collections.Generic;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Color = System.Drawing.Color;

public class TitleAnimation : MonoBehaviour
{
    [SerializeField] private Image _titleImage;
    [SerializeField] private Gradient _gradient;

    // Start is called before the first frame update
    private void Start()
    {
        //画像が縮小・_gradientの色になる(周期11秒)
        _titleImage.transform.DOScale(new Vector3(5.5f, 5.5f, 5.5f), 5.5f).SetEase(Ease.InOutQuad)
            .SetLoops(-1, LoopType.Yoyo);
        _titleImage.DOGradientColor(_gradient, 5.5f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
    }

    // Update is called once per frame
    private void Update()
    {
    }
}