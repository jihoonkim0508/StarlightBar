using UnityEngine;

namespace StarlightBar.UI
{
    /// <summary>
    /// 주점의 단계별 콘텐츠가 표시되는 편집 가능한 패널 참조입니다.
    /// </summary>
    public sealed class TavernView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private RectTransform content;
        [SerializeField] private GameObject categoryRoot;
        [SerializeField] private RectTransform categoryContent;
        [SerializeField] private GameObject furnitureRoot;

        public GameObject Root => root;
        public RectTransform Content => content;
        public GameObject CategoryRoot => categoryRoot;
        public RectTransform CategoryContent => categoryContent;
        public GameObject FurnitureRoot => furnitureRoot;
    }
}
