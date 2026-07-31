using TMPro;
using UnityEngine;

namespace StarlightBar.UI
{
    /// <summary>
    /// 메인 메뉴 기록 보관소에서 별자리 한 명의 복원 결과를 표시합니다.
    /// </summary>
    public sealed class ArchiveEntryView : MonoBehaviour
    {
        [SerializeField, Tooltip("별자리 이름과 복원 결과를 표시하는 텍스트입니다.")]
        private TMP_Text label;

        /// <summary>
        /// 기록 행의 표시 문구만 갱신하며 폰트와 레이아웃은 프리팹 값을 유지합니다.
        /// </summary>
        public void SetText(string value)
        {
            if (label != null)
                label.text = value;
        }
    }
}
