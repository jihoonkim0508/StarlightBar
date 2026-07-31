using TMPro;
using UnityEngine;

namespace StarlightBar.Gameplay
{
    /// <summary>
    /// 프리팹 텍스트의 원래 크기를 보관해 접근성 배율을 누적 오차 없이 적용합니다.
    /// </summary>
    public sealed class RuntimeTextScale : MonoBehaviour
    {
        private TMP_Text target;
        private float baseSize;

        /// <summary>
        /// 텍스트와 원래 글자 크기를 등록합니다.
        /// </summary>
        public void Initialize(TMP_Text text, float size)
        {
            target = text;
            baseSize = size;
        }

        /// <summary>
        /// 접근성 글자 배율을 원래 크기에 적용합니다.
        /// </summary>
        public void Apply(float scale)
        {
            if (target != null)
                target.fontSize = Mathf.Clamp(baseSize * scale, 12f, 96f);
        }
    }
}
