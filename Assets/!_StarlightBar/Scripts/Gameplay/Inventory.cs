using System;
using System.Collections.Generic;
using StarlightBar.Gameplay.Items;

namespace StarlightBar.Gameplay
{
    public sealed class Inventory
    {
        private sealed class Entry
        {
            public IItem Item;
            public int Count;
        }

        private readonly Dictionary<string, Entry> items = new();

        public event Action Changed;

        public IEnumerable<KeyValuePair<IItem, int>> Items
        {
            get
            {
                foreach (var entry in items.Values)
                    yield return new KeyValuePair<IItem, int>(entry.Item, entry.Count);
            }
        }

        public int Count(IItem item) => item == null ? 0 : Count(item.Id);

        public int Count(string itemId) =>
            !string.IsNullOrWhiteSpace(itemId) && items.TryGetValue(itemId, out var entry)
                ? entry.Count
                : 0;

        public void Clear()
        {
            if (items.Count == 0)
                return;

            items.Clear();
            Changed?.Invoke();
        }

        public void Add(IItem item, int amount = 1)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.Id) || amount <= 0)
                return;

            if (!items.TryGetValue(item.Id, out var entry))
            {
                entry = new Entry { Item = item };
                items.Add(item.Id, entry);
            }
            else if (entry.Item.Icon == null && item.Icon != null)
            {
                entry.Item = item;
            }

            entry.Count += amount;
            Changed?.Invoke();
        }

        public bool Remove(IItem item, int amount = 1) =>
            item != null && Remove(item.Id, amount);

        public bool Remove(string itemId, int amount = 1)
        {
            if (string.IsNullOrWhiteSpace(itemId) || amount <= 0 ||
                !items.TryGetValue(itemId, out var entry) || entry.Count < amount)
                return false;

            var remaining = entry.Count - amount;
            if (remaining == 0)
                items.Remove(itemId);
            else
                entry.Count = remaining;

            Changed?.Invoke();
            return true;
        }
    }
}
