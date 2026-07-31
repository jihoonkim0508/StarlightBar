using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace StarlightBar.Gameplay
{
    /// <summary>
    /// Kitchen 재료 인벤토리의 슬롯 하나에 버튼, 아이콘과 선택 상태를 연결합니다.
    /// </summary>
    public sealed class KitchenIngredientSlotUI : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField, Tooltip("선택 사항입니다. 연결하지 않으면 슬롯 Image가 직접 클릭을 받습니다.")]
        private Button selectButton;
        [SerializeField, Tooltip("재료 Sprite를 표시할 슬롯 내부 Image입니다.")]
        private Image icon;
        [SerializeField, Tooltip("수량 표기가 필요할 때 연결하는 선택 항목입니다.")]
        private TMP_Text quantityText;

        private Action selected;
        private bool isInteractable;

        /// <summary>
        /// 슬롯에 재료 이미지, 수량과 선택 동작을 표시합니다.
        /// </summary>
        public void Bind(Sprite sprite, int quantity, Action selectionAction)
        {
            if (icon == null)
                return;
            selected = selectionAction;
            isInteractable = sprite != null && quantity > 0;
            icon.sprite = sprite;
            icon.preserveAspect = true;
            icon.enabled = sprite != null;
            if (selectButton != null)
            {
                selectButton.interactable = isInteractable;
                selectButton.onClick.RemoveAllListeners();
                if (selected != null)
                    selectButton.onClick.AddListener(selected.Invoke);
            }
            if (quantityText != null)
                quantityText.text = quantity.ToString();
        }

        /// <summary>
        /// 사용하지 않는 슬롯의 아이콘과 상호작용을 비활성화합니다.
        /// </summary>
        public void Clear()
        {
            selected = null;
            isInteractable = false;
            if (selectButton != null)
            {
                selectButton.onClick.RemoveAllListeners();
                selectButton.interactable = false;
            }
            if (icon != null)
                icon.enabled = false;
            if (quantityText != null)
                quantityText.text = string.Empty;
        }

        /// <summary>
        /// Button이 없는 기존 슬롯에서는 Image의 포인터 클릭으로 재료를 선택합니다.
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (selectButton == null && isInteractable)
                selected?.Invoke();
        }
    }
}
