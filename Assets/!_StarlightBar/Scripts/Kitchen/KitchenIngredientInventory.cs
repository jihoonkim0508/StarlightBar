using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace StarlightBar.Gameplay
{
    /// <summary>
    /// Kitchen 씬의 재료 수량, 선택 상태와 도마 위 재료 생성을 관리합니다.
    /// </summary>
    public sealed class KitchenIngredientInventory : MonoBehaviour
    {
        [Serializable]
        private sealed class IngredientEntry
        {
            [Tooltip("저장이나 레시피 연결에 사용할 사람이 읽을 수 있는 ID입니다.")]
            public string id;
            [Tooltip("확인 팝업에 표시할 한국어 재료 이름입니다.")]
            public string displayName;
            [Tooltip("인벤토리와 도마 위에 표시할 Sprite입니다.")]
            public Sprite sprite;
            [Min(1), Tooltip("Kitchen 진입 시 사용할 수 있는 수량입니다.")]
            public int startingQuantity = 1;
            [Tooltip("도마 위에 배치할 재료의 기본 크기입니다.")]
            public Vector3 placedScale = Vector3.one;
        }

        [Header("Kitchen UI 연결")]
        [SerializeField, Tooltip("UI 오브젝트에 배치된 재료 인벤토리 UI입니다.")]
        [FormerlySerializedAs("view")]
        private KitchenIngredientInventoryUI ui;

        [Header("도마 배치")]
        [SerializeField] private SpriteRenderer boardRenderer;
        [SerializeField] private Transform placedIngredientRoot;
        [SerializeField] private KitchenPlacedIngredientDrag placedIngredientPrefab;
        [SerializeField, Tooltip("배치되는 첫 재료의 정렬 순서입니다.")]
        private int firstSortingOrder = 10;

        [Header("재료 인벤토리")]
        [SerializeField] private IngredientEntry[] ingredients;

        [Header("배치 레시피")]
        [SerializeField, Tooltip("아래 음식, 위 음식과 완성 음식 조합 목록입니다.")]
        private KitchenRecipe[] recipes;

        private int[] quantities;
        private int pendingIndex = -1;
        private int nextSortingOrder;

        private void Awake()
        {
            if (!ValidateReferences())
            {
                enabled = false;
                return;
            }

            quantities = new int[ingredients.Length];
            for (var index = 0; index < ingredients.Length; index++)
                quantities[index] = Mathf.Max(1, ingredients[index].startingQuantity);
            nextSortingOrder = firstSortingOrder;

            ui.Initialize(ToggleInventory);
            RefreshSlots();
        }

        private void Update()
        {
            if (Keyboard.current?.eKey.wasPressedThisFrame == true)
                ToggleInventory();
        }

        /// <summary>
        /// 재료 선택 인벤토리를 엽니다.
        /// </summary>
        public void OpenInventory()
        {
            if (ui.IsInventoryVisible)
                return;

            pendingIndex = -1;
            ui.HideConfirmation();
            ui.SetInventoryVisible(true);
            RefreshSlots();
        }

        /// <summary>
        /// 버튼 또는 E키 입력으로 재료 인벤토리의 표시 상태를 전환합니다.
        /// </summary>
        public void ToggleInventory()
        {
            if (ui.IsInventoryVisible)
            {
                CloseInventory();
                return;
            }

            OpenInventory();
        }

        /// <summary>
        /// 재료 인벤토리와 배치 확인 팝업을 함께 닫습니다.
        /// </summary>
        public void CloseInventory()
        {
            pendingIndex = -1;
            ui.HideConfirmation();
            ui.SetInventoryVisible(false);
        }

        /// <summary>
        /// 현재 선택한 재료를 확인 팝업에서 취소합니다.
        /// </summary>
        public void CancelPlacement()
        {
            pendingIndex = -1;
            ui.HideConfirmation();
        }

        /// <summary>
        /// 마지막으로 만진 재료가 위에 보이도록 정렬 순서를 갱신합니다.
        /// </summary>
        public void BringToFront(KitchenPlacedIngredientDrag ingredient)
        {
            if (ingredient != null)
                ingredient.SetSortingOrder(nextSortingOrder++);
        }

        private void SelectIngredient(int index)
        {
            if (index < 0 || index >= ingredients.Length || quantities[index] <= 0)
                return;
            pendingIndex = index;
            ui.ShowConfirmation($"{ingredients[index].displayName}을(를) 배치하시겠습니까?");
        }

        /// <summary>
        /// 확인 팝업에서 선택한 재료를 도마 위에 배치합니다.
        /// </summary>
        public void ConfirmPlacement()
        {
            if (pendingIndex < 0 || pendingIndex >= ingredients.Length ||
                quantities[pendingIndex] <= 0)
                return;

            var entry = ingredients[pendingIndex];
            var placed = Instantiate(
                placedIngredientPrefab,
                boardRenderer.bounds.center,
                Quaternion.identity,
                placedIngredientRoot);
            placed.name = $"Placed_{entry.id}";
            placed.Initialize(this, entry.id, entry.sprite, entry.placedScale);
            BringToFront(placed);
            TryCompleteRecipe(placed);

            quantities[pendingIndex]--;
            pendingIndex = -1;
            ui.HideConfirmation();
            ui.SetInventoryVisible(false);
            RefreshSlots();
        }

        private void RefreshSlots()
        {
            var slotIndex = 0;
            for (var ingredientIndex = 0;
                 ingredientIndex < ingredients.Length && slotIndex < ui.SlotCount;
                 ingredientIndex++)
            {
                // 수량이 없는 재료는 목록에서 제외하고 남은 재료를 앞 슬롯부터 채웁니다.
                if (quantities[ingredientIndex] <= 0)
                    continue;

                var capturedIndex = ingredientIndex;
                ui.BindSlot(
                    slotIndex,
                    ingredients[ingredientIndex].sprite,
                    quantities[ingredientIndex],
                    () => SelectIngredient(capturedIndex));
                slotIndex++;
            }

            for (; slotIndex < ui.SlotCount; slotIndex++)
                ui.ClearSlot(slotIndex);
        }

        /// <summary>
        /// 가장 나중에 배치하거나 움직인 위 음식과 아래 음식의 등록된 레시피를 판정합니다.
        /// </summary>
        public void TryCompleteRecipe(KitchenPlacedIngredientDrag topItem)
        {
            if (topItem == null || recipes == null || recipes.Length == 0)
                return;

            var placedItems =
                placedIngredientRoot.GetComponentsInChildren<KitchenPlacedIngredientDrag>();
            KitchenPlacedIngredientDrag matchedBottom = null;
            KitchenRecipe matchedRecipe = null;

            foreach (var bottomItem in placedItems)
            {
                if (bottomItem == null || bottomItem == topItem ||
                    bottomItem.SortingOrder >= topItem.SortingOrder)
                    continue;

                foreach (var recipe in recipes)
                {
                    if (recipe == null ||
                        !recipe.Matches(bottomItem.ItemId, topItem.ItemId) ||
                        CalculateTopOverlapRatio(bottomItem.VisualBounds, topItem.VisualBounds) <
                        recipe.RequiredTopOverlapRatio)
                        continue;

                    // 여러 음식이 겹치면 위 음식 바로 아래에 있는 가장 높은 음식을 사용합니다.
                    if (matchedBottom == null ||
                        bottomItem.SortingOrder > matchedBottom.SortingOrder)
                    {
                        matchedBottom = bottomItem;
                        matchedRecipe = recipe;
                    }
                }
            }

            if (matchedBottom == null || matchedRecipe == null)
                return;

            var result = Instantiate(
                placedIngredientPrefab,
                matchedBottom.transform.position,
                Quaternion.identity,
                placedIngredientRoot);
            var resultName = string.IsNullOrWhiteSpace(matchedRecipe.ResultDisplayName)
                ? matchedRecipe.ResultItemId
                : matchedRecipe.ResultDisplayName;
            result.name = $"Completed_{resultName}";
            result.Initialize(
                this,
                matchedRecipe.ResultItemId,
                matchedRecipe.ResultSprite,
                matchedRecipe.ResultScale);
            BringToFront(result);

            Destroy(matchedBottom.gameObject);
            Destroy(topItem.gameObject);
        }

        private static float CalculateTopOverlapRatio(
            Bounds bottomBounds,
            Bounds topBounds)
        {
            var overlapWidth = Mathf.Max(
                0f,
                Mathf.Min(bottomBounds.max.x, topBounds.max.x) -
                Mathf.Max(bottomBounds.min.x, topBounds.min.x));
            var overlapHeight = Mathf.Max(
                0f,
                Mathf.Min(bottomBounds.max.y, topBounds.max.y) -
                Mathf.Max(bottomBounds.min.y, topBounds.min.y));
            var topArea = topBounds.size.x * topBounds.size.y;
            return topArea > Mathf.Epsilon
                ? overlapWidth * overlapHeight / topArea
                : 0f;
        }

        private bool ValidateReferences()
        {
            var valid = ui != null && ui.HasRequiredReferences &&
                        boardRenderer != null &&
                        placedIngredientRoot != null && placedIngredientPrefab != null &&
                        ingredients != null && ingredients.Length > 0;
            if (!valid)
                Debug.LogError("KitchenIngredientInventory의 Inspector 참조가 누락되었습니다.", this);
            return valid;
        }
    }
}
