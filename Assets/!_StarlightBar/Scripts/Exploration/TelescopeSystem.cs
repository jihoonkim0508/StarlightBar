using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace StarlightBar.Exploration
{
    /// <summary>
    /// 망원경으로 탐지 가능한 흔적이 구현하는 계약입니다.
    /// </summary>
    public interface ITelescopeDetectable
    {
        bool IsAnalyzed { get; }
        void SetHighlighted(bool highlighted);
        void Analyze();
    }

    /// <summary>
    /// 원형 망원경 UI가 켜진 동안 화면 안의 별자리 흔적을 강조하고 분석합니다.
    /// </summary>
    public sealed class TelescopeController : MonoBehaviour
    {
        [SerializeField, Tooltip("망원경 포인터를 월드 좌표로 변환할 카메라입니다.")]
        private Camera worldCamera;
        [SerializeField, Tooltip("망원경을 열 때 표시할 원형 마스크 UI입니다.")]
        private CanvasGroup telescopeOverlay;
        [SerializeField, Tooltip("망원경 열기와 닫기에 사용할 Input Actions 참조입니다.")]
        private InputActionReference toggleAction;
        [SerializeField, Tooltip("조준한 흔적 분석에 사용할 Input Actions 참조입니다.")]
        private InputActionReference analyzeAction;
        [SerializeField, Tooltip("망원경 화면 닫기에 사용할 Input Actions 참조입니다.")]
        private InputActionReference closeAction;
        [SerializeField, Tooltip("망원경으로 탐지 가능한 오브젝트 레이어입니다.")]
        private LayerMask detectableMask = ~0;
        [SerializeField, Tooltip("포인터 주위에서 흔적을 감지할 월드 반경입니다.")]
        private float detectionRadius = 1f;

        private readonly HashSet<ITelescopeDetectable> highlighted = new();
        public bool IsOpen { get; private set; }

        private void OnEnable()
        {
            toggleAction?.action.Enable();
            analyzeAction?.action.Enable();
            closeAction?.action.Enable();
            if (toggleAction != null) toggleAction.action.performed += Toggle;
            if (analyzeAction != null) analyzeAction.action.performed += Analyze;
            if (closeAction != null) closeAction.action.performed += Close;
            ApplyOverlay();
        }

        private void OnDisable()
        {
            if (toggleAction != null) toggleAction.action.performed -= Toggle;
            if (analyzeAction != null) analyzeAction.action.performed -= Analyze;
            if (closeAction != null) closeAction.action.performed -= Close;
            toggleAction?.action.Disable();
            analyzeAction?.action.Disable();
            closeAction?.action.Disable();
            ClearHighlights();
        }

        private void Update()
        {
            if (!IsOpen || worldCamera == null)
                return;

            ClearHighlights();
            var world = worldCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            foreach (var hit in Physics2D.OverlapCircleAll(world, detectionRadius, detectableMask))
            {
                var detectable = hit.GetComponentInParent<ITelescopeDetectable>();
                if (detectable == null) continue;
                detectable.SetHighlighted(true);
                highlighted.Add(detectable);
            }
        }

        private void Toggle(InputAction.CallbackContext context)
        {
            IsOpen = !IsOpen;
            ApplyOverlay();
        }

        private void Close(InputAction.CallbackContext context)
        {
            if (!IsOpen) return;
            IsOpen = false;
            ApplyOverlay();
        }

        private void Analyze(InputAction.CallbackContext context)
        {
            if (!IsOpen) return;
            foreach (var detectable in highlighted)
                detectable.Analyze();
        }

        private void ClearHighlights()
        {
            foreach (var detectable in highlighted)
                detectable.SetHighlighted(false);
            highlighted.Clear();
        }

        private void ApplyOverlay()
        {
            if (telescopeOverlay == null) return;
            telescopeOverlay.alpha = IsOpen ? 1f : 0f;
            telescopeOverlay.interactable = IsOpen;
            telescopeOverlay.blocksRaycasts = IsOpen;
        }
    }

}
