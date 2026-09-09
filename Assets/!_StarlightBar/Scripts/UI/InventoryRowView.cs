using TMPro;
using UnityEngine;

namespace StarlightBar.UI
{
    public sealed class InventoryRowView : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameLabel;
        [SerializeField] private TMP_Text quantityLabel;

        public void SetData(string itemName, int quantity)
        {
            if (nameLabel != null)
                nameLabel.text = itemName;
            if (quantityLabel != null)
                quantityLabel.text = $"x{quantity}";
        }
    }
}
