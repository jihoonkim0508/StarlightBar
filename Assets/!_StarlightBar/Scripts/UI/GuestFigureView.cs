using UnityEngine;

namespace StarlightBar.UI
{
    /// <summary>
    /// 별자리 손님 한 명의 편집 가능한 월드 외형을 보관합니다.
    /// </summary>
    public sealed class GuestFigureView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Vector3 relaxedScale = new(0.68f, 1.05f, 1f);
        [SerializeField] private Vector3 tenseScale = new(0.68f, 0.98f, 1f);
        [SerializeField] private Vector3 distressedScale = new(0.68f, 0.88f, 1f);

        public Vector3 RelaxedScale => relaxedScale;
        public Vector3 TenseScale => tenseScale;
        public Vector3 DistressedScale => distressedScale;

        /// <summary>
        /// 캐릭터 데이터가 제공하는 테마색만 현재 외형에 적용합니다.
        /// </summary>
        public void Bind(Color themeColor) => spriteRenderer.color = themeColor;
    }
}
