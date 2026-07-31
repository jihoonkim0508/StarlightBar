using System;
using System.Collections.Generic;
using System.Linq;
using StarlightBar.Content;

namespace StarlightBar.Systems
{
    /// <summary>
    /// 플레이어가 선택한 조리 단계·장식·마법 재료를 묶어 제출합니다.
    /// </summary>
    [Serializable]
    public sealed class CookingSelection
    {
        public List<RecipeStep> steps = new();
        public string decorationId;
        public string magicalIngredientId;
    }

    /// <summary>
    /// 조리 선택을 레시피와 비교해 품질과 제출 가능 여부를 계산합니다.
    /// </summary>
    public static class RecipeEvaluator
    {
        /// <summary>
        /// 조리 선택을 정답 레시피와 비교해 제출 가능 여부와 상·중·하 품질을 판정합니다.
        /// </summary>
        public static CookingEvaluation Evaluate(RecipeDefinition recipe, CookingSelection selection)
        {
            if (recipe == null) throw new ArgumentNullException(nameof(recipe));
            if (selection == null) throw new ArgumentNullException(nameof(selection));

            var selectedIngredientIds = selection.steps
                .Where(step => step != null && !string.IsNullOrWhiteSpace(step.ingredientId))
                .Select(step => step.ingredientId)
                .ToHashSet();
            var isRelated = selectedIngredientIds.Any(recipe.allowedIngredientIds.Contains);
            if (!isRelated)
                return new CookingEvaluation(false, 0f, CookingQuality.Low);

            var totalPoints = recipe.steps.Count + 2;
            var earnedPoints = 0;
            foreach (var expected in recipe.steps)
            {
                if (selection.steps.Any(actual =>
                        actual != null &&
                        actual.ingredientId == expected.ingredientId &&
                        actual.method == expected.method &&
                        actual.order == expected.order))
                {
                    earnedPoints++;
                }
            }

            if (selection.decorationId == recipe.decorationId) earnedPoints++;
            if (selection.magicalIngredientId == recipe.magicalIngredientId) earnedPoints++;

            var score = totalPoints == 0 ? 0f : (float)earnedPoints / totalPoints;
            var quality = score >= recipe.highThreshold
                ? CookingQuality.High
                : score >= recipe.mediumThreshold
                    ? CookingQuality.Medium
                    : CookingQuality.Low;
            return new CookingEvaluation(true, score, quality);
        }
    }

    /// <summary>
    /// 조리 판정 결과입니다.
    /// </summary>
    public readonly struct CookingEvaluation
    {
        public bool CanServe { get; }
        public float Score { get; }
        public CookingQuality Quality { get; }

        /// <summary>
        /// 조리 제출 가능 여부와 일치 점수 및 품질을 만듭니다.
        /// </summary>
        public CookingEvaluation(bool canServe, float score, CookingQuality quality)
        {
            CanServe = canServe;
            Score = score;
            Quality = quality;
        }
    }
}
