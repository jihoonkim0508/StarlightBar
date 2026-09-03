using TMPro;
using UnityEngine;

namespace StarlightBar.UI
{
    // 인벤토리 패널의 목록 한 줄. Name/Qty 라벨은 프리팹에서 Inspector로 직접 연결됨
    // (에디터 도구가 처음 만들 때 자동 연결해주고, 이후엔 프리팹에서 드래그로 위치만 조정하면 됨).
    public sealed class InventoryRowView : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private TMP_Text quantityLabel;

        // 이 행에 표시할 아이템 이름/수량 설정
        public void SetData(string itemName, int quantity)
        {
            if (nameLabel != null)
                nameLabel.text = itemName;
            if (quantityLabel != null)
                quantityLabel.text = $"x{quantity}";
        }
    }
}
