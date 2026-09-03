using StarlightBar.Core;
using UnityEngine;
using UnityEngine.UI;

namespace StarlightBar.UI
{
    // 인벤토리 패널: 프리팹(InventoryPanelUI.prefab)에 미리 배치된 UI를
    // 인벤토리 데이터로 채우고 열고 닫는 역할만 담당.
    // 배경 색상·크기·위치는 전부 프리팹에서 직접 드래그/Inspector로 조정 → 저장하면 그대로 유지되고
    // Play 모드에서도 저장된 그 상태 그대로 반영됨 (코드가 매번 새로 만드는 게 아니라
    // 프리팹에 이미 저장된 실제 UI를 그대로 사용하기 때문).
    public sealed class InventoryPanelUI : MonoBehaviour
    {
        [SerializeField] private Button openButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private GameObject panel;
        [SerializeField] private Transform content;
        [SerializeField] private InventoryRowView rowTemplate;
        [SerializeField] private GameObject emptyLabel;

        private InventoryManager inventory;

        // 버튼 클릭 연결 + 템플릿/패널 초기 비활성화
        private void Awake()
        {
            if (openButton != null)
                openButton.onClick.AddListener(TogglePanel);
            if (closeButton != null)
                closeButton.onClick.AddListener(ClosePanel);

            if (rowTemplate != null)
                rowTemplate.gameObject.SetActive(false);
            if (panel != null)
                panel.SetActive(false);
        }

        // 인벤토리 변경 이벤트 구독 + 최초 1회 화면 갱신
        private void Start()
        {
            inventory = GameManager.Instance?.Inventory;
            if (inventory != null)
                inventory.OnChanged += Refresh;

            Refresh();
        }

        // 파괴될 때 이벤트 구독 해제 (메모리 누수 방지)
        private void OnDestroy()
        {
            if (inventory != null)
                inventory.OnChanged -= Refresh;
        }

        // 토글 버튼 클릭 시 패널 열기/닫기
        private void TogglePanel()
        {
            if (panel == null)
                return;

            var next = !panel.activeSelf;
            panel.SetActive(next);
            if (next)
                Refresh();
        }

        // 닫기(X) 버튼 클릭 시 패널 닫기
        private void ClosePanel()
        {
            if (panel != null)
                panel.SetActive(false);
        }

        // 현재 인벤토리 상태를 읽어서 목록을 다시 그림 (rowTemplate를 복제해서 채움)
        private void Refresh()
        {
            if (content == null || rowTemplate == null)
                return;

            // rowTemplate 자신은 남기고 이전에 복제해둔 행들만 지움
            for (var i = content.childCount - 1; i >= 0; i--)
            {
                var child = content.GetChild(i);
                if (child != rowTemplate.transform)
                    Destroy(child.gameObject);
            }

            var entries = inventory?.Entries;
            var hasItems = entries != null && entries.Count > 0;

            if (emptyLabel != null)
                emptyLabel.SetActive(!hasItems);

            if (!hasItems)
                return;

            foreach (var entry in entries)
            {
                var row = Instantiate(rowTemplate, content);
                row.gameObject.SetActive(true);
                row.SetData(entry.ItemId, entry.Quantity);
            }
        }
    }
}
