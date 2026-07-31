using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarlightBar.UI
{
    /// <summary>
    /// 설정과 접근성 화면의 편집 가능한 고정 오브젝트 참조입니다.
    /// </summary>
    public sealed class SettingsView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private ScrollRect panel;
        [SerializeField] private RectTransform content;
        [SerializeField] private Image brightnessOverlay;

        public GameObject Root => root;
        public ScrollRect Panel => panel;
        public RectTransform Content => content;
        public Image BrightnessOverlay => brightnessOverlay;
    }
}
