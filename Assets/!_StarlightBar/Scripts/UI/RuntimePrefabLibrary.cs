using UnityEngine;

namespace StarlightBar.UI
{
    /// <summary>
    /// 런타임에서 반복 배치하는 UI와 월드 오브젝트의 편집 가능한 프리팹을 모아 둡니다.
    /// </summary>
    [CreateAssetMenu(menuName = "별빛주점/런타임 프리팹 라이브러리", fileName = "RuntimePrefabLibrary")]
    public sealed class RuntimePrefabLibrary : ScriptableObject
    {
        [Header("공용 UI")]
        [Tooltip("스크롤 패널과 Content 자식이 포함된 공용 패널 프리팹입니다.")]
        public GameObject panelPrefab;

        [Tooltip("TextMeshProUGUI와 LayoutElement가 포함된 공용 텍스트 프리팹입니다.")]
        public GameObject textPrefab;

        [Tooltip("Button과 Label 자식이 포함된 공용 버튼 프리팹입니다.")]
        public GameObject buttonPrefab;

        [Tooltip("TMP_InputField와 입력 텍스트·안내문 자식이 포함된 입력창 프리팹입니다.")]
        public GameObject inputFieldPrefab;

        [Header("월드 오브젝트")]
        [Tooltip("혜화동의 조사 목표를 표시하는 월드 프리팹입니다.")]
        public GameObject objectiveMarkerPrefab;

        [Tooltip("기억공간의 진짜 기억 파편 프리팹입니다.")]
        public GameObject trueMemoryFragmentPrefab;

        [Tooltip("기억공간의 거짓 기억 파편 프리팹입니다.")]
        public GameObject falseMemoryFragmentPrefab;

        [Tooltip("기억공간 플레이어 프리팹입니다.")]
        public GameObject memoryPlayerPrefab;

        [Tooltip("기억공간에서 대화하는 별자리 메아리 프리팹입니다.")]
        public GameObject memoryEchoPrefab;

        [Tooltip("기억공간의 움직이는 감정 장애물 프리팹입니다.")]
        public GameObject memoryHazardPrefab;

        private static RuntimePrefabLibrary instance;

        /// <summary>
        /// Resources에 저장된 프로젝트 공용 프리팹 라이브러리를 반환합니다.
        /// </summary>
        public static RuntimePrefabLibrary Instance =>
            instance != null
                ? instance
                : instance = Resources.Load<RuntimePrefabLibrary>("StarlightBar/RuntimePrefabLibrary");
    }
}
