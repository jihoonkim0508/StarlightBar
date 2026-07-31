using TMPro;
using StarlightBar.Content;
using UnityEngine;
using UnityEngine.UI;

namespace StarlightBar.UI
{
    /// <summary>
    /// 조사 상세 화면의 편집 가능한 고정 참조입니다.
    /// </summary>
    public sealed class InvestigationView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text title;
        [SerializeField] private Image objectImage;
        [SerializeField] private TMP_Text body;
        [SerializeField] private TMP_Text metadata;
        [SerializeField] private TMP_Text notebookNotice;
        [SerializeField] private Button addMemoButton;
        [SerializeField] private Button compareButton;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button closeButton;
        [Header("조사 종류별 표시색")]
        [SerializeField] private Color defaultColor = new(0.72f, 0.68f, 0.82f, 0.88f);
        [SerializeField] private Color mythColor = new(0.42f, 0.86f, 0.95f, 0.85f);
        [SerializeField] private Color antagonistColor = new(0.32f, 0.12f, 0.42f, 0.90f);
        [SerializeField] private Color ingredientColor = new(0.56f, 0.76f, 0.48f, 0.88f);
        [SerializeField] private Color furnitureColor = new(0.72f, 0.58f, 0.40f, 0.88f);

        public GameObject Root => root;
        public TMP_Text Title => title;
        public Image ObjectImage => objectImage;
        public TMP_Text Body => body;
        public TMP_Text Metadata => metadata;
        public TMP_Text NotebookNotice => notebookNotice;
        public Button AddMemoButton => addMemoButton;
        public Button CompareButton => compareButton;
        public Button ConfirmButton => confirmButton;
        public Button CloseButton => closeButton;

        /// <summary>
        /// Inspector에서 지정한 조사 종류별 표시색을 반환합니다.
        /// </summary>
        public Color ColorFor(ObjectiveType type) => type switch
        {
            ObjectiveType.MythEvidence => mythColor,
            ObjectiveType.AntagonistEvidence => antagonistColor,
            ObjectiveType.RequiredIngredient => ingredientColor,
            ObjectiveType.Furniture => furnitureColor,
            _ => defaultColor
        };
    }
}
