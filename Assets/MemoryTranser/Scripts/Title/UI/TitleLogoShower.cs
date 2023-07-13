using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace MemoryTranser.Scripts.Title.UI {
    public class TitleLogoAnimation : MonoBehaviour {
        [SerializeField] private Image titleLogoImage;

        [SerializeField] private float titleLogoScaleMultiplier;
        [SerializeField] private float animationUnitDurationSec;

        private void Start() {
            titleLogoImage.transform.DOScale(titleLogoImage.transform.localScale * titleLogoScaleMultiplier,
                    animationUnitDurationSec / 2f)
                .SetEase(Ease.InOutQuad)
                .SetLoops(-1, LoopType.Yoyo)
                .SetLink(gameObject);
        }
    }
}