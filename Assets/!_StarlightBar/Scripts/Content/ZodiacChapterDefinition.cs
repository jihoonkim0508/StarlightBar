using System.Collections.Generic;
using UnityEngine;

namespace StarlightBar.Content
{
    /// <summary>
    /// 하루 동안 진행되는 별자리 챕터의 모든 콘텐츠 참조를 묶습니다.
    /// </summary>
    [CreateAssetMenu(menuName = "별빛주점/별자리 챕터", fileName = "Chapter_")]
    public sealed class ZodiacChapterDefinition : ScriptableObject
    {
        [Tooltip("프로젝트 전체에서 중복되지 않는 챕터 ID입니다.")]
        public string id;
        [Tooltip("0부터 11까지의 정식 진행 순서입니다.")]
        [Range(0, 11)] public int chapterIndex;
        [Tooltip("챕터 화면과 기록 보관소에 표시할 한국어 제목입니다.")]
        public string title;
        [Tooltip("추리와 기억공간의 근거가 되는 신화 원전 또는 사건 요약입니다.")]
        [TextArea(2, 6)] public string mythologySource;
        [Tooltip("현재 챕터가 다루는 핵심 상처와 감정 주제입니다.")]
        [TextArea(1, 4)] public string emotionalTheme;
        [Tooltip("별자리 손님이 인간으로 살아가는 현재 직업과 관계입니다.")]
        [TextArea(2, 5)] public string currentLife;
        [Tooltip("원전의 상처가 지상에서 나타나는 구체적인 트라우마 반응입니다.")]
        [TextArea(2, 5)] public string traumaReaction;
        [Tooltip("이 챕터에 방문하는 별자리 손님 정의입니다.")]
        public CharacterDefinition guest;
        [Tooltip("낮 탐색에서 활성화할 필수·선택 목표 목록입니다.")]
        public List<ObjectiveDefinition> objectives = new();
        [Tooltip("조사, 음식, 가구와 대화에서 획득할 증거 목록입니다.")]
        public List<EvidenceDefinition> evidence = new();
        [Tooltip("이 챕터에서 선택적으로 획득할 수 있는 영구 가구 목록입니다.")]
        public List<FurnitureDefinition> obtainableFurniture = new();
        [Tooltip("아침 브리핑에서 재생할 한국어 대화입니다.")]
        public DialogueDefinition briefingDialogue;
        [Tooltip("야간 손님 입장과 음식 전후에 재생할 한국어 대화입니다.")]
        public DialogueDefinition nightDialogue;
        [Tooltip("손님의 기억 반응을 여는 별자리 전용 레시피입니다.")]
        public RecipeDefinition specialRecipe;
        [Tooltip("별자리, 신화 사건과 핵심 증거 제출 조건입니다.")]
        public DeductionDefinition deduction;
        [Tooltip("정답 추리 뒤 진입하는 기억공간 목표와 기믹입니다.")]
        public MemorySpaceDefinition memorySpace;
        [Tooltip("완전 복원 등급에서 기록할 손님별 결과 문구입니다.")]
        [TextArea(2, 5)] public string completeRestorationText;
        [Tooltip("부분 복원 등급에서 기록할 손님별 결과 문구입니다.")]
        [TextArea(2, 5)] public string partialRestorationText;
        [Tooltip("불안정 복원 등급에서 기록할 손님별 결과 문구입니다.")]
        [TextArea(2, 5)] public string unstableRestorationText;
        [Tooltip("손님이 복원된 별자리로 하늘에 돌아갈 때 기록할 문구입니다.")]
        [TextArea(2, 5)] public string returnToSkyChoiceText;
        [Tooltip("별의 기억을 간직한 인간으로 남을 때 기록할 문구입니다.")]
        [TextArea(2, 5)] public string remainHumanWithMemoriesChoiceText;
        [Tooltip("천상의 정체성을 놓고 인간으로 남을 때 기록할 문구입니다.")]
        [TextArea(2, 5)] public string remainHumanWithoutIdentityChoiceText;
    }
}
