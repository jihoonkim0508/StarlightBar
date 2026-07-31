using System.Collections.Generic;
using System.Linq;
using StarlightBar.Core;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace StarlightBar.Exploration
{
    /// <summary>
    /// 플레이어 주변에서 가장 가까운 상호작용 대상을 선택하고 안내 문구를 표시합니다.
    /// </summary>
    public sealed class InteractionController : MonoBehaviour
    {
        [SerializeField, Tooltip("상호작용 후보를 검색할 반경입니다.")]
        private float radius = 1.5f;
        [SerializeField, Tooltip("상호작용 가능한 레이어입니다.")]
        private LayerMask interactableMask = ~0;
        [SerializeField, Tooltip("F 조사 또는 E 대화 액션 참조입니다.")]
        private InputActionReference interactAction;
        [SerializeField, Tooltip("화면 하단 상호작용 안내 텍스트입니다.")]
        private TMP_Text promptText;

        private IInteractable current;

        private void OnEnable()
        {
            interactAction?.action.Enable();
            if (interactAction != null)
                interactAction.action.performed += OnInteract;
        }

        private void OnDisable()
        {
            if (interactAction != null)
                interactAction.action.performed -= OnInteract;
            interactAction?.action.Disable();
        }

        private void Update()
        {
            current = FindNearest();
            if (promptText != null)
                promptText.text = current == null ? string.Empty : current.InteractionLabel;
        }

        private IInteractable FindNearest()
        {
            var results = Physics2D.OverlapCircleAll(transform.position, radius, interactableMask);
            var candidates = new List<(IInteractable target, float distance)>();
            for (var index = 0; index < results.Length; index++)
            {
                var target = results[index].GetComponentInParent<IInteractable>();
                if (target != null && target.CanInteract(gameObject))
                    candidates.Add((target, Vector2.Distance(transform.position, results[index].transform.position)));
            }
            return candidates.OrderBy(item => item.distance).Select(item => item.target).FirstOrDefault();
        }

        private void OnInteract(InputAction.CallbackContext context)
        {
            current?.Interact(gameObject);
        }
    }
}
