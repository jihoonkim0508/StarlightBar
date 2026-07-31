using System;
using System.Collections.Generic;
using System.Linq;
using StarlightBar.Content;
using StarlightBar.Core;
using UnityEngine;

namespace StarlightBar.Systems
{
    /// <summary>
    /// 콘텐츠 데이터와 진행·목표·조리·추리·기억공간 시스템을 실제 하루 루프로 연결합니다.
    /// </summary>
    public sealed class GameRuntimeCoordinator
    {
        private readonly GameSession session;
        private readonly GameFlowController flow;
        private readonly ChapterDatabase database;
        private readonly IReadOnlyList<ZodiacChapterDefinition> chapters;
        private readonly Action checkpointSaver;

        public ZodiacChapterDefinition CurrentChapter { get; private set; }
        public ObjectiveTracker Objectives { get; } = new();
        public InventoryService Inventory { get; private set; }
        public EvidenceGraph Evidence { get; private set; } = new();
        public GuestStateModel GuestState { get; private set; } = new();
        public MemorySpaceSession MemorySpace { get; private set; }
        public CookingEvaluation? CookingResult { get; private set; }
        public bool PreparationComplete { get; private set; }
        public bool NightIntroductionComplete { get; private set; }
        public bool DeductionComplete { get; private set; }
        public bool ChapterFinalized { get; private set; }

        /// <summary>
        /// 세션·진행 컨트롤러와 챕터 콘텐츠를 결합해 현재 하루의 런타임 시스템을 만듭니다.
        /// </summary>
        /// <param name="gameSession">저장 가능한 현재 게임 세션입니다.</param>
        /// <param name="gameFlow">중앙 게임 단계 전환 컨트롤러입니다.</param>
        /// <param name="saveCheckpoint">중요 진행 직후 호출할 선택적 자동 저장 동작입니다.</param>
        public GameRuntimeCoordinator(
            GameSession gameSession,
            GameFlowController gameFlow,
            Action saveCheckpoint = null)
        {
            session = gameSession ?? throw new ArgumentNullException(nameof(gameSession));
            flow = gameFlow ?? throw new ArgumentNullException(nameof(gameFlow));
            checkpointSaver = saveCheckpoint;
            database = Resources.Load<ChapterDatabase>("StarlightBar/ChapterDatabase");
            chapters = database != null && database.chapters.Count >= 12
                ? database.chapters.Where(item => item != null).OrderBy(item => item.chapterIndex).ToArray()
                : BuiltInChapterCatalog.GetChapters();
            Inventory = new InventoryService(session.Data);
            ReloadCurrentChapter();
        }

        /// <summary>
        /// 새 게임 또는 불러오기 이후 저장 데이터에 맞춰 현재 챕터 시스템을 재구성합니다.
        /// </summary>
        public void ReloadCurrentChapter()
        {
            NormalizeSaveCollections();
            if (chapters == null || chapters.Count == 0)
            {
                Debug.LogError("별자리 챕터 데이터를 찾지 못했습니다.");
                CurrentChapter = null;
                return;
            }

            var index = Mathf.Clamp(session.Data.currentChapterIndex, 0, chapters.Count - 1);
            CurrentChapter = chapters.FirstOrDefault(item => item.id == session.Data.currentChapterId) ?? chapters[index];
            session.Data.currentChapterId = CurrentChapter.id;
            session.Data.currentChapterIndex = CurrentChapter.chapterIndex;
            Inventory = new InventoryService(session.Data);

            Objectives.Load(CurrentChapter.objectives);
            foreach (var objectiveId in session.Data.completedObjectiveIds ?? new List<string>())
                Objectives.AddProgress(objectiveId);

            Evidence = new EvidenceGraph();
            foreach (var evidenceId in session.Data.collectedEvidenceIds ?? new List<string>())
            {
                var definition = CurrentChapter.evidence.Find(item => item != null && item.id == evidenceId);
                if (definition != null)
                    Evidence.Collect(definition);
            }
            foreach (var link in session.Data.evidenceLinks)
                Evidence.TryLink(link.firstEvidenceId, link.secondEvidenceId);

            GuestState = new GuestStateModel();
            GuestState.Restore(
                session.Data.currentGuestTrust,
                session.Data.currentGuestStability,
                session.Data.currentGuestMemory);
            MemorySpace = CurrentChapter.memorySpace == null ? null : new MemorySpaceSession(CurrentChapter.memorySpace);
            MemorySpace?.RestoreCompletedObjectives(session.Data.completedMemoryObjectiveIds);
            PreparationComplete = session.Data.currentPreparationComplete;
            NightIntroductionComplete = session.Data.currentNightIntroductionComplete;
            DeductionComplete = session.Data.currentDeductionComplete;
            ChapterFinalized = session.Data.guestProgress.Any(item =>
                item.characterId == CurrentChapter.guest.id && item.completed);
            CookingResult = session.Data.currentCookingComplete
                ? new CookingEvaluation(true, ScoreFor(session.Data.currentCookingQuality), session.Data.currentCookingQuality)
                : null;
        }

