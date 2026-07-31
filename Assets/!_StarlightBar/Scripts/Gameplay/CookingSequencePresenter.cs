using System;
using System.Collections.Generic;
using System.Linq;
using StarlightBar.Content;
using StarlightBar.Systems;
using StarlightBar.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarlightBar.Gameplay
{
    /// <summary>
    /// 재료, 손질법, 조리 순서, 장식, 마법 재료를 한 단계씩 선택하는 조리 UI입니다.
    /// </summary>
    public sealed class CookingSequencePresenter
    {
        private RecipeDefinition recipe;
        private RectTransform stageRoot;
        private TMP_Text progress;
        private Action<CookingSelection> submitted;
        private readonly List<RecipeStep> selectedSteps = new();
        private int stepIndex;
        private string pendingIngredient;
        private CookingMethod pendingMethod;
        private string decorationId;
        private string magicalIngredientId;

        /// <summary>
        /// 재료부터 마법 재료까지 다섯 단계의 조리 선택 UI를 구성합니다.
        /// </summary>
        public void Build(
            RectTransform parent,
            RecipeDefinition definition,
            Action<CookingSelection> onSubmitted)
        {
            recipe = definition;
            submitted = onSubmitted;
            selectedSteps.Clear();
            stepIndex = 0;
            progress = DynamicContentFactory.CreateText(parent, string.Empty, 20);
            if (!string.IsNullOrWhiteSpace(recipe.expectedEffectHint))
            {
                DynamicContentFactory.CreateText(
                    parent,
                    $"예상 효과 힌트 · {recipe.expectedEffectHint}\n" +
                    $"효과: {string.Join(" · ", recipe.effects.Select(EffectLabel))}",
                    18);
            }

            stageRoot = DynamicContentFactory.CreateContentGroup(parent, "CookingStage");
            ShowIngredientSelection();
        }

        private void ShowIngredientSelection()
        {
            ClearStage();
            SetProgress($"1/5 재료 선택 · 조리 항목 {stepIndex + 1}/{recipe.steps.Count}");
            DynamicContentFactory.CreateText(stageRoot, "사용할 재료를 고르세요. 노트 단서가 있는 재료에는 ✦가 표시됩니다.", 21);
            foreach (var ingredientId in recipe.allowedIngredientIds.Distinct())
            {
                var captured = ingredientId;
                DynamicContentFactory.CreateButton(
                    stageRoot,
                    $"{BuiltInChapterCatalog.GetLabel(ingredientId)}  ✦",
                    () =>
                    {
                        pendingIngredient = captured;
                        ShowMethodSelection();
                    });
            }
            DynamicContentFactory.CreateButton(stageRoot, "정체불명의 검은 열매", () =>
            {
                pendingIngredient = "ingredient_unrelated";
                ShowMethodSelection();
            });
        }

        private void ShowMethodSelection()
        {
            ClearStage();
            SetProgress($"2/5 손질·조리법 · {BuiltInChapterCatalog.GetLabel(pendingIngredient)}");
            var expected = recipe.steps[Mathf.Clamp(stepIndex, 0, recipe.steps.Count - 1)].method;
            var candidates = new[] { expected, CookingMethod.Raw, CookingMethod.Boil, CookingMethod.Grill }
                .Distinct().Take(3);
            foreach (var method in candidates)
            {
                var captured = method;
                DynamicContentFactory.CreateButton(stageRoot, ToKorean(method), () =>
                {
                    pendingMethod = captured;
                    ShowOrderSelection();
                });
            }
        }

        private void ShowOrderSelection()
        {
            ClearStage();
            SetProgress("3/5 조리 순서");
            for (var order = 0; order < recipe.steps.Count; order++)
            {
                var captured = order;
                DynamicContentFactory.CreateButton(stageRoot, $"{order + 1}번째로 처리", () =>
                {
                    selectedSteps.Add(new RecipeStep
                    {
                        ingredientId = pendingIngredient,
                        method = pendingMethod,
                        order = captured
                    });
                    stepIndex++;
                    if (stepIndex < recipe.steps.Count)
                        ShowIngredientSelection();
                    else
                        ShowDecorationSelection();
                });
            }
        }

        private void ShowDecorationSelection()
        {
            ClearStage();
            SetProgress("4/5 마무리 장식");
            DynamicContentFactory.CreateButton(
                stageRoot,
                $"{BuiltInChapterCatalog.GetLabel(recipe.decorationId)}  ✦",
                () =>
                {
                    decorationId = recipe.decorationId;
                    ShowMagicSelection();
                });
            DynamicContentFactory.CreateButton(stageRoot, "장식하지 않는다", () =>
            {
                decorationId = string.Empty;
                ShowMagicSelection();
            });
            DynamicContentFactory.CreateButton(stageRoot, "강한 네온 설탕 장식", () =>
            {
                decorationId = "decoration_wrong";
                ShowMagicSelection();
            });
        }

        private void ShowMagicSelection()
        {
            ClearStage();
            SetProgress("5/5 마법 재료");
            DynamicContentFactory.CreateText(stageRoot, "스텔라가 준비한 올바른 마법 재료가 기억공간의 문을 활성화합니다.", 20);
            DynamicContentFactory.CreateButton(
                stageRoot,
                $"{BuiltInChapterCatalog.GetLabel(recipe.magicalIngredientId)}  ✦",
                () =>
                {
                    magicalIngredientId = recipe.magicalIngredientId;
                    ShowReview();
                });
            DynamicContentFactory.CreateButton(stageRoot, "정체불명의 검은 별가루", () =>
            {
                magicalIngredientId = "magic_contaminated";
                ShowReview();
            });
        }

        private void ShowReview()
        {
            ClearStage();
            SetProgress("조리 검토");
            var stepSummary = string.Join("\n", selectedSteps.Select(step =>
                $"{step.order + 1}. {BuiltInChapterCatalog.GetLabel(step.ingredientId)} · {ToKorean(step.method)}"));
            DynamicContentFactory.CreateText(
                stageRoot,
                $"재료·방법\n{stepSummary}\n\n" +
                $"장식: {BuiltInChapterCatalog.GetLabel(decorationId)}\n" +
                $"마법 재료: {BuiltInChapterCatalog.GetLabel(magicalIngredientId)}",
                21);
            DynamicContentFactory.CreateButton(stageRoot, "요리 제출", () => submitted?.Invoke(new CookingSelection
            {
                steps = selectedSteps.ToList(),
                decorationId = decorationId,
                magicalIngredientId = magicalIngredientId
            }));
            DynamicContentFactory.CreateButton(stageRoot, "처음부터 다시 조리", () =>
            {
                selectedSteps.Clear();
                stepIndex = 0;
                ShowIngredientSelection();
            });
        }

        private void ClearStage()
        {
            for (var index = stageRoot.childCount - 1; index >= 0; index--)
                UnityEngine.Object.Destroy(stageRoot.GetChild(index).gameObject);
        }

        private void SetProgress(string value)
        {
            if (progress != null)
                progress.text = $"{recipe.displayName} · {value}";
        }

        private static string ToKorean(CookingMethod method) => method switch
        {
            CookingMethod.Raw => "생으로 사용",
            CookingMethod.Slice => "썰기",
            CookingMethod.Grind => "갈기",
            CookingMethod.Marinate => "재우기",
            CookingMethod.Boil => "끓이기",
            CookingMethod.Steam => "찌기",
            CookingMethod.Bake => "굽기",
            CookingMethod.Grill => "직화 굽기",
            CookingMethod.StirFry => "볶기",
            CookingMethod.Infuse => "우려내기",
            CookingMethod.Chill => "차갑게 식히기",
            _ => method.ToString()
        };

        private static string EffectLabel(CookingEffect effect) => effect switch
        {
            CookingEffect.Stability => "안정",
            CookingEffect.Trust => "신뢰",
            CookingEffect.Memory => "기억",
            CookingEffect.Truth => "진실",
            CookingEffect.Empathy => "공감",
            _ => "연결"
        };
    }
}
