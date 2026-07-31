using UnityEngine;
using UnityEngine.EventSystems;

namespace StarlightBar.Gameplay
{
    /// <summary>
    /// 도마 위에 배치된 재료를 클릭한 동안 확대하고 드래그로 이동시킵니다.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public sealed class KitchenPlacedIngredientDrag :
        MonoBehaviour,
        IPointerDownHandler,
        IDragHandler,
        IPointerUpHandler
    {
        [SerializeField, Tooltip("배치된 재료 이미지를 표시하는 SpriteRenderer입니다.")]
        private SpriteRenderer targetRenderer;
        [SerializeField, Tooltip("마우스 선택과 드래그 판정에 사용하는 Collider입니다.")]
        private BoxCollider2D targetCollider;
        [SerializeField, Range(1f, 1.5f), Tooltip("클릭한 동안 적용할 확대 배율입니다.")]
        private float selectedScale = 1.1f;

        private KitchenIngredientInventory owner;
        private Vector3 restingScale;
        private Vector3 pointerOffset;
        private bool dragging;

        internal string ItemId { get; private set; }
        internal Bounds VisualBounds => targetRenderer.bounds;
        internal int SortingOrder => targetRenderer.sortingOrder;

        /// <summary>
        /// 배치할 음식 ID, Sprite와 기본 크기를 설정합니다.
        /// </summary>
        public void Initialize(
            KitchenIngredientInventory inventory,
            string itemId,
            Sprite sprite,
            Vector3 placedScale)
        {
            owner = inventory;
            ItemId = itemId;
            restingScale = placedScale;
            transform.localScale = placedScale;
            targetRenderer.sprite = sprite;
            if (sprite != null)
            {
                targetCollider.size = sprite.bounds.size;
                targetCollider.offset = sprite.bounds.center;
            }
        }

        /// <summary>
        /// 재료를 선택하고 현재 탑의 맨 위로 올립니다.
        /// </summary>
        public void BeginDrag(Vector3 pointerWorldPosition)
        {
            dragging = true;
            pointerOffset = transform.position - pointerWorldPosition;
            transform.localScale = restingScale * selectedScale;
            owner?.BringToFront(this);
        }

        /// <summary>
        /// 선택한 재료를 포인터 위치로 이동합니다.
        /// </summary>
        public void Drag(Vector3 pointerWorldPosition)
        {
            if (!dragging)
                return;
            var target = pointerWorldPosition + pointerOffset;
            target.z = transform.position.z;
            transform.position = target;
        }

        /// <summary>
        /// 드래그를 끝내고 재료를 원래 크기로 되돌립니다.
        /// </summary>
        public void EndDrag()
        {
            dragging = false;
            transform.localScale = restingScale;
            owner?.TryCompleteRecipe(this);
        }

        /// <summary>
        /// SpriteRenderer의 정렬 순서를 설정합니다.
        /// </summary>
        public void SetSortingOrder(int sortingOrder) =>
            targetRenderer.sortingOrder = sortingOrder;

        /// <summary>
        /// EventSystem이 재료 위에서 포인터 누름을 감지하면 드래그를 시작합니다.
        /// </summary>
        public void OnPointerDown(PointerEventData eventData) =>
            BeginDrag(ReadPointerWorld(eventData));

        /// <summary>
        /// EventSystem의 포인터 위치를 월드 좌표로 변환하여 재료를 이동합니다.
        /// </summary>
        public void OnDrag(PointerEventData eventData) =>
            Drag(ReadPointerWorld(eventData));

        /// <summary>
        /// EventSystem이 포인터 놓기를 감지하면 드래그를 종료합니다.
        /// </summary>
        public void OnPointerUp(PointerEventData eventData) => EndDrag();

        private Vector3 ReadPointerWorld(PointerEventData eventData)
        {
            var targetCamera = eventData?.pressEventCamera ?? Camera.main;
            if (targetCamera == null)
                return transform.position;
            var pointer = eventData != null
                ? (Vector3)eventData.position
                : Vector3.zero;
            pointer.z = Mathf.Abs(targetCamera.transform.position.z - transform.position.z);
            return targetCamera.ScreenToWorldPoint(pointer);
        }
    }
}
