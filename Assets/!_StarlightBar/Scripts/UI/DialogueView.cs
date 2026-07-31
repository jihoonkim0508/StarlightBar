using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarlightBar.UI
{
    /// <summary>
    /// 대화창, 선택지, 조작 버튼과 로그 화면의 편집 가능한 참조입니다.
    /// </summary>
    public sealed class DialogueView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Image portrait;
        [SerializeField] private TMP_Text speaker;
        [SerializeField] private TMP_Text expression;
        [SerializeField] private TMP_Text body;
        [SerializeField] private RectTransform choiceRoot;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button autoButton;
        [SerializeField] private TMP_Text autoButtonLabel;
        [SerializeField] private Button logButton;
        [SerializeField] private Button skipButton;
        [SerializeField] private GameObject logRoot;
        [SerializeField] private RectTransform logContent;
        [SerializeField] private Button logCloseButton;
        [SerializeField] private Image dialogueBox;
        [Header("초상 상태 표시")]
        [SerializeField] private Color stellaPortraitColor = new(0.72f, 0.78f, 0.95f, 0.92f);
        [SerializeField] private Color defaultPortraitColor = new(0.68f, 0.62f, 0.76f, 0.90f);
        [SerializeField, Range(0f, 1f)] private float guestPortraitAlpha = 0.92f;

        public GameObject Root => root;
        public Image Portrait => portrait;
        public TMP_Text Speaker => speaker;
        public TMP_Text Expression => expression;
        public TMP_Text Body => body;
        public RectTransform ChoiceRoot => choiceRoot;
        public Button NextButton => nextButton;
        public Button AutoButton => autoButton;
        public TMP_Text AutoButtonLabel => autoButtonLabel;
        public Button LogButton => logButton;
        public Button SkipButton => skipButton;
        public GameObject LogRoot => logRoot;
        public RectTransform LogContent => logContent;
        public Button LogCloseButton => logCloseButton;

        /// <summary>
        /// 대화 설정의 불투명도만 현재 프리팹 배경색에 적용합니다.
        /// </summary>
        public void ApplyDialogueOpacity(float opacity)
        {
            if (dialogueBox == null)
                return;
            var color = dialogueBox.color;
            color.a = Mathf.Clamp01(opacity);
            dialogueBox.color = color;
        }

        /// <summary>
        /// Inspector에서 지정한 기본 초상색과 챕터 팔레트를 조합합니다.
        /// </summary>
        public Color PortraitColor(bool stella, bool guest, Color guestTheme)
        {
            if (stella)
                return stellaPortraitColor;
            return guest
                ? new Color(guestTheme.r, guestTheme.g, guestTheme.b, guestPortraitAlpha)
                : defaultPortraitColor;
        }
    }
}
