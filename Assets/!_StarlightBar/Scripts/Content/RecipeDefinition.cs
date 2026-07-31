using System.Collections.Generic;
using UnityEngine;

namespace StarlightBar.Content
{
    /// <summary>
    /// 별자리 전용 요리의 재료, 조리 순서, 장식, 마법 재료를 정의합니다.
    /// </summary>
    [CreateAssetMenu(menuName = "별빛주점/레시피", fileName = "Recipe_")]
    public sealed class RecipeDefinition : ScriptableObject
    {
        [Tooltip("프로젝트 전체에서 중복되지 않는 레시피 ID입니다.")]
        public string id;
        [Tooltip("조리 UI와 음식 기록에 표시할 한국어 메뉴 이름입니다.")]
        public string displayName;
        [Tooltip("수집한 증거를 바탕으로 보여 줄 기대 효과 힌트입니다.")]
        [TextArea(1, 4)] public string expectedEffectHint;
        [Tooltip("정답 조리 시 손님 상태와 기억에 적용되는 효과 목록입니다.")]
        public List<CookingEffect> effects = new();
        [Tooltip("재료, 손질법과 조리 순서를 정의하는 정답 단계 목록입니다.")]
        public List<RecipeStep> steps = new();
        [Tooltip("마무리 단계에서 선택해야 하는 장식 콘텐츠 ID입니다.")]
        public string decorationId;
        [Tooltip("기억 반응을 활성화하는 마법 재료 콘텐츠 ID입니다.")]
        public string magicalIngredientId;
        [Tooltip("중간 품질로 판정할 최소 정답 일치 점수입니다.")]
        [Range(0f, 1f)] public float mediumThreshold = 0.5f;
        [Tooltip("높은 품질로 판정할 최소 정답 일치 점수입니다.")]
        [Range(0f, 1f)] public float highThreshold = 0.85f;
        [Tooltip("완전히 무관한 음식 제출을 막기 위한 허용 재료 ID 목록입니다.")]
        public List<string> allowedIngredientIds = new();
    }
}
