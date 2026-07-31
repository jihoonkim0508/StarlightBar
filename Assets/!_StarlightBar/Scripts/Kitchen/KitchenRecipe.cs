using System;
using UnityEngine;

namespace StarlightBar.Gameplay
{
    /// <summary>
    /// 아래 음식과 위 음식을 겹쳐 만드는 Kitchen 배치 레시피를 정의합니다.
    /// </summary>
    [Serializable]
    public sealed class KitchenRecipe
    {
        [SerializeField, Tooltip("먼저 배치되어 아래에 있어야 하는 음식 ID입니다.")]
        private string bottomItemId;
        [SerializeField, Tooltip("나중에 배치하거나 움직여 위에 있어야 하는 음식 ID입니다.")]
        private string topItemId;
        [SerializeField, Tooltip("레시피 완성 후 생성되는 음식 ID입니다.")]
        private string resultItemId;
        [SerializeField, Tooltip("Hierarchy에 표시할 완성 음식의 한국어 이름입니다.")]
        private string resultDisplayName;
        [SerializeField, Tooltip("완성 음식에 사용할 Sprite입니다.")]
        private Sprite resultSprite;
        [SerializeField, Tooltip("완성 음식의 월드 표시 크기입니다.")]
        private Vector3 resultScale = Vector3.one;
        [SerializeField, Range(0.01f, 1f),
         Tooltip("위 음식 면적 중 아래 음식과 겹쳐야 하는 최소 비율입니다.")]
        private float requiredTopOverlapRatio = 0.5f;

        internal string ResultItemId => resultItemId;
        internal string ResultDisplayName => resultDisplayName;
        internal Sprite ResultSprite => resultSprite;
        internal Vector3 ResultScale => resultScale;
        internal float RequiredTopOverlapRatio => requiredTopOverlapRatio;

        internal bool Matches(string bottomId, string topId) =>
            bottomItemId == bottomId && topItemId == topId;
    }
}
