using StarlightBar.Core;
using UnityEngine;
using UnityEngine.UI;

namespace StarlightBar.UI
{
    public sealed class InventoryPanelUI : MonoBehaviour
    {
        [SerializeField] private Button openButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private GameObject panel;
        [SerializeField] private Transform content;
        [SerializeField] private InventoryRowView rowTemplate;
        [SerializeField] private GameObject emptyLabel;

        private void Awake()
        {
            openButton?.onClick.AddListener(TogglePanel);
            closeButton?.onClick.AddListener(ClosePanel);
            rowTemplate?.gameObject.SetActive(false);
            panel?.SetActive(false);
        }

        private void OnEnable()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.Inventory.Changed += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.Inventory.Changed -= Refresh;
        }

        private void OnDestroy()
        {
            openButton?.onClick.RemoveListener(TogglePanel);
            closeButton?.onClick.RemoveListener(ClosePanel);
        }

        private void TogglePanel()
        {
            if (panel == null)
                return;

            panel.SetActive(!panel.activeSelf);
            if (panel.activeSelf)
                Refresh();
        }

        private void ClosePanel() => panel?.SetActive(false);

        private void Refresh()
        {
            if (content == null || rowTemplate == null)
                return;

            for (var i = content.childCount - 1; i >= 0; i--)
            {
                var child = content.GetChild(i);
                if (child != rowTemplate.transform)
                    Destroy(child.gameObject);
            }

            var hasItems = false;
            var inventory = GameManager.Instance?.Inventory;
            if (inventory != null)
            {
                foreach (var pair in inventory.Items)
                {
                    hasItems = true;
                    var row = Instantiate(rowTemplate, content);
                    row.gameObject.SetActive(true);
                    row.SetData(pair.Key.DisplayName, pair.Value);
                }
            }

            emptyLabel?.SetActive(!hasItems);
        }
    }
}
