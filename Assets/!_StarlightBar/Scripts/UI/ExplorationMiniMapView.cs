using TMPro;
using UnityEngine;

namespace StarlightBar.UI
{
    /// <summary>
    /// 혜화동 미니맵의 고정 레이아웃과 반복 생성용 표식 프리팹을 보관합니다.
    /// </summary>
    public sealed class ExplorationMiniMapView : MonoBehaviour
    {
        [Header("에디터에서 배치한 고정 요소")]
        [SerializeField] private RectTransform mapArea;
        [SerializeField] private RectTransform playerDot;
        [SerializeField] private TMP_Text toolText;

        [Header("반복 항목")]
        [SerializeField] private MiniMapDotView markerPrefab;

        public RectTransform MapArea => mapArea;
        public RectTransform PlayerDot => playerDot;
        public TMP_Text ToolText => toolText;
        public MiniMapDotView MarkerPrefab => markerPrefab;
    }
}
