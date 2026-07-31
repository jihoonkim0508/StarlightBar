using System.Collections.Generic;
using UnityEngine;

namespace StarlightBar.Content
{
    /// <summary>
    /// 주점에 배치 가능한 가구와 정서 속성을 정의합니다.
    /// </summary>
    [CreateAssetMenu(menuName = "별빛주점/가구", fileName = "Furniture_")]
    public sealed class FurnitureDefinition : ScriptableObject
    {
        [Tooltip("프로젝트 전체에서 중복되지 않는 가구 ID입니다.")]
        public string id;
        [Tooltip("획득 알림과 배치 UI에 표시할 한국어 가구 이름입니다.")]
        public string displayName;
        [Tooltip("주점 배치 화면에 표시할 가구 스프라이트입니다.")]
        public Sprite sprite;
        [Tooltip("배치 영역에서 가구가 차지하는 가로·세로 기준 크기입니다.")]
        public Vector2 footprint = Vector2.one;
        [Tooltip("손님의 선호·기피 반응 계산에 사용하는 정서 속성입니다.")]
        public List<FurnitureTrait> traits = new();
    }
}
