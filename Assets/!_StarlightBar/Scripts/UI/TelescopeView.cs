using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarlightBar.UI
{
    /// <summary>
    /// 망원경 오버레이와 포인터의 편집 가능한 참조입니다.
    /// </summary>
    public sealed class TelescopeView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image maskImage;
        [SerializeField] private TMP_Text helpText;
        [SerializeField] private RectTransform pointer;

        public GameObject Root => root;
        public CanvasGroup CanvasGroup => canvasGroup;
        public Image MaskImage => maskImage;
        public TMP_Text HelpText => helpText;
        public RectTransform Pointer => pointer;
    }
}
