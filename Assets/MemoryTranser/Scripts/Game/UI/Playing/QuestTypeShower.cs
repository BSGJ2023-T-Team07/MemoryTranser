using System;
using System.Collections.Generic;
using DG.Tweening;
using MemoryTranser.Scripts.Game.GameManagers;
using MemoryTranser.Scripts.Game.MemoryBox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace MemoryTranser.Scripts.Game.UI.Playing {
    public class QuestTypeShower : MonoBehaviour {
        #region コンポーネントの定義

        [Header("クエストの文章の背景のx軸の余白")] [SerializeField]
        private float questTextPaddingX;

        [Header("Middleのオブジェクトの透明度")] [SerializeField]
        private float middleObjectAlpha;

        [Header("色付き文字が通常文字よりどれだけ大きいか")] [SerializeField]
        private float additionalFontSize;

        [Header("クエスト表示の遷移にかかる時間(秒)")] [SerializeField]
        private float questTransitionDuration;

        [Space] [SerializeField] private GameObject topStartQuestObject;
        [SerializeField] private Image topStartQuestImage;
        [SerializeField] private TextMeshProUGUI topStartQuestText;

        [Space] [SerializeField] private GameObject middleStartQuestObject;
        [SerializeField] private Image middleStartQuestImage;
        [SerializeField] private TextMeshProUGUI middleStartQuestText;

        [Space] [SerializeField] private GameObject downStartQuestObject;
        [SerializeField] private Image downStartQuestImage;
        [SerializeField] private TextMeshProUGUI downStartQuestText;

        [Space] [SerializeField] private Transform finishedQuestPosition;

        private Vector3 _topObjectPos;
        private Vector3 _middleObjectPos;
        private Vector3 _downObjectPos;
        private Vector3 _finishedQuestPos;

        private Vector3 _topObjectScale;
        private Vector3 _middleObjectScale;
        private Vector3 _downObjectScale;

        private float _defaultFontSize;

        private readonly Queue<Transform> _transforms = new();
        private readonly Queue<Image> _images = new();
        private readonly Queue<TextMeshProUGUI> _texts = new();

        private const int TOP = 2;
        private const int MIDDLE = 1;
        private const int DOWN = 0;

        #endregion

        #region 変数の定義

        //QuestTextの文章を設定
        [SerializeField] private QuestText[] mathTexts;
        [SerializeField] private QuestText[] japaneseTexts;
        [SerializeField] private QuestText[] englishTexts;
        [SerializeField] private QuestText[] lifeTexts;
        [SerializeField] private QuestText[] moralTexts;
        [SerializeField] private QuestText[] scienceTexts;
        [SerializeField] private QuestText[] triviaTexts;
        [SerializeField] private QuestText[] musicTexts;
        [SerializeField] private QuestText[] socialStudiesTexts;

        #endregion

        private void Awake() {
            //クエストの文章に空文字列を入れる
            middleStartQuestText.SetText("");
            downStartQuestText.SetText("");

            //初期位置を保存
            _topObjectPos = topStartQuestObject.transform.localPosition;
            _middleObjectPos = middleStartQuestObject.transform.localPosition;
            _downObjectPos = downStartQuestObject.transform.localPosition;
            _finishedQuestPos = finishedQuestPosition.localPosition;

            //初期サイズを保存
            _topObjectScale = topStartQuestObject.transform.localScale;
            _middleObjectScale = middleStartQuestObject.transform.localScale;
            _downObjectScale = downStartQuestObject.transform.localScale;

            //初期フォントサイズを保存
            _defaultFontSize = downStartQuestText.fontSize;

            //キューに入れるのはこの順番でなければならない
            _transforms.Enqueue(downStartQuestObject.transform);
            _transforms.Enqueue(middleStartQuestObject.transform);
            _transforms.Enqueue(topStartQuestObject.transform);

            _images.Enqueue(downStartQuestImage);
            _images.Enqueue(middleStartQuestImage);
            _images.Enqueue(topStartQuestImage);

            _texts.Enqueue(downStartQuestText);
            _texts.Enqueue(middleStartQuestText);
            _texts.Enqueue(topStartQuestText);
        }

        public void InitializeQuestText(BoxMemoryType currentMemoryType, BoxMemoryType nextMemoryType) {
            //クエストの文章を更新
            middleStartQuestText.SetText(GetRandomPaintedText(nextMemoryType));
            downStartQuestText.SetText(GetRandomPaintedText(currentMemoryType));

            //文章の長さによって背景画像のサイズを変更
            middleStartQuestImage.rectTransform.sizeDelta = new Vector2(
                middleStartQuestText.preferredWidth + questTextPaddingX * 2,
                middleStartQuestImage.rectTransform.sizeDelta.y);
            downStartQuestImage.rectTransform.sizeDelta = new Vector2(
                downStartQuestText.preferredWidth + questTextPaddingX * 2,
                downStartQuestImage.rectTransform.sizeDelta.y);


            //Middleは半透明にしておく
            var currentMiddleImageColor = middleStartQuestImage.color;
            var currentMiddleTextColor = middleStartQuestText.color;
            middleStartQuestImage.color = new Color(currentMiddleImageColor.r, currentMiddleImageColor.g,
                currentMiddleImageColor.b, middleObjectAlpha);
            middleStartQuestText.color = new Color(currentMiddleTextColor.r, currentMiddleTextColor.g,
                currentMiddleTextColor.b, middleObjectAlpha);
        }

        public void UpdateQuestText(BoxMemoryType afterNextMemoryType) {
            //インデックスから取得するためにArrayとして持っておく(参照は引き継いでいるので問題ない)
            var transformArray = _transforms.ToArray();
            var imageArray = _images.ToArray();
            var textArray = _texts.ToArray();

            //クエストの文章を更新
            textArray[TOP].SetText(GetRandomPaintedText(afterNextMemoryType));

            //文章の長さによって背景画像のサイズを変更
            imageArray[TOP].rectTransform.sizeDelta = new Vector2(textArray[TOP].preferredWidth + questTextPaddingX * 2,
                imageArray[TOP].rectTransform.sizeDelta.y);

            //TopのオブジェクトをMiddleに動かす
            transformArray[TOP].gameObject.SetActive(true);
            transformArray[TOP].DOLocalMove(_middleObjectPos, questTransitionDuration).SetEase(Ease.InOutQuad)
                .SetLink(gameObject);
            transformArray[TOP].DOScale(_middleObjectScale, questTransitionDuration).SetEase(Ease.InOutQuad)
                .SetLink(gameObject);
            imageArray[TOP].DOFade(middleObjectAlpha, questTransitionDuration).SetLink(gameObject);
            textArray[TOP].DOFade(middleObjectAlpha, questTransitionDuration).SetLink(gameObject);

            //MiddleのオブジェクトをDownに動かす
            transformArray[MIDDLE].DOLocalMove(_downObjectPos, questTransitionDuration).SetEase(Ease.InOutQuad)
                .SetLink(gameObject);
            transformArray[MIDDLE].DOScale(_downObjectScale, questTransitionDuration).SetEase(Ease.InOutQuad)
                .SetLink(gameObject);
            imageArray[MIDDLE].DOFade(1f, questTransitionDuration).SetLink(gameObject);
            textArray[MIDDLE].DOFade(1f, questTransitionDuration).SetLink(gameObject);

            //Downのオブジェクトを動かして表示を消す
            transformArray[DOWN].DOLocalMove(_finishedQuestPos, questTransitionDuration).SetEase(Ease.InOutQuad)
                .SetLink(gameObject);
            transformArray[DOWN].DOScale(0f, questTransitionDuration).SetEase(Ease.InOutQuad).SetLink(gameObject);
            imageArray[DOWN].DOFade(0f, questTransitionDuration).SetLink(gameObject);
            textArray[DOWN].DOFade(0f, questTransitionDuration)
                .OnComplete(() => {
                    //表示が消えきったら、Topに戻す
                    transformArray[DOWN].localPosition = _topObjectPos;
                    transformArray[DOWN].localScale = _topObjectScale;
                    transformArray[DOWN].gameObject.SetActive(false);
                }).SetLink(gameObject);

            //キューをずらす
            _transforms.Enqueue(_transforms.Dequeue());
            _images.Enqueue(_images.Dequeue());
            _texts.Enqueue(_texts.Dequeue());
        }

        private string GetRandomPaintedText(BoxMemoryType memoryType) {
            var randomQuestText = GetRandomQuestText(memoryType);
            var painted = randomQuestText.GetPaintedText(memoryType, _defaultFontSize, additionalFontSize);

            return painted;
        }

        private QuestText GetRandomQuestText(BoxMemoryType memoryType) {
            return memoryType switch {
                BoxMemoryType.Math => mathTexts[Random.Range(0, mathTexts.Length)],
                BoxMemoryType.Japanese => japaneseTexts[Random.Range(0, japaneseTexts.Length)],
                BoxMemoryType.English => englishTexts[Random.Range(0, englishTexts.Length)],
                BoxMemoryType.SocialStudies => socialStudiesTexts[Random.Range(0, socialStudiesTexts.Length)],
                BoxMemoryType.Science => scienceTexts[Random.Range(0, scienceTexts.Length)],
                // BoxMemoryType.Trivia => triviaTexts[Random.Range(0, triviaTexts.Length)],
                BoxMemoryType.Moral => moralTexts[Random.Range(0, moralTexts.Length)],
                // BoxMemoryType.Music => musicTexts[Random.Range(0, musicTexts.Length)],
                // BoxMemoryType.Life => lifeTexts[Random.Range(0, lifeTexts.Length)],
                _ => throw new ArgumentOutOfRangeException(nameof(memoryType), memoryType, null)
            };
        }
    }

    [Serializable]
    public class QuestText {
        [Tooltip("出だしの文")] public string startText;
        [Tooltip("科目に関連する文")] public string mainText;
        [Tooltip("締めの文")] public string finalText;

        /// <summary>
        /// 引数の記憶の色にテキストを塗る
        /// </summary>
        /// <param name="memoryType"></param>
        /// <param name="defaultFontSize">普通の文字の大きさ</param>
        /// <param name="additionalFontSize">色付き文字と普通の文字の大きさの差</param>
        /// <returns></returns>
        public string GetPaintedText(BoxMemoryType memoryType, float defaultFontSize, float additionalFontSize) {
            return
                $"{startText}<material={MemoryColorManager.GetTextMaterialName(memoryType)}><size={defaultFontSize + additionalFontSize}>{mainText}</size></material>{finalText}";
        }
    }
}