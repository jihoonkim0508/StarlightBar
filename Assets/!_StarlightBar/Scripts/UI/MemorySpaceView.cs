using TMPro;
using UnityEngine;

namespace StarlightBar.UI
{
    /// <summary>
    /// 기억공간 씬의 고정 HUD와 배치 지점 참조입니다.
    /// </summary>
    public sealed class MemorySpaceView : MonoBehaviour
    {
        [SerializeField] private TMP_Text objectiveText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private Transform playerSpawnPoint;
        [SerializeField] private Transform fragmentRoot;

        public TMP_Text ObjectiveText => objectiveText;
        public TMP_Text StatusText => statusText;
        public Transform PlayerSpawnPoint => playerSpawnPoint;
        public Transform FragmentRoot => fragmentRoot;
    }
}
