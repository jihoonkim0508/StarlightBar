using UnityEngine;

namespace StarlightBar.Gameplay
{
    /// <summary>
    /// 기억공간에서 회피해야 하는 상징적 감정 장애물입니다.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public sealed class MemoryObstacle : MonoBehaviour
    {
        private Vector3 origin;
        private Vector2 direction;
        private float distance;
        private float speed;

        /// <summary>
        /// 감정 장애물의 왕복 방향·거리·속도를 설정합니다.
        /// </summary>
        public void Initialize(Vector2 moveDirection, float moveDistance, float moveSpeed)
        {
            origin = transform.position;
            direction = moveDirection.normalized;
            distance = moveDistance;
            speed = moveSpeed;
            GetComponent<BoxCollider2D>().isTrigger = true;
        }

        private void Update()
        {
            transform.position = origin + (Vector3)(direction * (Mathf.Sin(Time.time * speed) * distance));
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<StarlightBar.Exploration.PlayerMover2D>() != null)
            {
                var presenter = Object.FindFirstObjectByType<MemorySpacePresenter>();
                if (presenter != null)
                    presenter.HandleObstacleHit();
                else
                    other.transform.position = Vector3.zero;
            }
        }
    }
}