        /// <summary>
        /// 목표를 완료하고 목표 종류에 따라 재료나 증거를 함께 지급합니다.
        /// </summary>
        public bool CompleteObjective(string objectiveId)
        {
            var progress = Objectives.All.FirstOrDefault(item => item.Definition.id == objectiveId);
            if (progress == null || progress.IsComplete || !Objectives.AddProgress(objectiveId))
                return false;

            if (!session.Data.completedObjectiveIds.Contains(objectiveId))
                session.Data.completedObjectiveIds.Add(objectiveId);

            var definition = progress.Definition;
            if (definition.type == ObjectiveType.RequiredIngredient)
            {
                var recipeIngredients = CurrentChapter.specialRecipe == null
                    ? new[] { definition.targetContentId }
                    : CurrentChapter.specialRecipe.steps.Select(step => step.ingredientId).Distinct();
                foreach (var ingredientId in recipeIngredients)
                    Inventory.Add(ingredientId, 1);
                var identityEvidence = CurrentChapter.evidence.Find(item =>
                    item != null && item.category == EvidenceCategory.Identity && !Evidence.Collected.Contains(item));
                if (identityEvidence != null)
                    CollectEvidence(identityEvidence);
            }

            if (definition.type == ObjectiveType.Furniture &&
                !session.Data.ownedFurnitureIds.Contains(definition.targetContentId))
            {
                session.Data.ownedFurnitureIds.Add(definition.targetContentId);
                session.Data.furniturePlacements.Add(new FurniturePlacementData
                {
                    furnitureId = definition.targetContentId,
                    position = Vector2.zero,
                    rotation = 0f,
                    stored = false
                });
            }

            var evidence = CurrentChapter.evidence.Find(item => item != null && item.id == definition.targetContentId);
            if (evidence != null)
                CollectEvidence(evidence);
            return true;
        }

        /// <summary>
        /// 증거를 런타임 그래프와 저장 데이터에 중복 없이 기록합니다.
        /// </summary>
        public bool CollectEvidence(EvidenceDefinition evidence)
        {
            if (!Evidence.Collect(evidence))
                return false;
            if (!session.Data.collectedEvidenceIds.Contains(evidence.id))
                session.Data.collectedEvidenceIds.Add(evidence.id);
            return true;
        }

        /// <summary>
        /// 대사나 선택지에 연결된 증거 ID를 현재 챕터에서 찾아 기록합니다.
        /// </summary>
        public bool CollectEvidence(string evidenceId)
        {
            var evidence = CurrentChapter?.evidence?.FirstOrDefault(item => item != null && item.id == evidenceId);
            return evidence != null && CollectEvidence(evidence);
        }

        /// <summary>
        /// 대화 선택의 신뢰·안정 변화를 내부 손님 상태에 적용합니다.
        /// </summary>
        public void ApplyDialogueChoice(DialogueChoice choice)
        {
            if (choice == null)
                return;
            GuestState.Apply(choice.trustDelta, choice.stabilityDelta, 0);
            if (!string.IsNullOrWhiteSpace(choice.evidenceId))
                CollectEvidence(choice.evidenceId);
            SyncGuestState();
        }

        /// <summary>
        /// 필수 준비를 완료하고 현재 가구에 대한 손님 반응을 적용합니다.
        /// </summary>
        public void CompletePreparation()
        {
            PreparationComplete = true;
            session.Data.currentPreparationComplete = true;
            ApplyFurnitureReaction();
        }

        /// <summary>
        /// 손님의 입장 관찰과 첫 대화를 완료해 별자리 전용 조리를 시작할 수 있게 합니다.
        /// </summary>
        public void CompleteNightIntroduction()
        {
            NightIntroductionComplete = true;
            session.Data.currentNightIntroductionComplete = true;
            GuestState.Apply(5, 4, 0);
            SyncGuestState();
        }

