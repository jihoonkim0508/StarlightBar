using UnityEngine;

namespace StarlightBar.Content
{
    /// <summary>
    /// 낮 탐색에서 수행하는 필수 또는 선택 목표를 정의합니다.
    /// </summary>
    [CreateAssetMenu(menuName = "별빛주점/목표", fileName = "Objective_")]
    public sealed class ObjectiveDefinition : ScriptableObject
    {
        [Tooltip("프로젝트 전체에서 중복되지 않는 목표 ID입니다.")]
        public string id;
        [Tooltip("목표 HUD와 패스파인더 노트에 표시할 한국어 제목입니다.")]
        public string title;
        [Tooltip("플레이어가 해야 할 조사 행동과 목적을 설명합니다.")]
        [TextArea(2, 5)] public string description;
        [Tooltip("재료, 신화, 인물, 가구 등 목표 진행 유형입니다.")]
        public ObjectiveType type;
        [Tooltip("완료하지 않으면 낮 복귀가 제한되는 필수 목표인지 지정합니다.")]
        public bool mandatory = true;
        [Tooltip("목표 완료에 필요한 누적 상호작용 횟수입니다.")]
        [Min(1)] public int requiredCount = 1;
        [Tooltip("대화 또는 조사 완료 시 낮 시계에 더할 고정 시간입니다.")]
        [Min(0)] public int timeCostMinutes = 10;
        [Tooltip("완료 시 지급하거나 기록할 재료·증거·가구 콘텐츠 ID입니다.")]
        public string targetContentId;
    }
}
