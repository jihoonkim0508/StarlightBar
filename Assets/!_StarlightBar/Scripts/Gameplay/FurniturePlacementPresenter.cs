using System.Linq;
using StarlightBar.Content;
using StarlightBar.Core;
using StarlightBar.Systems;
using StarlightBar.UI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace StarlightBar.Gameplay
{
    /// <summary>
    /// 에디터에 배치된 가구 영역에 보유 가구 프리팹을 연결합니다.
    /// </summary>
    public sealed class FurniturePlacementPresenter : MonoBehaviour
    {
        [SerializeField, Tooltip("씬에 배치한 가구 영역과 반복 가구 프리팹입니다.")]
        private FurniturePlacementView view;

        private RectTransform placementArea;
        private FurnitureDraggableView selected;
        private TMP_Text status;

        /// <summary>
        /// 저장된 가구 배치를 현재 주점 화면에 표시합니다.
        /// </summary>
        public void Build(RectTransform parent, GameRuntimeCoordinator coordinator)
        {
            if (view == null || view.PlacementArea == null || view.ItemPrefab == null)
            {
                Debug.LogError("FurniturePlacementView 참조가 연결되지 않았습니다.", this);
                return;
            }

            placementArea = view.PlacementArea;
            status = view.Status;
            ClearPlacedItems();
            DynamicContentFactory.CreateText(parent, "인테리어 배치", 26);
            DynamicContentFactory.CreateText(
                parent,
                "가구를 드래그해 이동 · 우클릭으로 90° 회전 · 선택 후 Delete로 보관",
                18);

            var owned = GameBootstrapper.Instance.Session.Data.ownedFurnitureIds;
            for (var index = 0; index < owned.Count; index++)
            {
                var definition = BuiltInChapterCatalog.FindFurniture(owned[index]);
                if (definition == null)
                    continue;
                var placement = EnsurePlacement(definition.id, index);
                if (!placement.stored)
                {
                    Spawn(definition, placement);
                    continue;
                }

                var capturedDefinition = definition;
                var capturedPlacement = placement;
                DynamicContentFactory.CreateButton(parent, $"보관함에서 배치 · {definition.displayName}", () =>
                {
                    capturedPlacement.stored = false;
                    capturedPlacement.position = Vector2.zero;
                    Spawn(capturedDefinition, capturedPlacement);
                    Save();
                });
            }

            if (status != null)
                status.text = "배치된 가구의 속성이 야간 대사와 증거에 반영됩니다.";
        }

        private void Update()
        {
            if (selected == null || Keyboard.current?.deleteKey.wasPressedThisFrame != true)
                return;
            selected.Store();
            if (status != null)
                status.text = $"{selected.Definition.displayName}을 보관함으로 옮겼습니다.";
            selected = null;
            Save();
        }

        private void ClearPlacedItems()
        {
            for (var index = placementArea.childCount - 1; index >= 0; index--)
                Destroy(placementArea.GetChild(index).gameObject);
        }

        private static FurniturePlacementData EnsurePlacement(string furnitureId, int index)
        {
            var placements = GameBootstrapper.Instance.Session.Data.furniturePlacements;
            var placement = placements.FirstOrDefault(item => item.furnitureId == furnitureId);
            if (placement != null)
                return placement;
            placement = new FurniturePlacementData
            {
                furnitureId = furnitureId,
                position = new Vector2(-180 + index * 70, 0),
                stored = false
            };
            placements.Add(placement);
            return placement;
        }

        private void Spawn(FurnitureDefinition definition, FurniturePlacementData placement)
        {
            if (placementArea == null)
                return;
            var itemView = Instantiate(view.ItemPrefab, placementArea);
            itemView.name = $"Placed_{definition.id}";
            itemView.Bind(definition.sprite, definition.displayName);
            var rect = itemView.GetComponent<RectTransform>();
            rect.anchoredPosition = placement.position;
            rect.localRotation = Quaternion.Euler(0, 0, placement.rotation);
            var draggable = itemView.Draggable;
            draggable.Initialize(
                definition, placement, placementArea, () => selected = draggable, Save);
        }

        private static void Save() => GameBootstrapper.Instance?.SaveNow();
    }
}