        /// <summary>
        /// 재료 손질·청소·도구 준비·스텔라·아기별 행동 중 완료한 항목을 기록합니다.
        /// </summary>
        public void CompletePreparationTask(string taskId)
        {
            if (!string.IsNullOrWhiteSpace(taskId) &&
                !session.Data.completedPreparationTaskIds.Contains(taskId))
            {
                session.Data.completedPreparationTaskIds.Add(taskId);
            }
        }

        /// <summary>
        /// 조리 결과가 제출 가능한 경우 손님 상태에 효과를 적용합니다.
        /// </summary>
        public CookingEvaluation SubmitCooking(CookingSelection selection)
        {
            if (CurrentChapter?.specialRecipe == null)
                throw new InvalidOperationException("현재 챕터에 전용 레시피가 없습니다.");

            var result = RecipeEvaluator.Evaluate(CurrentChapter.specialRecipe, selection);
            if (selection.magicalIngredientId != CurrentChapter.specialRecipe.magicalIngredientId)
                return new CookingEvaluation(false, 0f, CookingQuality.Low);
            var selectedIngredients = selection.steps
                .Where(step => step != null)
                .Select(step => step.ingredientId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToArray();
            if (selectedIngredients.Any(id => Inventory.GetQuantity(id) <= 0))
                return new CookingEvaluation(false, 0f, CookingQuality.Low);

            if (result.CanServe)
            {
                foreach (var ingredientId in selectedIngredients)
                    Inventory.TryConsume(ingredientId, 1);
                CookingResult = result;
                var effect = result.Quality switch
                {
                    CookingQuality.High => 35,
                    CookingQuality.Medium => 22,
                    _ => 10
                };
                GuestState.Apply(effect, effect, effect);
                session.Data.currentCookingComplete = true;
                session.Data.currentCookingQuality = result.Quality;
                RecordFoodClue(result.Quality);
                CollectEvidenceByCategory(EvidenceCategory.FoodReaction);
                SyncGuestState();
                // 조리 결과는 이후 대화와 추리 조건에 직접 영향을 주므로 화면 구현과 무관하게 즉시 저장한다.
                checkpointSaver?.Invoke();
            }
            return result;
        }

        /// <summary>
        /// 별자리·신화·핵심 증거를 제출하고 성공 시 기억공간 입장 상태를 갱신합니다.
        /// </summary>
        public DeductionResult SubmitDeduction(string zodiacId, string mythId, IEnumerable<string> evidenceIds)
        {
            var result = DeductionEvaluator.Evaluate(CurrentChapter.deduction, zodiacId, mythId, evidenceIds);
            DeductionComplete = result.Success;
            if (result.Success)
            {
                GuestState.Apply(15, 15, 25);
                session.Data.currentDeductionComplete = true;
                SyncGuestState();
                // 올바른 추리 직후 종료해도 기억공간 입구에서 이어갈 수 있어야 한다.
                checkpointSaver?.Invoke();
            }
            return result;
        }

        /// <summary>
        /// 기억공간 목표를 완료하고 모든 목표 완료 시 손님의 기억 상태를 복원합니다.
        /// </summary>
        public bool CompleteMemoryObjective(string objectiveId)
        {
            var completed = MemorySpace != null && MemorySpace.CompleteObjective(objectiveId);
            if (completed && !session.Data.completedMemoryObjectiveIds.Contains(objectiveId))
                session.Data.completedMemoryObjectiveIds.Add(objectiveId);
            if (completed && MemorySpace.IsComplete)
            {
                GuestState.Apply(10, 10, 100);
                SyncGuestState();
                // 마지막 기억 정화는 챕터 결과 직전의 복구 지점이다.
                checkpointSaver?.Invoke();
            }
            return completed;
        }

        /// <summary>
        /// 기억공간 실패 시 최근 체크포인트와 그 시점의 정화 목표를 복원합니다.
        /// </summary>
        public bool RestoreMemoryCheckpoint(out MemoryCheckpoint checkpoint)
        {
            checkpoint = default;
            if (MemorySpace == null || !MemorySpace.TryRestoreLastCheckpoint(out checkpoint))
                return false;
            session.Data.completedMemoryObjectiveIds =
                MemorySpace.CompletedObjectiveIds.ToList();
            return true;
        }

        /// <summary>
        /// 현재 단계의 완료 조건을 확인한 뒤 다음 단계로 이동합니다.
        /// </summary>
        public bool TryAdvance(out string reason)
        {
            reason = string.Empty;
            switch (flow.CurrentPhase)
            {
                case GamePhaseType.DayExploration when !Objectives.MandatoryObjectivesComplete:
                    reason = "필수 조사 목표를 먼저 완료해야 합니다.";
                    return false;
                case GamePhaseType.TavernPreparation when !PreparationComplete:
                    reason = "재료와 주점 준비를 완료해야 합니다.";
                    return false;
                case GamePhaseType.NightService when !NightIntroductionComplete:
                    reason = "손님의 입장 반응을 관찰하고 첫 대화를 마쳐야 합니다.";
                    return false;
                case GamePhaseType.NightService when CookingResult == null:
                    reason = "손님에게 별자리 전용 요리를 제공해야 합니다.";
                    return false;
                case GamePhaseType.Deduction when GuestState.TrustValue < 20:
                    reason = "기억공간에 들어가려면 손님의 신뢰가 최소 '낮음' 단계에 도달해야 합니다.";
                    return false;
                case GamePhaseType.Deduction when !DeductionComplete:
                    reason = "올바른 별자리와 신화를 추리해야 합니다.";
                    return false;
                case GamePhaseType.MemorySpace when MemorySpace is { IsComplete: false }:
                    reason = "오염된 기억을 모두 정화해야 합니다.";
                    return false;
                case GamePhaseType.ChapterResult when !ChapterFinalized:
                    reason = "손님의 미래 선택을 기록해야 합니다.";
                    return false;
            }

            var previous = flow.CurrentPhase;
            if (previous == GamePhaseType.DayExploration)
            {
                // 15시보다 일찍 돌아온 시간은 기본 120분의 준비 시간에 보너스로 더한다.
                session.Data.preparationBonusMinutes =
                    Mathf.Max(0, GameClock.DayEndMinute - session.Data.currentGameMinute);
            }
            if (!flow.TryAdvance())
                return false;

            if (previous == GamePhaseType.ChapterResult)
            {
                session.Data.completedObjectiveIds.Clear();
                session.Data.collectedEvidenceIds.Clear();
                session.Data.evidenceLinks.Clear();
                ResetCurrentChapterState();
                session.Data.currentChapterId = string.Empty;
                ReloadCurrentChapter();
                // 단계 변경 이벤트는 챕터 인덱스를 먼저 저장할 수 있으므로,
                // 새 챕터 ID와 초기 상태까지 재구성한 뒤 정상 체크포인트로 한 번 더 덮어쓴다.
                checkpointSaver?.Invoke();
            }
            return true;
        }

        /// <summary>
        /// 복원 등급과 손님의 선택을 저장 데이터에 기록합니다.
        /// </summary>
        public RestorationGrade FinalizeChapter(GuestFutureChoice futureChoice)
        {
            var grade = PreviewRestorationGrade();
            var allowedChoices = GetAvailableFutureChoices(grade);
            if (!allowedChoices.Contains(futureChoice))
                futureChoice = allowedChoices[0];

            var progress = session.Data.guestProgress.Find(item => item.characterId == CurrentChapter.guest.id);
            if (progress == null)
            {
                progress = new GuestProgressData { characterId = CurrentChapter.guest.id };
                session.Data.guestProgress.Add(progress);
            }

            progress.trust = GuestState.TrustStage;
            progress.stability = GuestState.StabilityStage;
            progress.memory = GuestState.MemoryStage;
            progress.restorationGrade = grade;
            progress.futureChoice = futureChoice;
            progress.completed = true;
            ChapterFinalized = true;
            // 개인 선택은 기록 보관소의 영구 결과이므로 결과 화면을 벗어나기 전에도 보존한다.
            checkpointSaver?.Invoke();
            return grade;
        }

        /// <summary>
        /// 현재 조리와 선택 목표 결과로 예상되는 복원 등급을 반환합니다.
        /// </summary>
        public RestorationGrade PreviewRestorationGrade()
        {
            return CookingResult?.Quality == CookingQuality.High &&
                   Objectives.All.Where(item => !item.Definition.mandatory).All(item => item.IsComplete)
                ? RestorationGrade.Complete
                : CookingResult?.Quality == CookingQuality.Low
                    ? RestorationGrade.Unstable
                    : RestorationGrade.Partial;
        }

        /// <summary>
        /// 복원 안정도에 따라 손님이 실제로 선택할 수 있는 미래를 반환합니다.
        /// </summary>
        public IReadOnlyList<GuestFutureChoice> GetAvailableFutureChoices(RestorationGrade grade)
        {
            return grade switch
            {
                RestorationGrade.Complete => new[]
                {
                    GuestFutureChoice.ReturnToSky,
                    GuestFutureChoice.RemainHumanWithMemories,
                    GuestFutureChoice.RemainHumanWithoutCelestialIdentity
                },
                RestorationGrade.Partial => new[]
                {
                    GuestFutureChoice.ReturnToSky,
                    GuestFutureChoice.RemainHumanWithMemories
                },
                _ => new[] { GuestFutureChoice.RemainHumanWithMemories }
            };
        }

        private void ApplyFurnitureReaction()
        {
            if (CurrentChapter?.guest == null)
                return;

            var placed = session.Data.furniturePlacements
                .Where(item => item != null && !item.stored)
                .Select(item => BuiltInChapterCatalog.FindFurniture(item.furnitureId))
                .Where(item => item != null);
            var reaction = FurnitureReactionEvaluator.Evaluate(CurrentChapter.guest, placed);
            GuestState.Apply(reaction.PositiveCount * 8, reaction.PositiveCount * 10 - reaction.NegativeCount * 8, 0);
            CollectEvidenceByCategory(EvidenceCategory.InteriorReaction);
            SyncGuestState();
        }

        private void CollectEvidenceByCategory(EvidenceCategory category)
        {
            var evidence = CurrentChapter?.evidence?.FirstOrDefault(item => item != null && item.category == category);
            if (evidence != null)
                CollectEvidence(evidence);
        }

        private void SyncGuestState()
        {
            session.Data.currentGuestTrust = GuestState.TrustValue;
            session.Data.currentGuestStability = GuestState.StabilityValue;
            session.Data.currentGuestMemory = GuestState.MemoryValue;
        }

        private void ResetCurrentChapterState()
        {
            session.Data.currentPreparationComplete = false;
            session.Data.completedPreparationTaskIds.Clear();
            session.Data.currentCookingComplete = false;
            session.Data.currentNightIntroductionComplete = false;
            session.Data.currentCookingQuality = CookingQuality.Low;
            session.Data.currentDeductionComplete = false;
            session.Data.completedMemoryObjectiveIds.Clear();
            session.Data.currentMemoryEchoHeard = false;
            session.Data.currentGuestTrust = 0;
            session.Data.currentGuestStability = 0;
            session.Data.currentGuestMemory = 0;
            session.Data.currentGameMinute = 540;
            session.Data.preparationBonusMinutes = 0;
        }

        private void NormalizeSaveCollections()
        {
            session.Data.inventory ??= new List<InventoryEntry>();
            session.Data.ownedFurnitureIds ??= new List<string>();
            session.Data.furniturePlacements ??= new List<FurniturePlacementData>();
            session.Data.collectedEvidenceIds ??= new List<string>();
            session.Data.completedObjectiveIds ??= new List<string>();
            session.Data.evidenceLinks ??= new List<EvidenceLinkData>();
            session.Data.guestProgress ??= new List<GuestProgressData>();
            session.Data.completedChapterIds ??= new List<string>();
            session.Data.readDialogueLineIds ??= new List<string>();
            session.Data.dialogueHistory ??= new List<DialogueHistoryEntry>();
            session.Data.storyFlagIds ??= new List<string>();
            session.Data.servedSideGuestIds ??= new List<string>();
            session.Data.foodClueRecords ??= new List<FoodClueRecord>();
            session.Data.completedMemoryObjectiveIds ??= new List<string>();
            session.Data.completedPreparationTaskIds ??= new List<string>();
            session.Data.settings ??= new GameSettingsData();
        }

        private void RecordFoodClue(CookingQuality quality)
        {
            var record = session.Data.foodClueRecords.Find(item => item.chapterId == CurrentChapter.id);
            if (record == null)
            {
                record = new FoodClueRecord { chapterId = CurrentChapter.id };
                session.Data.foodClueRecords.Add(record);
            }
            record.quality = quality;
            record.clarityText = quality switch
            {
                CookingQuality.High => "왜곡 없이 선명한 핵심 기억 단서",
                CookingQuality.Medium => "일부가 흐릿해 여러 해석이 가능한 기억 단서",
                _ => "오염의 영향이 남아 사실과 감정이 뒤섞인 단서"
            };
            record.effectLabels = CurrentChapter.specialRecipe.effects
                .Select(EffectLabel)
                .ToList();
        }

        private static string EffectLabel(CookingEffect effect) => effect switch
        {
            CookingEffect.Stability => "안정",
            CookingEffect.Trust => "신뢰",
            CookingEffect.Memory => "기억",
            CookingEffect.Truth => "진실",
            CookingEffect.Empathy => "공감",
            _ => "연결"
        };

        private static float ScoreFor(CookingQuality quality) => quality switch
        {
            CookingQuality.High => 1f,
            CookingQuality.Medium => 0.65f,
            _ => 0.25f
        };
    }
}
