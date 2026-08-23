using StarlightBar.Core;
using TMPro;
using UnityEngine;

namespace StarlightBar.UI
{
    // 인벤토리 패널: 토글 버튼 + 보유 아이템 목록 패널을 코드로 생성하고 최신 상태로 유지
    public sealed class InventoryPanelUI : MonoBehaviour
    {
        private UITheme theme;
        private GameObject panel;
        private RectTransform content;

        // 토글 버튼과 패널 틀을 생성 (아직 데이터는 안 채움)
        private void Awake()
        {
            theme = GetComponent<UITheme>();

            var toggle = UIFactory.CreateButton("OpenInventoryButton", transform, "인벤토리", theme.Font, UITheme.PanelColor, UITheme.TextPrimary, TogglePanel);
            UIFactory.AnchorTopLeftStack((RectTransform)toggle.transform, 0, new Vector2(168, 53), 24f, 92f, 12f);

            var chrome = UIFactory.CreatePanelChrome("InventoryPanel", transform, "인벤토리", theme.Font, new Vector2(480, 420));
            panel = chrome.panel;
            content = chrome.content;
        }

        // 상태 변경 이벤트 구독 + 최초 1회 화면 갱신
        private void Start()
        {
            GameEvents.OnStateChanged += Refresh;
            Refresh();
        }

        // 파괴될 때 이벤트 구독 해제 (메모리 누수 방지)
        private void OnDestroy()
        {
            GameEvents.OnStateChanged -= Refresh;
        }

        // 토글 버튼 클릭 시 패널 열기/닫기
        private void TogglePanel()
        {
            var next = !panel.activeSelf;
            panel.SetActive(next);
            if (next)
                Refresh();
        }

        // 현재 인벤토리 상태를 읽어서 목록을 다시 그림
        private void Refresh()
        {
            for (var i = content.childCount - 1; i >= 0; i--)
                Destroy(content.GetChild(i).gameObject);

            var entries = GameManager.Instance?.Inventory?.Entries;
            if (entries == null || entries.Count == 0)
            {
                UIFactory.CreateLabel("EmptyLabel", content, "아직 가진 아이템이 없습니다.", theme.Font, 20f, UITheme.TextMuted, TextAlignmentOptions.Center);
                return;
            }

            foreach (var entry in entries)
            {
                var row = UIFactory.CreateRow($"Row_{entry.itemId}", content, 56f);

                var nameLabel = UIFactory.CreateLabel("Name", row.transform, entry.itemId, theme.Font, 20f, UITheme.TextPrimary, TextAlignmentOptions.MidlineLeft);
                UIFactory.StretchWithMargins(nameLabel.rectTransform, 16f, 0f, 80f, 0f);

                var qtyLabel = UIFactory.CreateLabel("Qty", row.transform, $"x{entry.quantity}", theme.Font, 20f, UITheme.AccentCoral, TextAlignmentOptions.MidlineRight);
                UIFactory.StretchWithMargins(qtyLabel.rectTransform, 0f, 0f, 16f, 0f);
            }
        }
    }
}
