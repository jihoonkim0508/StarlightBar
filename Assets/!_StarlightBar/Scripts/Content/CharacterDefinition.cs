using System.Collections.Generic;
using UnityEngine;

namespace StarlightBar.Content
{
    /// <summary>
    /// 캐릭터의 표시 정보와 손님 상태 범위를 정의합니다.
    /// </summary>
    [CreateAssetMenu(menuName = "별빛주점/캐릭터", fileName = "Character_")]
    public sealed class CharacterDefinition : ScriptableObject
    {
        [Tooltip("사람이 읽을 수 있고 프로젝트 전체에서 중복되지 않는 캐릭터 ID입니다.")]
        public string id;
        [Tooltip("한국어 UI와 대화에 표시할 캐릭터 이름입니다.")]
        public string displayName;
        [Tooltip("현재 생활, 감정 주제와 트라우마 반응을 포함한 인물 설명입니다.")]
        [TextArea(2, 6)] public string description;
        [Tooltip("대화창과 기록 화면에 사용할 캐릭터 초상화입니다.")]
        public Sprite portrait;
        [Tooltip("손님 시각 요소와 기억공간에 사용할 대표 색입니다.")]
        public Color themeColor = Color.white;
        [Tooltip("배치 시 신뢰와 안정에 긍정적으로 작용하는 가구 속성입니다.")]
        public List<FurnitureTrait> preferredFurnitureTraits = new();
        [Tooltip("배치 시 트라우마 반응이나 불안을 유발하는 가구 속성입니다.")]
        public List<FurnitureTrait> rejectedFurnitureTraits = new();
    }
}
