using System;
using StarlightBar.Core;
using UnityEngine;

namespace StarlightBar.Gameplay
{
    /// <summary>
    /// 기억 속 별자리 인물의 대화를 여는 편집 가능한 메아리 프리팹입니다.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(CapsuleCollider2D))]
    public sealed class MemoryEchoMarker : MonoBehaviour, IInteractable
    {
        [SerializeField, Range(0f, 1f), Tooltip("챕터 팔레트에 적용할 메아리 불투명도입니다.")]
        private float paletteAlpha = 0.72f;

        public event Action<MemoryEchoMarker> InteractionRequested;
        public string InteractionLabel => "E 별자리의 목소리 듣기";

        /// <summary>
        /// 현재 기억공간 팔레트로 메아리 색상만 설정합니다.
        /// </summary>
        public void Initialize(Color color)
        {
            gameObject.name = "ConstellationMemoryEcho";
            GetComponent<SpriteRenderer>().color =
                new Color(color.r, color.g, color.b, paletteAlpha);
            GetComponent<CapsuleCollider2D>().isTrigger = true;
        }

        /// <summary>
        /// 메아리가 활성 상태이고 행위자가 존재하는지 확인합니다.
        /// </summary>
        public bool CanInteract(GameObject actor) =>
            actor != null && isActiveAndEnabled;

        /// <summary>
        /// 기억공간 Presenter에 대화 요청을 전달합니다.
        /// </summary>
        public void Interact(GameObject actor)
        {
            if (CanInteract(actor))
                InteractionRequested?.Invoke(this);
        }
    }
}
