using TMPro;
using UnityEngine;

namespace StarlightBar.UI
{
    /// <summary>
    /// 폰트가 비어 있거나 손상된 동적 텍스트에만 프로젝트의 한글 폰트를 보충합니다.
    /// </summary>
    public static class KoreanFontRuntime
    {
        private const string KoreanFontResource = "StarlightBar/Fonts/NotoSansKR-Dynamic";
        private static TMP_FontAsset koreanFont;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => koreanFont = null;

        /// <summary>
        /// Inspector에서 정상 폰트를 지정한 텍스트는 그대로 두고, 폰트가 없는 동적 텍스트만 복구합니다.
        /// </summary>
        public static void ApplyFont(TMP_Text text)
        {
            if (text == null || IsUsable(text.font))
                return;
            koreanFont ??= Resources.Load<TMP_FontAsset>(KoreanFontResource);
            if (!IsUsable(koreanFont))
            {
                Debug.LogError("NotoSansKR-Dynamic 폰트 자산을 찾을 수 없습니다.");
                return;
            }
            text.font = koreanFont;
            text.fontSharedMaterial = koreanFont.material;
            text.SetAllDirty();
        }

        private static bool IsUsable(TMP_FontAsset fontAsset) =>
            fontAsset != null && fontAsset.material != null;
    }
}
