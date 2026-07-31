using System.Collections.Generic;
using UnityEngine;

namespace StarlightBar.Content
{
    /// <summary>
    /// 패스파인더 노트에 기록되는 증거 카드입니다.
    /// </summary>
    [CreateAssetMenu(menuName = "별빛주점/증거", fileName = "Evidence_")]
    public sealed class EvidenceDefinition : ScriptableObject
    {
        [Tooltip("프로젝트 전체에서 중복되지 않는 증거 카드 ID입니다.")]
        public string id;
        [Tooltip("패스파인더 노트에 표시할 증거 제목입니다.")]
        public string title;
        [Tooltip("조사 결과와 추리 근거를 설명하는 한국어 본문입니다.")]
        [TextArea(2, 6)] public string description;
        [Tooltip("증거가 정체, 신화, 감정, 생활 등의 어느 범주에 속하는지 지정합니다.")]
        public EvidenceCategory category;
        [Tooltip("증거 카드와 획득 알림에 사용할 아이콘입니다.")]
        public Sprite icon;
        [Tooltip("최종 추리 제출에 반드시 필요한 핵심 증거인지 지정합니다.")]
        public bool coreEvidence;
        [Tooltip("적대 세력이 변조한 오염 증거인지 지정합니다.")]
        public bool corrupted;
        [Tooltip("드래그 연결이 허용되는 다른 증거 ID 목록입니다.")]
        public List<string> allowedLinkEvidenceIds = new();
        [Tooltip("이 증거가 신뢰도를 높이는 별자리 후보 ID 목록입니다.")]
        public List<string> supportedCandidateIds = new();
        [Tooltip("이 증거가 후보에서 제외하는 별자리 ID 목록입니다.")]
        public List<string> excludedCandidateIds = new();
    }
}
