using UnityEngine;

namespace StarlightBar.Exploration
{
    /// <summary>
    /// 일반·오염·기억 흔적에 공통으로 사용할 수 있는 기본 탐지 대상입니다.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class TelescopeDetectable : MonoBehaviour, ITelescopeDetectable
    {
        [SerializeField, Tooltip("망원경 조준 색을 적용할 대상 렌더러입니다.")]
        private SpriteRenderer targetRenderer;
        [SerializeField, Tooltip("별자리 흔적을 조준했을 때 사용할 은색·청록색 강조색입니다.")]
        private Color highlightColor = new(0.55f, 0.95f, 1f);
        [SerializeField, Tooltip("망원경 분석을 마친 뒤 활성화할 숨겨진 상호작용 대상입니다.")]
        private GameObject revealAfterAnalysis;
        [SerializeField, Tooltip("프리팹 자식에 배치된 망원경 흔적 파티클입니다.")]
        private ParticleSystem highlightParticles;
        private Color originalColor = Color.white;

        public bool IsAnalyzed { get; private set; }

        /// <summary>
        /// 조사 대상의 렌더러와 분석 후 표시할 오브젝트를 연결합니다.
        /// </summary>
        public void Initialize(SpriteRenderer renderer, GameObject revealObject = null)
        {
            targetRenderer = renderer;
            revealAfterAnalysis = revealObject;
            highlightParticles ??= GetComponentInChildren<ParticleSystem>(true);
            if (targetRenderer != null)
                originalColor = targetRenderer.color;
            if (revealAfterAnalysis != null)
                revealAfterAnalysis.SetActive(false);
            ConfigureParticles();
        }

        private void Awake()
        {
            targetRenderer ??= GetComponent<SpriteRenderer>();
            highlightParticles ??= GetComponentInChildren<ParticleSystem>(true);
            if (targetRenderer != null)
                originalColor = targetRenderer.color;
            if (revealAfterAnalysis != null)
                revealAfterAnalysis.SetActive(false);
        }

        /// <summary>
        /// 망원경 조준 여부에 따라 은색·청록색 강조 표시를 전환합니다.
        /// </summary>
        public void SetHighlighted(bool highlighted)
        {
            if (targetRenderer != null)
                targetRenderer.color = highlighted ? highlightColor : originalColor;
            if (highlightParticles == null)
                return;
            if (highlighted && !highlightParticles.isPlaying)
                highlightParticles.Play();
            else if (!highlighted && highlightParticles.isPlaying)
                highlightParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        /// <summary>
        /// 흔적을 분석 완료 상태로 만들고 숨겨진 상호작용 대상을 드러냅니다.
        /// </summary>
        public void Analyze()
        {
            IsAnalyzed = true;
            if (revealAfterAnalysis != null)
                revealAfterAnalysis.SetActive(true);
            highlightParticles?.Play();
        }

        private void ConfigureParticles()
        {
            if (highlightParticles == null)
                return;
            var main = highlightParticles.main;
            main.loop = true;
            main.startLifetime = 0.65f;
            main.startSpeed = 0.18f;
            main.startSize = 0.07f;
            main.startColor = highlightColor;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            var emission = highlightParticles.emission;
            emission.rateOverTime = 12f;
            var shape = highlightParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.55f;
            highlightParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
