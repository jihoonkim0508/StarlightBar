using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarlightBar.UI
{
    /// <summary>
    /// 혜화동 씬의 고정 HUD와 복귀 선택 UI 참조입니다.
    /// </summary>
    public sealed class DayExplorationView : MonoBehaviour
    {
        [SerializeField] private TMP_Text timeText;
        [SerializeField] private Image timeDial;
        [SerializeField] private TMP_Text objectiveText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text contextPrompt;
        [SerializeField] private TMP_Text notificationText;
        [SerializeField] private GameObject returnActions;
        [SerializeField] private Button returnButton;
        [SerializeField] private Button continueExplorationButton;
        [SerializeField, Tooltip("챕터 조사 대상을 배치할 편집 가능한 위치 목록입니다.")]
        private Transform[] objectiveSpawnPoints;
        [Header("시간 상태 표시")]
        [SerializeField] private Color normalTimeColor = new(0.65f, 0.85f, 0.95f);
        [SerializeField] private Color cautionTimeColor = new(0.95f, 0.90f, 0.72f);
        [SerializeField] private Color warningTimeColor = new(0.95f, 0.67f, 0.42f);
        [SerializeField] private Color urgentTimeColor = new(0.78f, 0.34f, 0.34f);
        [SerializeField] private Color morningCameraColor = new(0.60f, 0.72f, 0.78f);
        [SerializeField] private Color eveningCameraColor = new(0.36f, 0.32f, 0.52f);

        public TMP_Text TimeText => timeText;
        public Image TimeDial => timeDial;
        public TMP_Text ObjectiveText => objectiveText;
        public TMP_Text StatusText => statusText;
        public TMP_Text ContextPrompt => contextPrompt;
        public TMP_Text NotificationText => notificationText;
        public GameObject ReturnActions => returnActions;
        public Button ReturnButton => returnButton;
        public Button ContinueExplorationButton => continueExplorationButton;
        public Transform[] ObjectiveSpawnPoints => objectiveSpawnPoints;
        public Color MorningCameraColor => morningCameraColor;
        public Color EveningCameraColor => eveningCameraColor;

        /// <summary>
        /// 남은 시간에 맞는 Inspector 설정 색상을 반환합니다.
        /// </summary>
        public Color TimeColor(int remainingMinutes) => remainingMinutes switch
        {
            > 180 => normalTimeColor,
            > 60 => cautionTimeColor,
            > 30 => warningTimeColor,
            _ => urgentTimeColor
        };
    }
}
