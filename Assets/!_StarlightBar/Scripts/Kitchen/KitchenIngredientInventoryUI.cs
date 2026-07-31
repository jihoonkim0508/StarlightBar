using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace StarlightBar.Gameplay
{
    /// <summary>
    /// Kitchen 재료 인벤토리의 팝업, 버튼, 텍스트와 슬롯 참조를 보관합니다.
    /// </summary>
    public sealed class KitchenIngredientInventoryUI : MonoBehaviour
    {
        [Header("재료 선택 UI")]
        [SerializeField, Tooltip("재료 인벤토리를 여는 버튼입니다.")]
        private Button openInventoryButton;
        [SerializeField, Tooltip("재료 슬롯을 포함하는 인벤토리 팝업입니다.")]
        private GameObject inventoryPopup;
        [SerializeField, Tooltip("콘텐츠를 표시할 재료 슬롯 목록입니다.")]
        private KitchenIngredientSlotUI[] slots;

        [Header("배치 확인 UI")]
        [SerializeField, Tooltip("재료 배치 여부를 묻는 확인 팝업입니다.")]
        private GameObject confirmationPopup;
        [SerializeField, Tooltip("선택한 재료 이름과 확인 문구를 표시합니다.")]
        private TMP_Text confirmationText;
        [SerializeField, Tooltip("선택한 재료를 배치하는 버튼입니다.")]
        private Button confirmButton;
        [SerializeField, Tooltip("재료 배치를 취소하는 버튼입니다.")]
        private Button cancelButton;

        private UnityAction toggleInventoryAction;

        /// <summary>
        /// UI에 연결된 슬롯 수를 반환합니다.
        /// </summary>
        public int SlotCount => slots?.Length ?? 0;

        /// <summary>
        /// 재료 인벤토리 팝업이 현재 열려 있는지 반환합니다.
        /// </summary>
        public bool IsInventoryVisible => inventoryPopup != null && inventoryPopup.activeSelf;

        /// <summary>
        /// 필수 UI 참조가 모두 연결되어 있는지 반환합니다.
        /// </summary>
        public bool HasRequiredReferences =>
            openInventoryButton != null &&
            inventoryPopup != null &&
            slots != null &&
            confirmationPopup != null &&
            confirmationText != null &&
            confirmButton != null &&
            cancelButton != null;

        /// <summary>
        /// Kitchen 진입 시 열기 버튼 동작과 팝업의 초기 표시 상태를 설정합니다.
        /// </summary>
        public void Initialize(UnityAction toggleAction)
        {
            if (toggleInventoryAction != null)
                openInventoryButton.onClick.RemoveListener(toggleInventoryAction);

            toggleInventoryAction = toggleAction;
            if (toggleInventoryAction != null)
                openInventoryButton.onClick.AddListener(toggleInventoryAction);

            inventoryPopup.SetActive(false);
            confirmationPopup.SetActive(false);
        }

        private void OnDestroy()
        {
            if (openInventoryButton != null && toggleInventoryAction != null)
                openInventoryButton.onClick.RemoveListener(toggleInventoryAction);
        }

        /// <summary>
        /// 재료 인벤토리 팝업의 표시 상태를 설정합니다.
        /// </summary>
        public void SetInventoryVisible(bool visible) => inventoryPopup.SetActive(visible);

        /// <summary>
        /// 선택한 재료의 배치 확인 문구와 팝업을 표시합니다.
        /// </summary>
        public void ShowConfirmation(string message)
        {
            confirmationText.text = message;
            confirmationPopup.SetActive(true);
        }

        /// <summary>
        /// 재료 배치 확인 팝업을 닫습니다.
        /// </summary>
        public void HideConfirmation() => confirmationPopup.SetActive(false);

        /// <summary>
        /// 지정한 슬롯에 재료 표시 정보와 선택 동작을 연결합니다.
        /// </summary>
        public void BindSlot(int index, Sprite sprite, int quantity, Action selected)
        {
            if (index >= 0 && index < SlotCount)
                slots[index]?.Bind(sprite, quantity, selected);
        }

        /// <summary>
        /// 지정한 슬롯의 표시와 상호작용을 비활성화합니다.
        /// </summary>
        public void ClearSlot(int index)
        {
            if (index >= 0 && index < SlotCount)
                slots[index]?.Clear();
        }
    }
}
