using System;
using System.Collections.Generic;
using System.Linq;
using StarlightBar.Content;
using TMPro;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace StarlightBar.Editor
{
    /// <summary>
    /// 콘텐츠 ID와 챕터 참조를 검사해 런타임 진행 불능을 사전에 차단합니다.
    /// </summary>
    public sealed class ContentValidator : IPreprocessBuildWithReport
    {
        private static readonly Type[] ContentTypes =
        {
            typeof(CharacterDefinition),
            typeof(ObjectiveDefinition),
            typeof(EvidenceDefinition),
            typeof(DialogueDefinition),
            typeof(RecipeDefinition),
            typeof(FurnitureDefinition),
            typeof(DeductionDefinition),
            typeof(MemorySpaceDefinition),
            typeof(ZodiacChapterDefinition)
        };

        public int callbackOrder => 0;

        /// <summary>
        /// Unity 메뉴에서 전체 ScriptableObject 및 내장 12궁 콘텐츠 검증을 실행합니다.
        /// </summary>
        [MenuItem("별빛주점/콘텐츠 검증")]
        public static void ValidateFromMenu()
        {
            var errors = ValidateAll(out var warnings);
            foreach (var warning in warnings)
                Debug.LogWarning(warning);
            if (errors.Count == 0)
                Debug.Log("별빛주점 콘텐츠 검증을 통과했습니다.");
            else
                foreach (var error in errors)
                    Debug.LogError(error);
        }

        /// <summary>
        /// 빌드 전에 콘텐츠 무결성과 정식판 12궁 등록 여부를 강제로 검사합니다.
        /// </summary>
        public void OnPreprocessBuild(BuildReport report)
        {
            var errors = ValidateAll(out var warnings);
            foreach (var warning in warnings)
                Debug.LogWarning(warning);

            var developmentBuild = (report.summary.options & BuildOptions.Development) != 0;
            var database = FindAssets<ChapterDatabase>().FirstOrDefault();
            var chapterCount = database != null && database.chapters.Count >= 12
                ? database.chapters.Count
                : BuiltInChapterCatalog.GetChapters().Count;
            if (!developmentBuild && chapterCount != 12)
                errors.Add($"정식 빌드에는 12개 챕터가 필요합니다. 현재 {chapterCount}개입니다.");

            if (errors.Count > 0)
                throw new BuildFailedException(string.Join("\n", errors));
        }

        /// <summary>
        /// 모든 콘텐츠 자산을 검사하고 오류와 경고를 분리해 반환합니다.
        /// </summary>
        public static List<string> ValidateAll(out List<string> warnings)
        {
            var errors = new List<string>();
            warnings = new List<string>();
            var ids = new Dictionary<string, UnityEngine.Object>();

            foreach (var type in ContentTypes)
            {
                foreach (var asset in FindAssets(type))
                {
                    var serialized = new SerializedObject(asset);
                    var idProperty = serialized.FindProperty("id");
                    if (idProperty == null)
                        continue;

                    var id = idProperty.stringValue;
                    if (string.IsNullOrWhiteSpace(id))
                    {
                        errors.Add($"ID가 비어 있습니다: {AssetDatabase.GetAssetPath(asset)}");
                        continue;
                    }

                    if (ids.TryGetValue(id, out var duplicate))
                        errors.Add($"중복 ID '{id}': {AssetDatabase.GetAssetPath(duplicate)}, {AssetDatabase.GetAssetPath(asset)}");
                    else
                        ids.Add(id, asset);
                }
            }

            foreach (var chapter in FindAssets<ZodiacChapterDefinition>())
                ValidateChapter(chapter, errors);
            ValidateBuiltInCatalog(errors);

            var database = FindAssets<ChapterDatabase>().FirstOrDefault();
            if (database == null)
                errors.Add("ChapterDatabase 자산이 없습니다.");
            else if (database.chapters.Count < 12 && BuiltInChapterCatalog.GetChapters().Count < 12)
                warnings.Add($"진행 가능한 챕터가 {database.chapters.Count}/12개입니다.");

            var koreanFontPath =
                "Assets/!_StarlightBar/Resources/StarlightBar/Fonts/NotoSansKR-Variable.ttf";
            var koreanTmpFontPath =
                "Assets/!_StarlightBar/Resources/StarlightBar/Fonts/NotoSansKR-Dynamic.asset";
            var koreanFontLicensePath = "Assets/!_StarlightBar/Fonts/NotoSansKR-OFL.txt";
            if (AssetDatabase.LoadAssetAtPath<Font>(koreanFontPath) == null)
                errors.Add("배포용 Noto Sans KR 한글 폰트가 없습니다.");
            var koreanTmpFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(koreanTmpFontPath);
            if (koreanTmpFont == null || koreanTmpFont.material == null)
                errors.Add("씬 전환에 안전한 Noto Sans KR TMP 폰트 또는 영구 재료가 없습니다.");
            else if (koreanTmpFont.isMultiAtlasTexturesEnabled || koreanTmpFont.atlasWidth < 4096)
                errors.Add("한글 TMP 폰트는 파괴된 폴백 재료를 만들지 않는 4096 단일 동적 아틀라스여야 합니다.");
            if (AssetDatabase.LoadAssetAtPath<TextAsset>(koreanFontLicensePath) == null)
                errors.Add("Noto Sans KR OFL 라이선스 파일이 없습니다.");
            var inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                "Assets/!_StarlightBar/Input/StarlightBarControls.inputactions");
            var gameplayMap = inputActions?.FindActionMap("Gameplay");
            var requiredActions = new[]
            {
                "Move", "Inspect", "Talk", "Notebook", "Objectives", "Telescope", "Menu"
            };
            if (gameplayMap == null || requiredActions.Any(name => gameplayMap.FindAction(name) == null))
                errors.Add("게임 전용 Gameplay Input Actions 구성이 없거나 필수 동작이 누락되었습니다.");

            return errors;
        }

        private static void ValidateBuiltInCatalog(ICollection<string> errors)
        {
            var chapters = BuiltInChapterCatalog.GetChapters();
            if (chapters.Count != 12)
            {
                errors.Add($"내장 카탈로그는 12개 챕터여야 합니다. 현재 {chapters.Count}개입니다.");
                return;
            }

            var ids = new HashSet<string>();
            var indices = new HashSet<int>();
            foreach (var chapter in chapters)
            {
                if (chapter == null)
                {
                    errors.Add("내장 카탈로그에 비어 있는 챕터가 있습니다.");
                    continue;
                }

                RegisterId(chapter.id, "챕터", ids, errors);
                if (!indices.Add(chapter.chapterIndex) || chapter.chapterIndex is < 0 or > 11)
                    errors.Add($"{chapter.id}: 챕터 순서 {chapter.chapterIndex}가 중복되었거나 범위를 벗어났습니다.");
                ValidateChapter(chapter, errors);
                ValidateNarrativeFields(chapter, errors);

                RegisterContentId(chapter.guest, chapter.guest?.id, "손님", ids, errors);
                RegisterContentId(chapter.specialRecipe, chapter.specialRecipe?.id, "레시피", ids, errors);
                RegisterContentId(chapter.deduction, chapter.deduction?.id, "추리", ids, errors);
                RegisterContentId(chapter.memorySpace, chapter.memorySpace?.id, "기억공간", ids, errors);
                RegisterContentId(chapter.briefingDialogue, chapter.briefingDialogue?.id, "아침 대화", ids, errors);
                RegisterContentId(chapter.nightDialogue, chapter.nightDialogue?.id, "야간 대화", ids, errors);
                foreach (var objective in chapter.objectives.Where(item => item != null))
                    RegisterContentId(objective, objective.id, "목표", ids, errors);
                foreach (var evidence in chapter.evidence.Where(item => item != null))
                    RegisterContentId(evidence, evidence.id, "증거", ids, errors);
                foreach (var furniture in chapter.obtainableFurniture.Where(item => item != null))
                    RegisterContentId(furniture, furniture.id, "가구", ids, errors);

                ValidateEvidenceLinks(chapter, errors);
                ValidateDialogue(chapter.briefingDialogue, chapter.id, errors);
                ValidateDialogue(chapter.nightDialogue, chapter.id, errors);
                ValidateRecipe(chapter, errors);
                ValidateMemorySpace(chapter, errors);
            }

            if (!Enumerable.Range(0, 12).All(indices.Contains))
                errors.Add("내장 카탈로그의 챕터 순서는 0부터 11까지 모두 존재해야 합니다.");
        }

        private static void ValidateNarrativeFields(ZodiacChapterDefinition chapter, ICollection<string> errors)
        {
            if (string.IsNullOrWhiteSpace(chapter.mythologySource))
                errors.Add($"{chapter.id}: 신화 원전 또는 사건 설명이 없습니다.");
            if (string.IsNullOrWhiteSpace(chapter.emotionalTheme))
                errors.Add($"{chapter.id}: 감정 주제가 없습니다.");
            if (string.IsNullOrWhiteSpace(chapter.currentLife))
                errors.Add($"{chapter.id}: 인간으로서의 현재 생활이 없습니다.");
            if (string.IsNullOrWhiteSpace(chapter.traumaReaction))
                errors.Add($"{chapter.id}: 트라우마 반응이 없습니다.");
            if (string.IsNullOrWhiteSpace(chapter.completeRestorationText) ||
                string.IsNullOrWhiteSpace(chapter.partialRestorationText) ||
                string.IsNullOrWhiteSpace(chapter.unstableRestorationText))
                errors.Add($"{chapter.id}: 복원 등급별 대사가 모두 필요합니다.");
            if (string.IsNullOrWhiteSpace(chapter.returnToSkyChoiceText) ||
                string.IsNullOrWhiteSpace(chapter.remainHumanWithMemoriesChoiceText) ||
                string.IsNullOrWhiteSpace(chapter.remainHumanWithoutIdentityChoiceText))
                errors.Add($"{chapter.id}: 세 가지 개인 미래 선택의 기록 문구가 모두 필요합니다.");
            if (chapter.guest != null &&
                chapter.guest.preferredFurnitureTraits.Intersect(chapter.guest.rejectedFurnitureTraits).Any())
                errors.Add($"{chapter.id}: 같은 가구 속성을 동시에 선호하고 기피할 수 없습니다.");
            if (chapter.guest == null ||
                chapter.guest.preferredFurnitureTraits.Count == 0 ||
                chapter.guest.rejectedFurnitureTraits.Count == 0)
                errors.Add($"{chapter.id}: 손님의 선호·기피 가구 속성이 모두 필요합니다.");

            var objectiveTypes = chapter.objectives
                .Where(item => item != null)
                .Select(item => item.type)
                .ToHashSet();
            foreach (var requiredType in new[]
                     {
                         ObjectiveType.RequiredIngredient,
                         ObjectiveType.MythEvidence,
                         ObjectiveType.HumanLifeTrace,
                         ObjectiveType.Furniture
                     })
            {
                if (!objectiveTypes.Contains(requiredType))
                    errors.Add($"{chapter.id}: 제작 게이트 목표 유형 '{requiredType}'이 누락되었습니다.");
            }

            var evidenceCategories = chapter.evidence
                .Where(item => item != null)
                .Select(item => item.category)
                .ToHashSet();
            foreach (var requiredCategory in new[]
                     {
                         EvidenceCategory.Identity,
                         EvidenceCategory.Myth,
                         EvidenceCategory.HumanLife,
                         EvidenceCategory.FoodReaction,
                         EvidenceCategory.InteriorReaction
                     })
            {
                if (!evidenceCategories.Contains(requiredCategory))
                    errors.Add($"{chapter.id}: 제작 게이트 증거 범주 '{requiredCategory}'가 누락되었습니다.");
            }
        }

        private static void ValidateEvidenceLinks(ZodiacChapterDefinition chapter, ICollection<string> errors)
        {
            var evidenceIds = chapter.evidence.Where(item => item != null).Select(item => item.id).ToHashSet();
            foreach (var evidence in chapter.evidence.Where(item => item != null))
            {
                foreach (var linkedId in evidence.allowedLinkEvidenceIds)
                    if (!evidenceIds.Contains(linkedId))
                        errors.Add($"{chapter.id}: 증거 '{evidence.id}'의 연결 대상 '{linkedId}'가 없습니다.");
            }

            if (chapter.deduction == null)
                return;
            if (!chapter.deduction.zodiacCandidateIds.Contains(chapter.deduction.correctZodiacId))
                errors.Add($"{chapter.id}: 별자리 정답이 후보 목록에 없습니다.");
            if (!chapter.deduction.mythCandidateIds.Contains(chapter.deduction.correctMythId))
                errors.Add($"{chapter.id}: 신화 정답이 후보 목록에 없습니다.");
            if (chapter.deduction.zodiacCandidateIds.Distinct().Count() < 3 ||
                chapter.deduction.mythCandidateIds.Distinct().Count() < 3)
                errors.Add($"{chapter.id}: 별자리와 신화 후보는 각각 정답 포함 3개 이상이어야 합니다.");
            foreach (var requiredId in chapter.deduction.requiredCoreEvidenceIds)
            {
                var requiredEvidence = chapter.evidence.FirstOrDefault(item => item != null && item.id == requiredId);
                if (requiredEvidence != null && !requiredEvidence.coreEvidence)
                    errors.Add($"{chapter.id}: 추리 필수 증거 '{requiredId}'가 핵심 증거로 표시되지 않았습니다.");
            }
        }

        private static void ValidateDialogue(
            DialogueDefinition dialogue, string chapterId, ICollection<string> errors)
        {
            if (dialogue == null)
                return;
            var lineIds = dialogue.lines.Where(item => item != null).Select(item => item.id).ToList();
            if (lineIds.Count == 0 || lineIds.Any(string.IsNullOrWhiteSpace) ||
                lineIds.Distinct().Count() != lineIds.Count)
                errors.Add($"{chapterId}: 대화 '{dialogue.id}'의 대사 ID가 비었거나 중복되었습니다.");
            if (!lineIds.Contains(dialogue.entryLineId))
                errors.Add($"{chapterId}: 대화 '{dialogue.id}'의 진입 대사를 찾을 수 없습니다.");
            foreach (var choice in dialogue.lines.Where(item => item != null).SelectMany(item => item.choices))
            {
                if (choice != null && !string.IsNullOrWhiteSpace(choice.nextLineId) &&
                    !lineIds.Contains(choice.nextLineId))
                    errors.Add($"{chapterId}: 선택지 '{choice.id}'의 다음 대사 '{choice.nextLineId}'가 없습니다.");
            }
        }

        private static void ValidateRecipe(ZodiacChapterDefinition chapter, ICollection<string> errors)
        {
            var recipe = chapter.specialRecipe;
            if (recipe == null)
                return;
            if (recipe.steps.Count == 0 || recipe.allowedIngredientIds.Count == 0 ||
                string.IsNullOrWhiteSpace(recipe.decorationId) ||
                string.IsNullOrWhiteSpace(recipe.magicalIngredientId))
                errors.Add($"{chapter.id}: 레시피의 재료·장식·마법 재료 정보가 불완전합니다.");
            if (recipe.effects.Count == 0)
                errors.Add($"{chapter.id}: 레시피의 기대 효과가 없습니다.");
            if (recipe.steps.Any(step => step == null ||
                                         !recipe.allowedIngredientIds.Contains(step.ingredientId)))
                errors.Add($"{chapter.id}: 레시피 단계 재료가 허용 재료 목록에 없거나 비어 있습니다.");
            if (recipe.mediumThreshold < 0f || recipe.highThreshold > 1f ||
                recipe.mediumThreshold >= recipe.highThreshold)
                errors.Add($"{chapter.id}: 조리 품질 기준값의 순서가 올바르지 않습니다.");
        }

        private static void ValidateMemorySpace(ZodiacChapterDefinition chapter, ICollection<string> errors)
        {
            var memory = chapter.memorySpace;
            if (memory == null)
                return;
            if (memory.objectiveIds.Count < 3 ||
                memory.objectiveTitles.Count != memory.objectiveIds.Count ||
                memory.objectiveIds.Any(string.IsNullOrWhiteSpace) ||
                memory.objectiveIds.Distinct().Count() != memory.objectiveIds.Count)
                errors.Add($"{chapter.id}: 기억공간 목표와 한국어 제목 구성이 올바르지 않습니다.");
            if (string.IsNullOrWhiteSpace(memory.sceneVariantId) || memory.mechanicModuleIds.Count == 0)
                errors.Add($"{chapter.id}: 별자리별 기억공간 기믹 모듈이 없습니다.");
            else if (!memory.mechanicModuleIds.Contains(memory.sceneVariantId))
                errors.Add($"{chapter.id}: 기억공간 변형 ID가 활성 기믹 모듈 목록에 없습니다.");
            if (string.IsNullOrWhiteSpace(memory.keyMemoryObjectId))
                errors.Add($"{chapter.id}: 보호해야 할 핵심 기억 오브젝트가 없습니다.");
        }

        private static void RegisterContentId(
            UnityEngine.Object instance, string id, string label, ISet<string> ids, ICollection<string> errors)
        {
            if (instance != null)
                RegisterId(id, label, ids, errors);
        }

        private static void RegisterId(string id, string label, ISet<string> ids, ICollection<string> errors)
        {
            if (string.IsNullOrWhiteSpace(id))
                errors.Add($"내장 {label} ID가 비어 있습니다.");
            else if (!ids.Add(id))
                errors.Add($"내장 콘텐츠 ID '{id}'가 중복되었습니다.");
        }

        private static void ValidateChapter(ZodiacChapterDefinition chapter, ICollection<string> errors)
        {
            if (chapter.guest == null) errors.Add($"{chapter.id}: 손님 정의가 없습니다.");
            if (chapter.specialRecipe == null) errors.Add($"{chapter.id}: 전용 레시피가 없습니다.");
            if (chapter.deduction == null) errors.Add($"{chapter.id}: 추리 정의가 없습니다.");
            if (chapter.memorySpace == null) errors.Add($"{chapter.id}: 기억공간 정의가 없습니다.");
            if (chapter.objectives == null || chapter.objectives.All(item => item == null || !item.mandatory))
                errors.Add($"{chapter.id}: 필수 목표가 없습니다.");

            if (chapter.deduction != null)
            {
                var evidenceIds = new HashSet<string>(
                    chapter.evidence.Where(item => item != null).Select(item => item.id));
                foreach (var requiredId in chapter.deduction.requiredCoreEvidenceIds)
                    if (!evidenceIds.Contains(requiredId))
                        errors.Add($"{chapter.id}: 핵심 증거 '{requiredId}'가 챕터 증거 목록에 없습니다.");
            }
        }

        private static IEnumerable<T> FindAssets<T>() where T : UnityEngine.Object
        {
            return AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/!_StarlightBar" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<T>)
                .Where(item => item != null);
        }

        private static IEnumerable<UnityEngine.Object> FindAssets(Type type)
        {
            return AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/!_StarlightBar" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(path => AssetDatabase.LoadAssetAtPath(path, type))
                .Where(item => item != null);
        }
    }
}
