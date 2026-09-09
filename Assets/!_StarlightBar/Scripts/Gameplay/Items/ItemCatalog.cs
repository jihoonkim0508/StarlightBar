using System.Collections.Generic;
using UnityEngine;

namespace StarlightBar.Gameplay.Items
{
    public static class ItemCatalog
    {
        private sealed class TextItem : IItem
        {
            public TextItem(string id, string displayName)
            {
                Id = id;
                DisplayName = displayName;
            }

            public string Id { get; }
            public string DisplayName { get; }
            public Sprite Icon => null;
        }

        private static readonly Dictionary<string, IItem> Items = new()
        {
            ["warm_milk"] = new TextItem("warm_milk", "따뜻한 우유"),
            ["star_honey_milk"] = new TextItem("star_honey_milk", "별꿀 우유"),
            ["star_lantern"] = new TextItem("star_lantern", "별등")
        };

        public static bool TryGet(string itemId, out IItem item) =>
            Items.TryGetValue(itemId, out item);

        public static string GetDisplayName(string itemId) =>
            TryGet(itemId, out var item) ? item.DisplayName : itemId;

        public static IItem FromSave(string itemId, string displayName) =>
            TryGet(itemId, out var item)
                ? item
                : new TextItem(itemId, string.IsNullOrWhiteSpace(displayName) ? itemId : displayName);
    }
}
