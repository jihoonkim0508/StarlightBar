using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarlightBar.UI
{
    /// <summary>
    /// MainMenu 씬에서 팀원이 직접 편집하는 모든 고정 UI 참조를 보관합니다.
    /// </summary>
    public sealed class MainMenuView : MonoBehaviour
    {
        [Header("기본 메뉴")]
        [SerializeField, Tooltip("주인공 이름 입력창입니다.")]
        private TMP_InputField playerNameInput;
        [SerializeField, Tooltip("이어하기 버튼입니다.")]
        private Button continueButton;
        [SerializeField, Tooltip("새 게임 버튼입니다.")]
        private Button newGameButton;
        [SerializeField, Tooltip("챕터 기록 버튼입니다.")]
        private Button archiveButton;
        [SerializeField, Tooltip("설정 버튼입니다.")]
        private Button settingsButton;
        [SerializeField, Tooltip("종료 버튼입니다.")]
        private Button exitButton;

        [Header("기록 보관소")]
        [SerializeField, Tooltip("기록 보관소 전체 오브젝트입니다.")]
        private GameObject archiveRoot;
        [SerializeField, Tooltip("기록 행 프리팹이 배치되는 부모입니다.")]
        private Transform archiveContent;
        [SerializeField, Tooltip("복원 기록 한 행을 표시하는 프리팹입니다.")]
        private GameObject archiveEntryPrefab;
        [SerializeField, Tooltip("기록이 없을 때 표시할 안내문입니다.")]
        private GameObject archiveEmptyMessage;
        [SerializeField, Tooltip("기록 보관소 닫기 버튼입니다.")]
        private Button archiveCloseButton;

        public TMP_InputField PlayerNameInput => playerNameInput;
        public Button ContinueButton => continueButton;
        public Button NewGameButton => newGameButton;
        public Button ArchiveButton => archiveButton;
        public Button SettingsButton => settingsButton;
        public Button ExitButton => exitButton;
        public GameObject ArchiveRoot => archiveRoot;
        public Transform ArchiveContent => archiveContent;
        public GameObject ArchiveEntryPrefab => archiveEntryPrefab;
        public GameObject ArchiveEmptyMessage => archiveEmptyMessage;
        public Button ArchiveCloseButton => archiveCloseButton;
    }
}
