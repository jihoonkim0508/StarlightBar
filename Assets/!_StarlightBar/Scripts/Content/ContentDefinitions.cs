using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarlightBar.Content
{
    /// <summary>
    /// 증거가 어떤 관점의 단서인지 구분합니다.
    /// </summary>
    public enum EvidenceCategory
    {
        Identity, Myth, Emotion, HumanLife, FoodReaction, InteriorReaction, DocumentAndRumor
    }

    /// <summary>
    /// 목표의 필수 여부와 진행 성격을 구분합니다.
    /// </summary>
    public enum ObjectiveType
    {
        RequiredIngredient, MythEvidence, HumanLifeTrace, ReturnCondition, OptionalEvidence,
        Furniture, SpecialDialogue, AntagonistEvidence, RecipeQuality
    }

    /// <summary>
    /// 가구가 손님에게 전달하는 정서적 속성입니다.
    /// </summary>
    public enum FurnitureTrait
    {
        Stability, Memory, Vitality, Calm, Connection, Mystery
    }

    /// <summary>
    /// 조리 선택의 종류를 나타냅니다.
    /// </summary>
    public enum CookingMethod
    {
        Raw, Slice, Grind, Marinate, Boil, Steam, Bake, Grill, StirFry, Infuse, Chill
    }

    /// <summary>
    /// 조리 결과 품질입니다.
    /// </summary>
    public enum CookingQuality { Low, Medium, High }

    /// <summary>
    /// 별자리 전용 음식이 손님과 기억 복원에 주는 서사 효과입니다.
    /// </summary>
    public enum CookingEffect
    {
        Stability, Trust, Memory, Truth, Empathy, Connection
    }

    /// <summary>
    /// 추리 후보의 신뢰도 표시 단계입니다.
    /// </summary>
    public enum CandidateConfidence { Excluded, Low, Medium, High }

    /// <summary>
    /// 화자·표정·증거 연결과 선택지를 포함하는 한 줄의 대사 데이터입니다.
    /// </summary>
    [Serializable]
    public sealed class DialogueLine
    {
        [Tooltip("대화 정의 안에서 중복되지 않는 대사 노드 ID입니다.")]
        public string id;
        [Tooltip("한국어 표시명으로 변환할 화자 캐릭터 ID입니다.")]
        public string speakerId;
        [Tooltip("플레이어 이름과 조사 토큰을 포함할 수 있는 한국어 대사 본문입니다.")]
        [TextArea(2, 6)] public string text;
        [Tooltip("초상화 옆에 표시할 표정 또는 연출 콘텐츠 ID입니다.")]
        public string expressionId;
        [Tooltip("이 대사가 표시될 때 자동으로 기록할 증거 ID입니다.")]
        public string evidenceId;
        [Tooltip("플레이어가 선택할 수 있는 다음 대사 분기 목록입니다.")]
        public List<DialogueChoice> choices = new();
    }

    /// <summary>
    /// 다음 대사 분기와 손님 상태 변화를 포함하는 대화 선택지입니다.
    /// </summary>
    [Serializable]
    public sealed class DialogueChoice
    {
        [Tooltip("대화 정의 안에서 중복되지 않는 선택지 ID입니다.")]
        public string id;
        [Tooltip("플레이어에게 표시할 한국어 선택지 문구입니다.")]
        [TextArea(1, 3)] public string text;
        [Tooltip("선택한 뒤 이동할 다음 대사 노드 ID입니다.")]
        public string nextLineId;
        [Tooltip("선택 시 패스파인더 노트에 기록할 증거 ID입니다.")]
        public string evidenceId;
        [Tooltip("선택으로 변화하는 손님 신뢰 내부 값입니다.")]
        public int trustDelta;
        [Tooltip("선택으로 변화하는 손님 안정 내부 값입니다.")]
        public int stabilityDelta;
    }

    /// <summary>
    /// 재료와 손질법 및 순서를 정의하는 개별 조리 단계입니다.
    /// </summary>
    [Serializable]
    public sealed class RecipeStep
    {
        [Tooltip("이 단계에서 선택해야 하는 재료 콘텐츠 ID입니다.")]
        public string ingredientId;
        [Tooltip("재료에 적용할 손질 또는 조리 방법입니다.")]
        public CookingMethod method;
        [Tooltip("0부터 시작하는 정답 조리 순서입니다.")]
        [Min(0)] public int order;
        [Tooltip("품질 판정과 제출 가능 여부에 반드시 필요한 단계인지 지정합니다.")]
        public bool required = true;
    }
}
