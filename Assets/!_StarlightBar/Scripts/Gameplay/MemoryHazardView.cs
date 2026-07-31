using StarlightBar.Exploration;
using UnityEngine;

namespace StarlightBar.Gameplay
{
    /// <summary>
    /// 프리팹 외형을 유지하면서 기억공간 장애물의 이동 패턴만 수행합니다.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
    public sealed class MemoryHazardView : MonoBehaviour
    {
        private enum Pattern { Sweep, Chase, Orbit, Pulse }

        private Pattern pattern;
        private MemoryMechanicDirector director;
        private Transform target;
        private Vector2 direction;
        private Vector2 origin;
        private float speed;
        private float distance;
        private float radius;
        private float pulseDelay;
        private float createdAt;
        private SpriteRenderer targetRenderer;
        private Collider2D targetCollider;

        /// <summary>
        /// 지정 방향으로 반복 횡단하는 장애물로 초기화합니다.
        /// </summary>
        public void InitializeSweep(
            MemoryMechanicDirector owner, Vector2 moveDirection,
            float moveSpeed, float resetDistance)
        {
            SetCommon(owner, Pattern.Sweep);
            direction = moveDirection.normalized;
            speed = moveSpeed;
            distance = resetDistance;
            origin = transform.position;
        }

        /// <summary>
        /// 플레이어를 추적하는 장애물로 초기화합니다.
        /// </summary>
        public void InitializeChase(
            MemoryMechanicDirector owner, Transform chaseTarget, float moveSpeed)
        {
            SetCommon(owner, Pattern.Chase);
            target = chaseTarget;
            speed = moveSpeed;
        }

        /// <summary>
        /// 지정 중심을 공전하는 장애물로 초기화합니다.
        /// </summary>
        public void InitializeOrbit(
            MemoryMechanicDirector owner, Vector2 center,
            float orbitRadius, float orbitSpeed)
        {
            SetCommon(owner, Pattern.Orbit);
            origin = center;
            radius = orbitRadius;
            speed = orbitSpeed;
        }

        /// <summary>
        /// 경고 후 활성화되는 파동 장애물로 초기화합니다.
        /// </summary>
        public void InitializePulse(MemoryMechanicDirector owner, float warningTime)
        {
            SetCommon(owner, Pattern.Pulse);
            pulseDelay = warningTime;
            targetCollider.enabled = false;
        }

        private void SetCommon(MemoryMechanicDirector owner, Pattern hazardPattern)
        {
            director = owner;
            pattern = hazardPattern;
            createdAt = Time.time;
            targetRenderer = GetComponent<SpriteRenderer>();
            targetCollider = GetComponent<Collider2D>();
        }

        private void Update()
        {
            switch (pattern)
            {
                case Pattern.Sweep:
                    transform.position += (Vector3)(direction * (speed * Time.deltaTime));
                    if (Vector2.Distance(origin, transform.position) > distance)
                        transform.position = origin;
                    break;
                case Pattern.Chase:
                    if (target != null)
                        transform.position = Vector2.MoveTowards(
                            transform.position, target.position, speed * Time.deltaTime);
                    break;
                case Pattern.Orbit:
                    var angle = (Time.time - createdAt) * speed;
                    transform.position =
                        origin + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                    break;
                case Pattern.Pulse:
                    UpdatePulse();
                    break;
            }
        }

        private void UpdatePulse()
        {
            var elapsed = Time.time - createdAt;
            targetRenderer.color = elapsed < pulseDelay
                ? new Color(0.92f, 0.62f, 0.18f, 0.28f + elapsed / pulseDelay * 0.38f)
                : new Color(0.58f, 0.04f, 0.14f, 0.92f);
            targetCollider.enabled = elapsed >= pulseDelay;
            if (elapsed >= pulseDelay + 0.45f)
                Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<PlayerMover2D>() == null)
                return;
            director?.DamageBarrier(28f);
            AccessibleCameraEffectsPresenter.PlayImpact(0.09f, 0.16f);
            if (pattern == Pattern.Sweep)
                transform.position = origin;
        }
    }
}
