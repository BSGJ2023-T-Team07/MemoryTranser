using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace MemoryTranser.Scripts.Title.UI.TitleLogo {
    public class TitleLogoAnimation : MonoBehaviour {
        [SerializeField] private Image titleImage;
        [SerializeField] private Gradient gradient;

        private void Start() {
            //画像が縮小・_gradientの色になる(周期11秒)
            titleImage.transform.DOScale(new Vector3(5.5f, 5.5f, 5.5f), 5.5f).SetEase(Ease.InOutQuad)
                .SetLoops(-1, LoopType.Yoyo);
            titleImage.DOGradientColor(gradient, 5.5f).SetEase(Ease.InOutQuad).SetLoops(-1, LoopType.Yoyo);
        }
    }
}