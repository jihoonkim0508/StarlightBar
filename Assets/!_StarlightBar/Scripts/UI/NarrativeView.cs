using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarlightBar.UI
{
    /// <summary>
    /// 프롤로그와 엔딩 씬에서 직접 편집하는 서사 화면 참조입니다.
    /// </summary>
    public sealed class NarrativeView : MonoBehaviour
    {
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text body;
        [SerializeField] private TMP_Text status;
        [SerializeField] private Button advanceButton;
        [SerializeField] private TMP_Text advanceButtonLabel;

        public TMP_Text Title => title;
        public TMP_Text Body => body;
        public TMP_Text Status => status;
        public Button AdvanceButton => advanceButton;
        public TMP_Text AdvanceButtonLabel => advanceButtonLabel;
    }
}
