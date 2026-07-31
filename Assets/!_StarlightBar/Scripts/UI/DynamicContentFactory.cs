using StarlightBar.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarlightBar.UI
{
    /// <summary>
    /// 콘텐츠 개수에 따라 달라지는 행과 버튼만 편집 가능한 공용 프리팹에서 복제합니다.
    /// </summary>
    internal static class DynamicContentFactory
    {
        /// <summary>
        /// 공용 패널 프리팹의 Inspector 값을 유지한 채 동적 콘텐츠 그룹을 복제합니다.
        /// </summary>
        public static RectTransform CreateContentGroup(Transform parent, string name)
        {
            var panel = Object.Instantiate(RequireLibrary().panelPrefab, parent, false);
            panel.name = name;
            return panel.GetComponent<ScrollRect>().content;
        }

        /// <summary>
        /// 공용 텍스트 프리팹을 복제하고 표시 문구만 설정합니다.
        /// </summary>
        public static TMP_Text CreateText(Transform parent, string value, float legacySize = 24f)
        {
            var instance = Object.Instantiate(RequireLibrary().textPrefab, parent, false);
            instance.name = "Text";
            var text = instance.GetComponent<TextMeshProUGUI>();
            text.text = value;
            KoreanFontRuntime.ApplyFont(text);
            instance.GetComponent<RuntimeTextScale>().Initialize(text, text.fontSize);
            return text;
        }

        /// <summary>
        /// 공용 버튼 프리팹을 복제하고 문구와 클릭 동작만 연결합니다.
        /// </summary>
        public static Button CreateButton(
            Transform parent, string label, UnityEngine.Events.UnityAction action)
        {
            var instance = Object.Instantiate(RequireLibrary().buttonPrefab, parent, false);
            instance.name = $"Button_{label}";
            var button = instance.GetComponent<Button>();
            button.onClick.AddListener(action);
            button.onClick.AddListener(RuntimeAudioService.PlayUiConfirm);
            var text = instance.GetComponentInChildren<TMP_Text>(true);
            text.text = label;
            KoreanFontRuntime.ApplyFont(text);
            return button;
        }

        private static RuntimePrefabLibrary RequireLibrary()
        {
            var library = RuntimePrefabLibrary.Instance;
            if (library == null)
                throw new System.InvalidOperationException(
                    "Resources/StarlightBar/RuntimePrefabLibrary 자산이 필요합니다.");
            return library;
        }
    }
}
