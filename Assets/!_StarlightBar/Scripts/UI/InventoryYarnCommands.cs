using StarlightBar.Core;
using StarlightBar.Gameplay.Items;
using Yarn.Unity;

namespace StarlightBar.UI
{
    public static class InventoryYarnCommands
    {
        [YarnCommand("give_item")]
        public static void GiveItem(string itemId, int quantity = 1)
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null || quantity <= 0 || !ItemCatalog.TryGet(itemId, out var item))
                return;

            gameManager.Inventory.Add(item, quantity);
            if (!gameManager.SaveGame())
                gameManager.Inventory.Remove(item, quantity);
        }

        [YarnCommand("take_item")]
        public static void TakeItem(string itemId, int quantity = 1)
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null || quantity <= 0 || !ItemCatalog.TryGet(itemId, out var item) ||
                !gameManager.Inventory.Remove(item, quantity))
                return;

            if (!gameManager.SaveGame())
                gameManager.Inventory.Add(item, quantity);
        }

        [YarnFunction("has_item")]
        public static bool HasItem(string itemId) =>
            GameManager.Instance?.Inventory.Count(itemId) > 0;

        [YarnFunction("get_item_count")]
        public static int GetItemCount(string itemId) =>
            GameManager.Instance?.Inventory.Count(itemId) ?? 0;

        [YarnFunction("get_item_name")]
        public static string GetItemName(string itemId) => ItemCatalog.GetDisplayName(itemId);
    }
}
