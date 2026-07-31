using UnityEngine;
using UnityEngine.UI;

namespace StarlightBar.UI
{
    /// <summary>
    /// 접근성 화면 효과가 사용하는 씬 배치 오버레이를 보관합니다.
    /// </summary>
    public sealed class AccessibilityEffectsView : MonoBehaviour
    {
        [SerializeField] private Image flashOverlay;

        public Image FlashOverlay => flashOverlay;
    }
}
