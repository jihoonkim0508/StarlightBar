using System.Collections.Generic;

namespace StarlightBar.Core
{
    // 인벤토리(보유 아이템 목록)의 추가/제거/조회를 담당.
    // SaveData.inventory를 직접 들고 있는 게 아니라 참조만 해서 조작 = 곧 저장 데이터 조작.
    public sealed class InventoryManager
    {
        private readonly SaveData saveData;

        public InventoryManager(SaveData saveData)
        {
            this.saveData = saveData;
        }

        // 현재 보유 중인 아이템 전체 목록 (읽기 전용)
        public IReadOnlyList<InventoryEntry> Entries => saveData.inventory;

        // 특정 아이템의 보유 수량 조회 (없으면 0)
        public int GetQuantity(string itemId)
        {
            var entry = Find(itemId);
            return entry?.quantity ?? 0;
        }

        // 특정 수량 이상 보유 중인지 확인
        public bool HasItem(string itemId, int quantity = 1) => GetQuantity(itemId) >= quantity;

        // 아이템 지급: 이미 있으면 수량만 증가, 없으면 새 항목 추가
        public void AddItem(string itemId, int quantity = 1)
        {
            if (quantity <= 0)
                return;

            var entry = Find(itemId);
            if (entry == null)
            {
                entry = new InventoryEntry { itemId = itemId, quantity = 0 };
                saveData.inventory.Add(entry);
            }

            entry.quantity += quantity;
            GameEvents.RaiseStateChanged();
        }

        // 아이템 차감: 수량이 부족하면 실패(false), 0이 되면 목록에서 제거
        public bool RemoveItem(string itemId, int quantity = 1)
        {
            if (quantity <= 0)
                return false;

            var entry = Find(itemId);
            if (entry == null || entry.quantity < quantity)
                return false;

            entry.quantity -= quantity;
            if (entry.quantity <= 0)
                saveData.inventory.Remove(entry);

            GameEvents.RaiseStateChanged();
            return true;
        }

        // itemId로 기존 인벤토리 항목 찾기 (없으면 null)
        private InventoryEntry Find(string itemId)
        {
            foreach (var entry in saveData.inventory)
            {
                if (entry.itemId == itemId)
                    return entry;
            }

            return null;
        }
    }
}
