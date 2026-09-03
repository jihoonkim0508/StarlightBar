using System;
using System.Collections.Generic;

namespace StarlightBar.Core
{
    // 인벤토리(보유 아이템 목록)의 추가/제거/조회를 담당.
    // SaveData.inventory를 직접 들고 있는 게 아니라 참조만 해서 조작 = 곧 저장 데이터 조작.
    public sealed class InventoryManager
    {
        private readonly SaveData saveData;

        // 인벤토리 내용이 바뀔 때마다 발생. 구독자(예: InventoryPanelUI)가 이걸로 자기 화면을 갱신함.
        public event Action OnChanged;

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
            return entry?.Quantity ?? 0;
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
                entry = new InventoryEntry(itemId, 0);
                saveData.inventory.Add(entry);
            }

            entry.ChangeQuantity(quantity);
            RaiseChanged();
        }

        // 아이템 차감: 수량이 부족하면 실패(false), 0이 되면 목록에서 제거
        public bool RemoveItem(string itemId, int quantity = 1)
        {
            if (quantity <= 0)
                return false;

            var entry = Find(itemId);
            if (entry == null || entry.Quantity < quantity)
                return false;

            entry.ChangeQuantity(-quantity);
            if (entry.Quantity <= 0)
                saveData.inventory.Remove(entry);

            RaiseChanged();
            return true;
        }

        // itemId로 기존 인벤토리 항목 찾기 (없으면 null)
        private InventoryEntry Find(string itemId)
        {
            foreach (var entry in saveData.inventory)
            {
                if (entry.ItemId == itemId)
                    return entry;
            }

            return null;
        }

        private void RaiseChanged() => OnChanged?.Invoke();
    }
}
