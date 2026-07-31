using StarlightBar.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarlightBar.UI
{
    /// <summary>
    /// 배치 가능한 가구 한 개의 외형과 조작 컴포넌트를 보관합니다.
    /// </summary>
    public sealed class FurnitureItemView : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private TMP_Text label;
        [SerializeField] private FurnitureDraggableView draggable;

        public FurnitureDraggableView Draggable => draggable;

        /// <summary>
        /// 가구 콘텐츠가 제공하는 이미지와 이름을 표시합니다.
        /// </summary>
        public void Bind(Sprite sprite, string displayName)
        {
            if (sprite != null)
                image.sprite = sprite;
            label.text = displayName;
        }
    }
}
