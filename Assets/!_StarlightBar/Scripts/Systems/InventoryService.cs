using System;
using System.Linq;
using StarlightBar.Core;

namespace StarlightBar.Systems
{
    /// <summary>
    /// 저장 데이터의 재료 수량을 안전하게 조회하고 변경합니다.
    /// </summary>
    public sealed class InventoryService
    {
        private readonly GameSaveData data;

        public event Action<string, int> QuantityChanged;

        /// <summary>
        /// 지정한 저장 데이터의 인벤토리를 조작하는 서비스를 만듭니다.
        /// </summary>
        public InventoryService(GameSaveData saveData)
        {
            data = saveData ?? throw new ArgumentNullException(nameof(saveData));
        }

        /// <summary>
        /// 아이템 ID의 현재 보유 수량을 반환합니다.
        /// </summary>
        public int GetQuantity(string itemId)
        {
            return data.inventory.FirstOrDefault(item => item.itemId == itemId)?.quantity ?? 0;
        }

        /// <summary>
        /// 유효한 아이템 수량을 추가하고 변경 이벤트를 보냅니다.
        /// </summary>
        public void Add(string itemId, int amount)
        {
            if (string.IsNullOrWhiteSpace(itemId) || amount <= 0)
                return;

            var entry = data.inventory.FirstOrDefault(item => item.itemId == itemId);
            if (entry == null)
            {
                entry = new InventoryEntry { itemId = itemId };
                data.inventory.Add(entry);
            }

            entry.quantity += amount;
            QuantityChanged?.Invoke(itemId, entry.quantity);
        }

        /// <summary>
        /// 필요한 수량이 있을 때만 아이템을 소비합니다.
        /// </summary>
        public bool TryConsume(string itemId, int amount)
        {
            if (amount <= 0)
                return false;

            var entry = data.inventory.FirstOrDefault(item => item.itemId == itemId);
            if (entry == null || entry.quantity < amount)
                return false;

            entry.quantity -= amount;
            QuantityChanged?.Invoke(itemId, entry.quantity);
            return true;
        }
    }
}
