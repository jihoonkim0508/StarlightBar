using StarlightBar.Content;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace StarlightBar.Gameplay
{
    /// <summary>
    /// 다른 증거 카드 위로 드래그해 관계를 연결할 수 있는 노트 카드입니다.
    /// </summary>
    public sealed class EvidenceCardView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text label;
        [SerializeField] private Image background;
        [SerializeField] private Color normalColor = new(0.18f, 0.22f, 0.34f, 0.96f);
        [SerializeField] private Color corruptedColor = new(0.26f, 0.08f, 0.30f, 0.96f);
        private RectTransform rect;
        private Canvas canvas;
        private Transform originalParent;
        private Vector2 originalPosition;
        private System.Action<EvidenceDefinition, EvidenceDefinition> linkRequested;

        public EvidenceDefinition Evidence { get; private set; }

        /// <summary>
        /// 증거 카드의 표시 정보와 연결 완료 콜백을 설정합니다.
        /// </summary>
        public void Initialize(
            EvidenceDefinition evidence, Canvas rootCanvas,
            System.Action<EvidenceDefinition, EvidenceDefinition> onLinkRequested)
        {
            Evidence = evidence;
            canvas = rootCanvas;
            linkRequested = onLinkRequested;
            rect = GetComponent<RectTransform>();
            background.color = evidence.corrupted ? corruptedColor : normalColor;
            label.text =
                $"{evidence.title}\n<size=75%>{ToKorean(evidence.category)}</size>";
        }

        /// <summary>
        /// 증거 연결을 시작하며 원래 카드 위치를 기억합니다.
        /// </summary>
        public void OnBeginDrag(PointerEventData eventData)
        {
            originalParent = transform.parent;
            originalPosition = rect.anchoredPosition;
            transform.SetParent(canvas.transform, true);
            transform.SetAsLastSibling();
            canvasGroup.blocksRaycasts = false;
        }

        /// <summary>
        /// 포인터 위치를 따라 증거 카드를 이동합니다.
        /// </summary>
        public void OnDrag(PointerEventData eventData)
        {
            rect.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }

        /// <summary>
        /// 겹친 증거와 연결을 시도한 뒤 카드를 원래 위치로 되돌립니다.
        /// </summary>
        public void OnEndDrag(PointerEventData eventData)
        {
            canvasGroup.blocksRaycasts = true;
            EvidenceCardView target = null;
            foreach (var result in eventData.hovered)
            {
                target = result.GetComponentInParent<EvidenceCardView>();
                if (target != null && target != this)
                    break;
            }

            if (target != null)
                linkRequested?.Invoke(Evidence, target.Evidence);

            transform.SetParent(originalParent, false);
            rect.anchoredPosition = originalPosition;
        }

        private static string ToKorean(EvidenceCategory category) => category switch
        {
            EvidenceCategory.Identity => "정체성",
            EvidenceCategory.Myth => "신화",
            EvidenceCategory.Emotion => "감정",
            EvidenceCategory.HumanLife => "인간 생활",
            EvidenceCategory.FoodReaction => "음식 반응",
            EvidenceCategory.InteriorReaction => "가구 반응",
            _ => "문서·소문"
        };
    }
}
