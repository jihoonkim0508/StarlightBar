using StarlightBar.Core;
using TMPro;
using UnityEngine;

namespace StarlightBar.UI
{
    // 퀘스트 패널: 토글 버튼 + 퀘스트 진행 목록 패널을 코드로 생성하고 최신 상태로 유지
    public sealed class QuestLogPanelUI : MonoBehaviour
    {
        private UITheme theme;
        private GameObject panel;
        private RectTransform content;

        // 토글 버튼과 패널 틀을 생성 (아직 데이터는 안 채움)
        private void Awake()
        {
            theme = GetComponent<UITheme>();

            var toggle = UIFactory.CreateButton("OpenQuestButton", transform, "퀘스트", theme.Font, UITheme.PanelColor, UITheme.TextPrimary, TogglePanel);
            UIFactory.AnchorTopLeftStack((RectTransform)toggle.transform, 1, new Vector2(168, 53), 24f, 92f, 12f);

            var chrome = UIFactory.CreatePanelChrome("QuestPanel", transform, "퀘스트", theme.Font, new Vector2(480, 420));
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

        // 현재 퀘스트 상태를 읽어서 목록을 다시 그림 (상태별 색상 구분)
        private void Refresh()
        {
            for (var i = content.childCount - 1; i >= 0; i--)
                Destroy(content.GetChild(i).gameObject);

            var entries = GameManager.Instance?.Quests?.Entries;
            if (entries == null || entries.Count == 0)
            {
                UIFactory.CreateLabel("EmptyLabel", content, "진행 중인 퀘스트가 없습니다.", theme.Font, 20f, UITheme.TextMuted, TextAlignmentOptions.Center);
                return;
            }

            foreach (var entry in entries)
            {
                var row = UIFactory.CreateRow($"Row_{entry.questId}", content, 56f);

                var nameLabel = UIFactory.CreateLabel("Name", row.transform, entry.questId, theme.Font, 20f, UITheme.TextPrimary, TextAlignmentOptions.MidlineLeft);
                UIFactory.StretchWithMargins(nameLabel.rectTransform, 16f, 0f, 100f, 0f);

                var stateColor = entry.state switch
                {
                    QuestState.Complete => UITheme.AccentGold,
                    QuestState.InProgress => UITheme.AccentCoral,
                    _ => UITheme.TextMuted
                };
                var stateText = entry.state switch
                {
                    QuestState.Complete => "완료",
                    QuestState.InProgress => "진행중",
                    _ => "시작 전"
                };

                var stateLabel = UIFactory.CreateLabel("State", row.transform, stateText, theme.Font, 20f, stateColor, TextAlignmentOptions.MidlineRight);
                UIFactory.StretchWithMargins(stateLabel.rectTransform, 0f, 0f, 16f, 0f);
            }
        }
    }
}
