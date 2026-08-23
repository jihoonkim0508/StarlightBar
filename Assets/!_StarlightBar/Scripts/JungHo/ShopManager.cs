namespace StarlightBar.Core
{
    // 상점 구매 처리: 재화 확인 → 차감 → 아이템 지급을 한 번에 수행
    public sealed class ShopManager
    {
        private readonly GameManager gameManager;
        private readonly InventoryManager inventory;

        public ShopManager(GameManager gameManager, InventoryManager inventory)
        {
            this.gameManager = gameManager;
            this.inventory = inventory;
        }

        // 구매 시도: 카탈로그에 없거나 재화가 부족하면 실패(false), 성공하면 재화 차감 + 아이템 지급 + 저장
        public bool TryPurchase(string itemId, int quantity = 1)
        {
            if (quantity <= 0)
                return false;

            var offer = FindOffer(itemId);
            if (offer == null)
                return false;

            var totalPrice = offer.Value.price * quantity;
            if (!gameManager.SpendCurrency(totalPrice))
                return false;

            inventory.AddItem(itemId, quantity);
            gameManager.SaveGame();
            return true;
        }

        // itemId로 판매 목록(ShopCatalog)에서 항목 찾기 (없으면 null)
        private ShopOffer? FindOffer(string itemId)
        {
            foreach (var offer in ShopCatalog.Offers)
            {
                if (offer.itemId == itemId)
                    return offer;
            }

            return null;
        }
    }
}
