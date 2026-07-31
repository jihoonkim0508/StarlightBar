using System.Collections.Generic;
using StarlightBar.Core;
using StarlightBar.Exploration;
using StarlightBar.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarlightBar.Gameplay
{
    /// <summary>
    /// 혜화동의 플레이어·필수 목표·선택 목표 위치와 현재 선택 도구를 텍스트·아이콘으로 함께 표시합니다.
    /// </summary>
    public sealed class ExplorationMiniMapPresenter : MonoBehaviour
    {
        [Header("에디터에서 배치한 미니맵")]
        [SerializeField] private ExplorationMiniMapView view;

        private const float MapHalfWidth = 9.5f;
        private const float MapHalfHeight = 5.5f;
        private RectTransform map;
        private RectTransform playerDot;
        private TMP_Text toolText;
        private readonly Dictionary<WorldObjectiveMarker, RectTransform> markerDots = new();

        private void Start()
        {
            if (view == null || view.MapArea == null || view.PlayerDot == null ||
                view.MarkerPrefab == null)
            {
                Debug.LogError("ExplorationMiniMapView 참조가 연결되지 않았습니다.", this);
                enabled = false;
                return;
            }
            map = view.MapArea;
            playerDot = view.PlayerDot;
            toolText = view.ToolText;
            RebuildMarkers();
        }

        private void Update()
        {
            if (map == null)
                return;

            var player = Object.FindFirstObjectByType<PlayerMover2D>();
            if (player != null)
                PlaceDot(playerDot, player.transform.position);

            foreach (var entry in markerDots)
            {
                if (entry.Key == null)
                {
                    if (entry.Value != null)
                        entry.Value.gameObject.SetActive(false);
                    continue;
                }
                PlaceDot(entry.Value, entry.Key.transform.position);
            }

            if (toolText != null)
            {
                var telescope = Object.FindFirstObjectByType<RuntimeTelescopePresenter>();
                toolText.text = telescope != null && telescope.IsOpen
                    ? "선택 도구  ① 망원경 [사용 중]"
                    : $"선택 도구  ① 망원경 · {GameInput.GetKey(GameInputAction.Telescope)}";
            }
        }

        private void RebuildMarkers()
        {
            foreach (var marker in Object.FindObjectsByType<WorldObjectiveMarker>(FindObjectsSortMode.None))
            {
                var dot = Instantiate(view.MarkerPrefab, map);
                dot.name = $"MapDot_{marker.Definition.id}";
                dot.Bind(marker.Definition.mandatory ? "필" : "선", marker.Definition.mandatory);
                markerDots[marker] = dot.Rect;
            }
        }

        private void PlaceDot(RectTransform dot, Vector3 worldPosition)
        {
            if (dot == null)
                return;
            dot.anchorMin = dot.anchorMax = new Vector2(
                Mathf.InverseLerp(-MapHalfWidth, MapHalfWidth, worldPosition.x),
                Mathf.InverseLerp(-MapHalfHeight, MapHalfHeight, worldPosition.y));
            dot.anchoredPosition = Vector2.zero;
        }
    }
}
