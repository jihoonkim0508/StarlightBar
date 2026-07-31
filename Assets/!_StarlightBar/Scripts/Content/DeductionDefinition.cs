using System.Collections.Generic;
using UnityEngine;

namespace StarlightBar.Content
{
    /// <summary>
    /// 별자리와 신화 정답 및 제출에 필요한 핵심 증거를 정의합니다.
    /// </summary>
    [CreateAssetMenu(menuName = "별빛주점/추리", fileName = "Deduction_")]
    public sealed class DeductionDefinition : ScriptableObject
    {
        [Tooltip("프로젝트 전체에서 중복되지 않는 추리 정의 ID입니다.")]
        public string id;
        [Tooltip("정답으로 제출해야 하는 별자리 콘텐츠 ID입니다.")]
        public string correctZodiacId;
        [Tooltip("정답으로 제출해야 하는 신화 사건 콘텐츠 ID입니다.")]
        public string correctMythId;
        [Tooltip("정답 제출에 반드시 포함해야 하는 핵심 증거 ID 목록입니다.")]
        public List<string> requiredCoreEvidenceIds = new();
        [Tooltip("플레이어에게 제시할 별자리 정답과 오답 후보 목록입니다.")]
        public List<string> zodiacCandidateIds = new();
        [Tooltip("플레이어에게 제시할 신화 정답과 오답 후보 목록입니다.")]
        public List<string> mythCandidateIds = new();
    }
}
