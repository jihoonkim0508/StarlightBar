using StarlightBar.Content;
using StarlightBar.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace StarlightBar.Gameplay
{
    /// <summary>
    /// 가구 한 개의 드래그, 선택, 회전과 보관 동작을 저장 데이터에 반영합니다.
    /// </summary>
    [RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
    public sealed class FurnitureDraggableView :
        MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
    {
        private FurniturePlacementData placement;
        private RectTransform area;
        private RectTransform rect;
        private Canvas canvas;
        private System.Action selected;
        private System.Action changed;

        public FurnitureDefinition Definition { get; private set; }

        /// <summary>
        /// 가구 데이터와 저장된 배치 상태 및 변경 콜백을 연결합니다.
        /// </summary>
        public void Initialize(
            FurnitureDefinition definition,
            FurniturePlacementData data,
            RectTransform placementArea,
            System.Action onSelected,
            System.Action onChanged)
        {
            Definition = definition;
            placement = data;
            area = placementArea;
            selected = onSelected;
            changed = onChanged;
            rect = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();
        }

        /// <summary>
        /// 가구 이동을 시작할 때 현재 항목을 선택합니다.
        /// </summary>
        public void OnBeginDrag(PointerEventData eventData)
        {
            selected?.Invoke();
            GetComponent<CanvasGroup>().alpha = 0.75f;
        }

        /// <summary>
        /// 가구를 주점 배치 영역 안에서 이동합니다.
        /// </summary>
        public void OnDrag(PointerEventData eventData)
        {
            var scale = canvas == null ? 1f : canvas.scaleFactor;
            rect.anchoredPosition += eventData.delta / Mathf.Max(0.01f, scale);
            var halfArea = area.rect.size * 0.5f;
            var halfItem = rect.rect.size * 0.5f;
            rect.anchoredPosition = new Vector2(
                Mathf.Clamp(rect.anchoredPosition.x, -halfArea.x + halfItem.x, halfArea.x - halfItem.x),
                Mathf.Clamp(rect.anchoredPosition.y, -halfArea.y + halfItem.y, halfArea.y - halfItem.y));
        }

        /// <summary>
        /// 드래그가 끝난 가구 위치를 저장합니다.
        /// </summary>
        public void OnEndDrag(PointerEventData eventData)
        {
            GetComponent<CanvasGroup>().alpha = 1f;
            placement.position = rect.anchoredPosition;
            changed?.Invoke();
        }

        /// <summary>
        /// 우클릭한 가구를 90도 단위로 회전합니다.
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            selected?.Invoke();
            if (eventData.button != PointerEventData.InputButton.Right)
                return;
            placement.rotation = (placement.rotation + 90f) % 360f;
            rect.localRotation = Quaternion.Euler(0, 0, placement.rotation);
            changed?.Invoke();
        }

        /// <summary>
        /// 선택한 가구를 보관함 상태로 전환합니다.
        /// </summary>
        public void Store()
        {
            placement.stored = true;
            gameObject.SetActive(false);
            changed?.Invoke();
        }
    }
}
