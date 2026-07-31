using System;
using StarlightBar.Content;
using StarlightBar.Core;
using StarlightBar.Exploration;
using UnityEngine;

namespace StarlightBar.Gameplay
{
    /// <summary>
    /// 혜화동에 배치된 조사·채집·가구 획득 목표를 나타냅니다.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(TelescopeDetectable))]
    public sealed class WorldObjectiveMarker : MonoBehaviour, IInteractable
    {
        public event Action<WorldObjectiveMarker> InteractionRequested;

        public ObjectiveDefinition Definition { get; private set; }
        public bool RequiresTelescope { get; private set; }
        public bool RequiresTalk { get; private set; }
        public string InteractionLabel =>
            Definition == null
                ? string.Empty
                : $"{(RequiresTalk ? "E 대화" : "F 조사")} · {Definition.title}";
        public bool IsTelescopeAnalyzed =>
            !RequiresTelescope || GetComponent<TelescopeDetectable>()?.IsAnalyzed == true;

        private SpriteRenderer targetRenderer;
        [Header("에디터 조정 표시")]
        [SerializeField] private Color requiredColor = new(0.25f, 0.9f, 1f);
        [SerializeField] private Color optionalColor = new(0.95f, 0.78f, 0.25f);
        [SerializeField] private Color hiddenTraceColor = new(0.25f, 0.9f, 1f, 0.18f);
        [SerializeField] private Color guidanceColor = new(0.75f, 0.95f, 1f, 1f);
        [SerializeField] private float guidanceScale = 1.35f;

        /// <summary>
        /// 챕터 목표 데이터와 지도 표시색을 적용해 상호작용 가능한 조사 대상을 초기화합니다.
        /// </summary>
        public void Initialize(ObjectiveDefinition definition)
        {
            Definition = definition;
            RequiresTalk = definition.type is ObjectiveType.HumanLifeTrace or ObjectiveType.SpecialDialogue;
            gameObject.name = $"Objective_{definition.id}";
            var renderer = GetComponent<SpriteRenderer>();
            targetRenderer = renderer;
            renderer.color = definition.mandatory ? requiredColor : optionalColor;
            GetComponent<CircleCollider2D>().isTrigger = true;
            RequiresTelescope = definition.type is ObjectiveType.MythEvidence or ObjectiveType.AntagonistEvidence;
            if (RequiresTelescope)
            {
                renderer.color = hiddenTraceColor;
                GetComponent<TelescopeDetectable>().Initialize(renderer);
            }
        }

        /// <summary>
        /// 시간 안전장치가 안내하는 목표에 프리팹 설정 강조값을 적용합니다.
        /// </summary>
        public void ShowGuidance()
        {
            if (targetRenderer != null)
                targetRenderer.color = guidanceColor;
            transform.localScale *= guidanceScale;
        }

        /// <summary>
        /// 목표가 유효하고 망원경 선행 분석 조건을 충족했는지 확인합니다.
        /// </summary>
        public bool CanInteract(GameObject actor)
        {
            return actor != null && Definition != null && IsTelescopeAnalyzed;
        }

        /// <summary>
        /// 공통 상호작용 컨트롤러의 요청을 낮 탐색 화면에 전달합니다.
        /// </summary>
        public void Interact(GameObject actor)
        {
            if (CanInteract(actor))
                InteractionRequested?.Invoke(this);
        }

        private void LateUpdate()
        {
            if (targetRenderer != null)
                targetRenderer.sortingOrder = -Mathf.RoundToInt(transform.position.y * 100f);
        }
    }
}
