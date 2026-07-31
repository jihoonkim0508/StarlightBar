using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarlightBar.UI
{
    /// <summary>
    /// 미니맵에 반복 표시되는 목표 표식의 편집 가능한 프리팹 View입니다.
    /// </summary>
    public sealed class MiniMapDotView : MonoBehaviour
    {
        [SerializeField] private RectTransform rect;
        [SerializeField] private Image image;
        [SerializeField] private TMP_Text label;
        [SerializeField] private Color requiredColor = new(0.22f, 0.90f, 0.98f);
        [SerializeField] private Color optionalColor = new(0.95f, 0.70f, 0.22f);

        public RectTransform Rect => rect;

        /// <summary>
        /// 목표 데이터에 따른 문구와 상태색만 적용합니다.
        /// </summary>
        public void Bind(string text, bool required)
        {
            label.text = text;
            image.color = required ? requiredColor : optionalColor;
        }
    }
}
