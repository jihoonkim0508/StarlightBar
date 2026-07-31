using System;
using StarlightBar.Core;
using StarlightBar.Exploration;
using UnityEngine;

namespace StarlightBar.Gameplay
{
    /// <summary>
    /// 기억 파편의 서사 역할을 구분합니다.
    /// </summary>
    public enum MemoryFragmentRole
    {
        Truth, KeyMemory, Acceptance, Corrupted
    }

    /// <summary>
    /// 기억공간에서 조사하거나 보호하는 편집 가능한 기억 파편입니다.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(TelescopeDetectable))]
    public sealed class MemoryFragmentMarker : MonoBehaviour, IInteractable
    {
        [Header("에디터에서 조정하는 상태 표시")]
        [SerializeField] private Color trueMemoryColor = new(0.65f, 0.18f, 0.78f, 1f);
        [SerializeField] private Color hiddenMemoryColor = new(0.65f, 0.18f, 0.78f, 0.3f);
        [SerializeField] private Color falseMemoryColor = new(0.28f, 0.12f, 0.36f, 0.3f);
        [SerializeField] private Color protectedMemoryColor = new(0.40f, 0.92f, 0.88f, 1f);
        [SerializeField] private float protectedScale = 1.18f;

        public event Action<MemoryFragmentMarker> InteractionRequested;
        public string ObjectiveId { get; private set; }
        public bool IsFalseMemory { get; private set; }
        public bool RequiresTelescope { get; private set; }
        public MemoryFragmentRole Role { get; private set; }
        public bool IsProtected { get; private set; }
        public string InteractionLabel =>
            Role == MemoryFragmentRole.KeyMemory && !IsProtected
                ? "E 핵심 기억 보호"
                : "F 기억 파편 정화";
        public bool IsAnalyzed =>
            !RequiresTelescope || GetComponent<TelescopeDetectable>()?.IsAnalyzed == true;

        /// <summary>
        /// 목표 ID와 진위, 망원경 조건 및 기억 역할을 연결합니다.
        /// </summary>
        public void Initialize(
            string objectiveId, bool isFalseMemory = false, bool requiresTelescope = false,
            MemoryFragmentRole role = MemoryFragmentRole.Truth)
        {
            ObjectiveId = objectiveId;
            IsFalseMemory = isFalseMemory;
            RequiresTelescope = requiresTelescope || isFalseMemory;
            Role = isFalseMemory ? MemoryFragmentRole.Corrupted : role;
            gameObject.name = isFalseMemory ? "MemoryFragment_False" : $"MemoryFragment_{objectiveId}";
            var renderer = GetComponent<SpriteRenderer>();
            renderer.color = IsFalseMemory
                ? falseMemoryColor
                : RequiresTelescope ? hiddenMemoryColor : trueMemoryColor;
            GetComponent<CircleCollider2D>().isTrigger = true;
            if (RequiresTelescope)
                GetComponent<TelescopeDetectable>().Initialize(renderer);
        }

        /// <summary>
        /// 파편이 활성 상태이고 필요한 분석이 끝났는지 확인합니다.
        /// </summary>
        public bool CanInteract(GameObject actor) =>
            actor != null && isActiveAndEnabled && IsAnalyzed;

        /// <summary>
        /// 기억공간 Presenter에 상호작용 요청을 전달합니다.
        /// </summary>
        public void Interact(GameObject actor)
        {
            if (CanInteract(actor))
                InteractionRequested?.Invoke(this);
        }

        /// <summary>
        /// 핵심 기억에 보호 상태의 외형과 크기를 적용합니다.
        /// </summary>
        public void Protect()
        {
            if (Role != MemoryFragmentRole.KeyMemory)
                return;
            IsProtected = true;
            var renderer = GetComponent<SpriteRenderer>();
            if (renderer != null)
                renderer.color = protectedMemoryColor;
            transform.localScale *= protectedScale;
        }
    }
}
