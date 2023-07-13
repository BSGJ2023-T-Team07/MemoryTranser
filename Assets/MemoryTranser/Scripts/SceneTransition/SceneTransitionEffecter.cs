using DG.Tweening;
using MemoryTranser.Scripts.Game.Util;
using UnityEngine;
using UnityEngine.UI;

namespace MemoryTranser.Scripts.SceneTransition {
    public class SceneTransitionEffecter : SingletonMonoBehaviour<SceneTransitionEffecter> {
        protected override bool DontDestroy => true;

        [SerializeField] private GameObject fadeCanvasPrefab;

        [Header("フェードインにかかる時間(秒)")] [SerializeField]
        private float fadeInDurationSec;

        [Header("フェードインとフェードアウトの間の待機時間(秒)")] [SerializeField]
        private float middleDurationSec;

        [Header("フェードアウトにかかる時間(秒)")] [SerializeField]
        private float fadeOutDurationSec;

        private static SpriteRenderer _fadeImage;

        protected override void Awake() {
            base.Awake();

            _fadeImage = Instantiate(fadeCanvasPrefab).GetComponent<SpriteRenderer>();
            DontDestroyOnLoad(_fadeImage);
            var currentColor = _fadeImage.color;
            _fadeImage.color = new Color(currentColor.r, currentColor.g, currentColor.b, 0f);
            _fadeImage.gameObject.SetActive(false);
        }

        public Sequence PlayFadeEffect(Sequence middleSequence) {
            _fadeImage.gameObject.SetActive(true);
            var sequence = DOTween.Sequence();
            sequence.OnStart(() => { Time.timeScale = 0f; })
                .Append(_fadeImage.DOFade(1f, fadeInDurationSec)).SetUpdate(true)
                .Append(middleSequence).SetUpdate(true)
                .AppendInterval(middleDurationSec).SetUpdate(true)
                .Append(_fadeImage.DOFade(0f, fadeOutDurationSec)).SetUpdate(true)
                .OnComplete(
                    () => {
                        _fadeImage.gameObject.SetActive(false);
                        Time.timeScale = 1f;
                    })
                .SetLink(gameObject);

            sequence.Play();

            return sequence;
        }
    }
}