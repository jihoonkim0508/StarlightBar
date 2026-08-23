using TMPro;
using UnityEngine;

namespace StarlightBar.UI
{
    // 인벤토리/퀘스트/상점 패널이 공유하는 색상·폰트 토큰.
    // Bar 씬 기존 UI(패널 #292E4C, 코랄 #FF9191, Pretendard)와 톤을 맞추기 위한 값.
    // Font는 Inspector에서 Pretendard 에셋을 드래그로 연결해야 함(그래서 MonoBehaviour로 만듦).
    public sealed class UITheme : MonoBehaviour
    {
        [SerializeField] private TMP_FontAsset font;

        public TMP_FontAsset Font => font;

        public static readonly Color PanelColor = new Color(0.16f, 0.18f, 0.3f, 0.96f);
        public static readonly Color PanelDeepColor = new Color(0.1764706f, 0.1529412f, 0.2509804f, 0.9215686f);
        public static readonly Color TextPrimary = new Color(0.9607843f, 0.9529412f, 0.9294118f, 1f);
        public static readonly Color TextMuted = new Color(0.6117647f, 0.6392157f, 0.7686275f, 1f);
        public static readonly Color AccentCoral = new Color(1f, 0.5686275f, 0.5686275f, 1f);
        public static readonly Color AccentGold = new Color(0.9568627f, 0.8352941f, 0.5529412f, 1f);
    }
}
