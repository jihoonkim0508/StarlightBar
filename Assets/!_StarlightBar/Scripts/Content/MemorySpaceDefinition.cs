using System.Collections.Generic;
using UnityEngine;

namespace StarlightBar.Content
{
    /// <summary>
    /// 기억공간의 공통 목표와 별자리별 기믹 모듈을 정의합니다.
    /// </summary>
    [CreateAssetMenu(menuName = "별빛주점/기억공간", fileName = "MemorySpace_")]
    public sealed class MemorySpaceDefinition : ScriptableObject
    {
        [Tooltip("프로젝트 전체에서 중복되지 않는 기억공간 ID입니다.")]
        public string id;
        [Tooltip("공용 기억공간에 결합할 별자리별 배치·위험 패턴 ID입니다.")]
        public string sceneVariantId;
        [Tooltip("기억공간 배경과 오브젝트에 적용할 별자리 대표 색입니다.")]
        public Color palette = new(0.25f, 0.25f, 0.5f);
        [Tooltip("순서대로 정화해야 하는 기억 목표 ID 목록입니다.")]
        public List<string> objectiveIds = new();
        [Tooltip("각 기억 목표 ID와 같은 순서로 표시할 한국어 목표 문구입니다.")]
        public List<string> objectiveTitles = new();
        [Tooltip("기억공간에 활성화할 확장 기믹 모듈 ID 목록입니다.")]
        public List<string> mechanicModuleIds = new();
        [Tooltip("오염으로부터 보호해야 하는 핵심 기억 오브젝트 ID입니다.")]
        public string keyMemoryObjectId;
        [Tooltip("스토리를 영구 차단하지 않도록 허용하는 최대 재시도 횟수입니다.")]
        [Min(1)] public int allowedRetries = 99;
    }
}
