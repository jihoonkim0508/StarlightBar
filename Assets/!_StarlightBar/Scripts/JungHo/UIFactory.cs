using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace StarlightBar.UI
{
    // 인벤토리/퀘스트/상점 패널이 런타임에 UI를 코드로 생성할 때 쓰는 공용 헬퍼.
    // 씬(.unity)에 직접 GameObject를 배치하지 않는 이유는 여러 팀원이 같은
    // Bar.unity를 병합하는 중이라 씬 파일 충돌을 피하기 위함.
    internal static class UIFactory
    {
        // 빈 RectTransform(UI 오브젝트) 하나 생성해서 parent 밑에 붙임
        internal static RectTransform CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            return rt;
        }

        // 단색 배경 이미지(패널/버튼 배경 등에 씀) 생성
        internal static Image CreatePanelImage(string name, Transform parent, Color color)
        {
            var rt = CreateRect(name, parent);
            var image = rt.gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        // 텍스트 라벨(TMP) 생성
        internal static TextMeshProUGUI CreateLabel(string name, Transform parent, string text, TMP_FontAsset font, float fontSize, Color color, TextAlignmentOptions alignment)
        {
            var rt = CreateRect(name, parent);
            var label = rt.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.font = font;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = alignment;
            label.raycastTarget = false;
            return label;
        }

        // 클릭 가능한 버튼(배경+라벨+onClick) 생성
        internal static Button CreateButton(string name, Transform parent, string label, TMP_FontAsset font, Color backgroundColor, Color textColor, UnityAction onClick)
        {
            var image = CreatePanelImage(name, parent, backgroundColor);
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            if (onClick != null)
                button.onClick.AddListener(onClick);

            var labelText = CreateLabel("Label", image.transform, label, font, 22f, textColor, TextAlignmentOptions.Center);
            StretchFull(labelText.rectTransform);

            return button;
        }

        // 목록 한 줄(고정 높이 배경) 생성 — 인벤토리/퀘스트/상점 패널의 각 행에 사용
        internal static Image CreateRow(string name, Transform parent, float height)
        {
            var image = CreatePanelImage(name, parent, UITheme.PanelColor);
            var layoutElement = image.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = height;
            layoutElement.flexibleWidth = 1;
            return image;
        }

        // 패널 틀(배경+제목+닫기버튼+세로 목록 컨테이너) 한 세트를 통째로 생성
        internal static (GameObject panel, RectTransform content) CreatePanelChrome(string name, Transform parent, string title, TMP_FontAsset font, Vector2 size)
        {
            var panelImage = CreatePanelImage(name, parent, UITheme.PanelDeepColor);
            var panel = panelImage.gameObject;
            var panelRect = (RectTransform)panel.transform;
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = size;
            panelRect.anchoredPosition = Vector2.zero;
            panel.SetActive(false);

            var header = CreateLabel("Header", panel.transform, title, font, 28f, UITheme.TextPrimary, TextAlignmentOptions.Left);
            header.fontStyle = FontStyles.Bold;
            AnchorTopStretch(header.rectTransform, topMargin: 16f, sideMargin: 24f, height: 40f);

            var closeButton = CreateButton("CloseButton", panel.transform, "X", font, UITheme.AccentCoral, UITheme.TextPrimary, () => panel.SetActive(false));
            AnchorTopRight((RectTransform)closeButton.transform, marginX: 16f, marginY: 16f, size: new Vector2(36, 36));

            var content = CreateRect("Content", panel.transform);
            StretchWithMargins(content, left: 24f, top: 72f, right: 24f, bottom: 24f);
            content.gameObject.AddComponent<RectMask2D>();

            var vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 8f;
            vlg.childControlWidth = true;
            vlg.childForceExpandWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandHeight = false;
            vlg.childAlignment = TextAnchor.UpperCenter;

            return (panel, content);
        }

        // 부모 전체를 꽉 채우도록 앵커 설정
        internal static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        // 상하좌우 여백을 주면서 부모를 채우도록 앵커 설정
        internal static void StretchWithMargins(RectTransform rt, float left, float top, float right, float bottom)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
        }

        // 상단에 가로로 붙이고(폭은 늘어남) 높이만 고정
        internal static void AnchorTopStretch(RectTransform rt, float topMargin, float sideMargin, float height)
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(-sideMargin * 2, height);
            rt.anchoredPosition = new Vector2(0, -topMargin);
        }

        // 우측 상단 모서리에 고정 크기로 배치
        internal static void AnchorTopRight(RectTransform rt, float marginX, float marginY, Vector2 size)
        {
            rt.anchorMin = new Vector2(1, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(1, 1);
            rt.sizeDelta = size;
            rt.anchoredPosition = new Vector2(-marginX, -marginY);
        }

        // 좌측 상단에서 시작해 index 순서대로 세로로 쌓아 배치 (인벤토리/퀘스트/상점 토글 버튼 3개에 사용)
        internal static void AnchorTopLeftStack(RectTransform rt, int index, Vector2 size, float startX, float startY, float gap)
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.sizeDelta = size;
            rt.anchoredPosition = new Vector2(startX, -(startY + index * (size.y + gap)));
        }
    }
}
