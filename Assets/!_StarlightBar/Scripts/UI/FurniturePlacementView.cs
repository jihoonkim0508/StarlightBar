using TMPro;
using UnityEngine;

namespace StarlightBar.UI
{
    /// <summary>
    /// 가구 배치 영역과 반복 생성 가구 프리팹을 Inspector에 노출합니다.
    /// </summary>
    public sealed class FurniturePlacementView : MonoBehaviour
    {
        [SerializeField] private RectTransform placementArea;
        [SerializeField] private TMP_Text status;
        [SerializeField] private FurnitureItemView itemPrefab;

        public RectTransform PlacementArea => placementArea;
        public TMP_Text Status => status;
        public FurnitureItemView ItemPrefab => itemPrefab;
    }

}
