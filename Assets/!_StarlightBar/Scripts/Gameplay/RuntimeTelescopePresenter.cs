using System.Collections.Generic;
using StarlightBar.Core;
using StarlightBar.Exploration;
using StarlightBar.UI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace StarlightBar.Gameplay
{
    /// <summary>
    /// 1키로 여는 원형 망원경 화면, 포인터 탐지, 휠 줌과 좌클릭 분석을 제공합니다.
    /// </summary>
    public sealed class RuntimeTelescopePresenter : MonoBehaviour
    {
        private readonly HashSet<ITelescopeDetectable> highlighted = new();
        private Camera worldCamera;
        [SerializeField, Tooltip("씬에 배치된 망원경 오버레이 참조입니다.")]
        private TelescopeView view;
        private TMP_Text help;
        private RectTransform pointerVisual;
        private Vector2 virtualPointer;
        private bool isOpen;
        private float baseOrthographicSize;

        public bool IsOpen => isOpen;
        public static bool AnyOpen { get; private set; }

        private void Start()
        {
            worldCamera = Camera.main;
            if (worldCamera != null)
                baseOrthographicSize = worldCamera.orthographicSize;
            if (view != null)
            {
                help = view.HelpText;
                pointerVisual = view.Pointer;
            }
            SetOpen(false);
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            var mouse = Mouse.current;
            if (!isOpen && mouse?.leftButton.wasPressedThisFrame == true &&
                (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject()) &&
                !RuntimeDialoguePresenter.AnyPlaying && !SettingsMenuPresenter.AnyOpen &&
                !PathfinderNotebookPresenter.AnyOpen)
            {
                SetOpen(true);
                return;
            }
            if (GameInput.WasPressedThisFrame(GameInputAction.Telescope))
            {
                if (!isOpen && (RuntimeDialoguePresenter.AnyPlaying ||
                                SettingsMenuPresenter.AnyOpen ||
                                PathfinderNotebookPresenter.AnyOpen))
                    return;
                SetOpen(!isOpen);
            }
            if (!isOpen || mouse == null)
                return;

            if (mouse.rightButton.wasPressedThisFrame ||
                GameInput.WasPressedThisFrame(GameInputAction.Menu))
            {
                SetOpen(false);
                return;
            }

            ApplyZoom(mouse.scroll.ReadValue().y);
            var sensitivity = GameBootstrapper.Instance?.Session?.Data?.settings?.mouseSensitivity ?? 1f;
            virtualPointer += mouse.delta.ReadValue() * Mathf.Clamp(sensitivity, 0.5f, 2f);
            virtualPointer = new Vector2(
                Mathf.Clamp(virtualPointer.x, 0f, Screen.width),
                Mathf.Clamp(virtualPointer.y, 0f, Screen.height));
            UpdatePointerVisual();
            RefreshHighlights(virtualPointer);
            if (mouse.leftButton.wasPressedThisFrame)
                AnalyzeHighlighted();
        }

        private void OnDisable()
        {
            AnyOpen = false;
            ClearHighlights();
            RestoreZoom();
        }

        private void SetOpen(bool value)
        {
            isOpen = value;
            AnyOpen = value;
            view?.Root?.SetActive(value);
            if (value)
            {
                virtualPointer = Mouse.current?.position.ReadValue() ??
                                 new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
                UpdatePointerVisual();
            }
            if (!value)
            {
                ClearHighlights();
                RestoreZoom();
            }
        }

        private void UpdatePointerVisual()
        {
            if (pointerVisual == null || pointerVisual.parent is not RectTransform parent)
                return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parent, virtualPointer, null, out var localPoint);
            pointerVisual.anchoredPosition = localPoint;
        }

        private void RefreshHighlights(Vector2 pointerScreenPosition)
        {
            ClearHighlights();
            if (worldCamera == null)
                return;

            var world = worldCamera.ScreenToWorldPoint(pointerScreenPosition);
            foreach (var hit in Physics2D.OverlapCircleAll(world, 1.15f))
            {
                var detectable = hit.GetComponentInParent<ITelescopeDetectable>();
                if (detectable == null)
                    continue;
                detectable.SetHighlighted(true);
                highlighted.Add(detectable);
            }
        }

        private void AnalyzeHighlighted()
        {
            if (highlighted.Count == 0)
            {
                if (help != null)
                    help.text = "이 위치에서는 별자리 흔적이 감지되지 않습니다.";
                return;
            }

            foreach (var detectable in highlighted)
                detectable.Analyze();
            if (help != null)
                help.text = "은색 흔적을 분석했습니다. 망원경을 닫고 가까이에서 조사하세요.";
        }

        private void ApplyZoom(float scroll)
        {
            if (worldCamera == null || !worldCamera.orthographic || Mathf.Approximately(scroll, 0f))
                return;
            worldCamera.orthographicSize = Mathf.Clamp(worldCamera.orthographicSize - scroll * 0.0025f, 2.2f, 7.5f);
        }

        private void RestoreZoom()
        {
            if (worldCamera != null && worldCamera.orthographic && baseOrthographicSize > 0f)
                worldCamera.orthographicSize = baseOrthographicSize;
        }

        private void ClearHighlights()
        {
            foreach (var detectable in highlighted)
                detectable?.SetHighlighted(false);
            highlighted.Clear();
        }

    }
}
