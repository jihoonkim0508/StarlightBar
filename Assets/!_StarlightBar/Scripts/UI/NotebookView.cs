using TMPro;
using UnityEngine;
using UnityEngine.UI;
using StarlightBar.Gameplay;

namespace StarlightBar.UI
{
    /// <summary>
    /// 패스파인더 노트의 탭과 콘텐츠 영역 참조입니다.
    /// </summary>
    public sealed class NotebookView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private RectTransform content;
        [SerializeField] private TMP_Text status;
        [SerializeField] private Button objectivesTab;
        [SerializeField] private Button evidenceTab;
        [SerializeField] private Button candidatesTab;
        [SerializeField] private Button mythsTab;
        [SerializeField] private Button foodTab;
        [SerializeField] private Button personTab;
        [SerializeField] private Button archiveTab;
        [SerializeField] private Button closeButton;
        [SerializeField, Tooltip("증거 탭에서 반복 배치할 카드 프리팹입니다.")]
        private EvidenceCardView evidenceCardPrefab;

        public GameObject Root => root;
        public RectTransform Content => content;
        public TMP_Text Status => status;
        public Button ObjectivesTab => objectivesTab;
        public Button EvidenceTab => evidenceTab;
        public Button CandidatesTab => candidatesTab;
        public Button MythsTab => mythsTab;
        public Button FoodTab => foodTab;
        public Button PersonTab => personTab;
        public Button ArchiveTab => archiveTab;
        public Button CloseButton => closeButton;
        public EvidenceCardView EvidenceCardPrefab => evidenceCardPrefab;
    }
}
