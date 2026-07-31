using StarlightBar.Gameplay;
using StarlightBar.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace StarlightBar.Exploration
{
    /// <summary>
    /// Rigidbody2D를 사용해 탑다운 8방향 이동과 애니메이션 값을 갱신합니다.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerMover2D : MonoBehaviour
    {
        [SerializeField, Tooltip("초당 이동 거리입니다.")]
        private float moveSpeed = 4f;
        [SerializeField, Tooltip("선택 사항인 8방향 캐릭터 Animator입니다.")]
        private Animator animator;
        [SerializeField, Tooltip("Input System의 Move 액션 참조입니다.")]
        private InputActionReference moveAction;
        [SerializeField, Tooltip("Animator가 없을 때 사용하는 에디터 배치형 8방향 표시기입니다.")]
        private TopDownCharacterAnimator fallbackAnimator;

        private Rigidbody2D body;
        private Vector2 moveInput;
        private Vector2 lastDirection = Vector2.down;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.gravityScale = 0f;
            body.freezeRotation = true;
        }

        private void OnEnable()
        {
            moveAction?.action.Enable();
        }

        private void OnDisable()
        {
            moveAction?.action.Disable();
        }

        private void Update()
        {
            if (RuntimeDialoguePresenter.AnyPlaying || SettingsMenuPresenter.AnyOpen ||
                PathfinderNotebookPresenter.AnyOpen || RuntimeTelescopePresenter.AnyOpen ||
                InvestigationDetailPresenter.AnyOpen)
            {
                moveInput = Vector2.zero;
                fallbackAnimator?.SetMovement(Vector2.zero, lastDirection);
                return;
            }
            moveInput = ReadKeyboardFallback();
            moveInput = Vector2.ClampMagnitude(moveInput, 1f);
            if (moveInput.sqrMagnitude > 0.001f)
                lastDirection = moveInput.normalized;
            fallbackAnimator?.SetMovement(moveInput, lastDirection);

            if (animator == null) return;
            animator.SetFloat("MoveX", moveInput.x);
            animator.SetFloat("MoveY", moveInput.y);
            animator.SetFloat("LastX", lastDirection.x);
            animator.SetFloat("LastY", lastDirection.y);
            animator.SetFloat("Speed", moveInput.sqrMagnitude);
        }

        private void FixedUpdate()
        {
            body.MovePosition(body.position + moveInput * (moveSpeed * Time.fixedDeltaTime));
        }

        private static Vector2 ReadKeyboardFallback()
        {
            var horizontal = (GameInput.IsPressed(GameInputAction.MoveRight) ? 1f : 0f) -
                             (GameInput.IsPressed(GameInputAction.MoveLeft) ? 1f : 0f);
            var vertical = (GameInput.IsPressed(GameInputAction.MoveUp) ? 1f : 0f) -
                           (GameInput.IsPressed(GameInputAction.MoveDown) ? 1f : 0f);
            return new Vector2(horizontal, vertical);
        }
    }

    /// <summary>
    /// 정식 스프라이트 시트가 없는 단계에서도 8방향 바라보기와 걷기 프레임 리듬을 표현합니다.
    /// Animator가 연결되면 PlayerMover2D의 파라미터 기반 애니메이션이 우선합니다.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class TopDownCharacterAnimator : MonoBehaviour
    {
        private SpriteRenderer targetRenderer;
        private Vector3 baseScale;
        private Vector2 movement;
        private Vector2 facing = Vector2.down;

        public int DirectionIndex { get; private set; }
        public bool IsMoving => movement.sqrMagnitude > 0.001f;

        private void Awake()
        {
            targetRenderer = GetComponent<SpriteRenderer>();
            baseScale = transform.localScale;
        }

        /// <summary>
        /// 현재 이동 벡터와 마지막 바라본 방향으로 8방향 애니메이션 파라미터를 갱신합니다.
        /// </summary>
        public void SetMovement(Vector2 move, Vector2 lastDirection)
        {
            movement = move;
            if (lastDirection.sqrMagnitude > 0.001f)
                facing = lastDirection;
            DirectionIndex = ResolveEightDirection(facing);
            targetRenderer.flipX = DirectionIndex is 1 or 2 or 3;
        }

        private void LateUpdate()
        {
            var phase = IsMoving ? Mathf.Sin(Time.time * 12f) : 0f;
            var bob = IsMoving ? Mathf.Abs(phase) * 0.045f : 0f;
            transform.localScale = new Vector3(
                baseScale.x * (1f + bob * 0.4f),
                baseScale.y * (1f - bob),
                baseScale.z);
            targetRenderer.sortingOrder = -Mathf.RoundToInt(transform.position.y * 100f);
        }

        private static int ResolveEightDirection(Vector2 direction)
        {
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            if (angle < 0f)
                angle += 360f;
            return Mathf.RoundToInt(angle / 45f) % 8;
        }
    }
}
