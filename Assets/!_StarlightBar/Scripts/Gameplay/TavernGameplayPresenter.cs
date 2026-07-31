using System.Collections.Generic;
using System.Linq;
using StarlightBar.Content;
using StarlightBar.Core;
using StarlightBar.Systems;
using StarlightBar.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarlightBar.Gameplay
{
    /// <summary>
    /// 아침 브리핑, 준비, 조리, 추리, 결과 선택을 하나의 재사용 주점 씬에서 제공합니다.
    /// </summary>
    public sealed class TavernGameplayPresenter : MonoBehaviour
    {
        [Header("에디터에서 배치한 화면 참조")]
        [SerializeField] private TavernView view;
        [SerializeField] private RuntimeDialoguePresenter dialoguePresenter;
        [SerializeField] private FurniturePlacementPresenter furniturePresenter;

        private GameRuntimeCoordinator runtime;
        private RectTransform panel;
        private TMP_Text status;
        private string selectedZodiacId;
        private string selectedMythId;
        private string selectedEvidenceChapterId;
        private readonly HashSet<string> selectedDeductionEvidenceIds = new();
        private TMP_Text resultBody;

        private void Start()
        {
            if (GameBootstrapper.Instance == null)
            {
                Debug.LogError("Bootstrap 씬에서 게임을 시작해야 합니다.");
                return;
            }

            runtime = GameBootstrapper.Instance.Runtime;
            if (view == null || view.Content == null || dialoguePresenter == null)
            {
                Debug.LogError("Tavern 씬의 TavernView와 대화 Presenter 참조를 Inspector에서 연결해야 합니다.", this);
                enabled = false;
                return;
            }

            panel = view.Content;
            GameBootstrapper.Instance.Flow.PhaseChanged += OnPhaseChanged;
            BuildForPhase(GameBootstrapper.Instance.Flow.CurrentPhase);
        }

        private void OnDestroy()
        {
            if (GameBootstrapper.Instance != null)
                GameBootstrapper.Instance.Flow.PhaseChanged -= OnPhaseChanged;
        }

        private void OnPhaseChanged(GamePhaseType previous, GamePhaseType next)
        {
            // 준비·접객·추리는 같은 Tavern 씬을 공유하므로 씬 재로드 없이 화면을 다시 구성한다.
            if (GameSceneRouter.ResolveSceneName(next) == "Tavern")
                BuildForPhase(next);
        }

        private void BuildForPhase(GamePhaseType phase)
        {
            ClearDynamicChildren(panel);
            ClearDynamicChildren(view.CategoryContent);
            view.Root.SetActive(true);
            view.CategoryRoot.SetActive(phase == GamePhaseType.TavernPreparation);
            if (view.FurnitureRoot != null)
                view.FurnitureRoot.SetActive(phase == GamePhaseType.TavernPreparation);

            switch (phase)
            {
                case GamePhaseType.MorningBriefing:
                    BuildBriefing();
                    break;
                case GamePhaseType.TavernPreparation:
                    BuildPreparation();
                    BuildPreparationCategoryMenu();
                    break;
                case GamePhaseType.NightService:
                    BuildNightService();
                    break;
                case GamePhaseType.Deduction:
                    BuildDeduction();
                    break;
                case GamePhaseType.ChapterResult:
                    BuildResult();
                    break;
                case GamePhaseType.MidpointEvent:
                case GamePhaseType.LateGameEvent:
                    BuildStoryEvent(phase);
                    break;
            }
        }

        private void BuildBriefing()
        {
            DynamicContentFactory.CreateText(panel, runtime.CurrentChapter.title, 34);
            var dialogue = runtime.CurrentChapter.briefingDialogue;
            DynamicContentFactory.CreateText(
                panel,
                $"오늘의 손님: {runtime.CurrentChapter.guest.displayName}\n" +
                $"감정 주제: {runtime.CurrentChapter.emotionalTheme}\n" +
                $"낮 탐색에서 신화·인물·재료 흔적을 확인하세요.",
                23).GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 180;
            if (runtime.CurrentChapter.chapterIndex == 0)
            {
                DynamicContentFactory.CreateText(
                    panel,
                    "별자리 존재 규칙 · 인간이 이야기를 전하지 않으면 세부 기억, 능력, 이름과 정체성, " +
                    "천상의 형체 순서로 무너져 지상에 떨어집니다. 사실 기억보다 공포·죄책감·슬픔 같은 감정 상처가 오래 남으며, " +
                    "먼저 추락한 별들의 두려움과 고립이 남은 별들의 연쇄 붕괴를 재촉했습니다. " +
                    "스텔라는 평범한 인간 앞에서 힘을 드러낼 수 없고, 과도한 힘으로 주점이 불안정해지면 아기별이 북극성 팀에 자동 보고합니다.",
                    19).GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 150;
            }
            var startButton = DynamicContentFactory.CreateButton(panel, "혜화동 조사 시작", Advance);
            status = DynamicContentFactory.CreateText(panel, "스텔라의 브리핑을 듣는 중입니다.", 19);
            if (dialogue == null)
                return;
            startButton.interactable = false;
            dialoguePresenter.Play(dialogue, () =>
            {
                startButton.interactable = true;
                status.text = "오늘의 손님과 필요한 단서를 확인했습니다.";
            });
        }

        private void BuildPreparation()
        {
            DynamicContentFactory.CreateText(
                panel,
                $"주점 준비 · 기본 120분 + 조기 복귀 {GameBootstrapper.Instance.Session.Data.preparationBonusMinutes}분",
                34);
            DynamicContentFactory.CreateText(
                panel,
                $"보유 재료\n{FormatInventory()}\n\n수집 증거\n{FormatEvidence()}\n\n획득 가구\n{FormatFurniture()}",
                21).GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 350;
            if (furniturePresenter != null)
                furniturePresenter.Build(panel, runtime);
            DynamicContentFactory.CreateText(panel, "준비 행동", 26);
            CreatePreparationTask("ingredient_prep", "재료 손질·분류", "재료를 씻고 조리 순서에 맞게 분류했습니다.");
            CreatePreparationTask("tools_clean", "조리 도구 준비·주점 청소", "도구와 손님 자리를 정돈했습니다.");
            CreatePreparationTask(
                "recipe_review",
                "레시피 단서 검토",
                $"{runtime.CurrentChapter.specialRecipe.displayName}의 재료와 마법 재료를 확인했습니다.");
            CreatePreparationTask(
                "stella_share",
                "스텔라와 조사 결과 공유",
                "스텔라가 핵심 증거와 오염 가능성을 함께 점검했습니다.");
            CreatePreparationTask(
                "baby_star",
                "아기별 돌보기",
                "아기별들이 손님 자리 주변의 불안정한 별가루를 치웠습니다.");
            DynamicContentFactory.CreateButton(panel, "재료 손질·청소·가구 배치 완료", () =>
            {
                runtime.CompletePreparation();
                GameBootstrapper.Instance.SaveNow();
                Advance();
            });
            status = DynamicContentFactory.CreateText(panel, "레시피 단서와 손님의 불안 요인을 함께 확인하세요.", 19);
            if (runtime.PreparationComplete)
            {
                status.text = "저장된 준비 진행을 불러왔습니다.";
                DynamicContentFactory.CreateButton(panel, "야간 접객 계속", Advance);
            }
        }

        private void BuildPreparationCategoryMenu()
        {
            var categories = view.CategoryContent;
            DynamicContentFactory.CreateText(categories, "준비", 24);
            DynamicContentFactory.CreateButton(categories, "재료 준비",
                () => FocusPreparation(1f, "재료 손질과 분류 항목으로 이동했습니다."));
            DynamicContentFactory.CreateButton(categories, "인테리어",
                () => FocusPreparation(0.82f, "가구 배치 영역으로 이동했습니다."));
            DynamicContentFactory.CreateButton(categories, "레시피",
                () => FocusPreparation(0.42f, "레시피 검토 항목으로 이동했습니다."));
            DynamicContentFactory.CreateButton(categories, "아기별",
                () => FocusPreparation(0.12f, "아기별 상호작용 항목으로 이동했습니다."));
        }

        private void FocusPreparation(float normalizedPosition, string message)
        {
            var scroll = panel != null ? panel.GetComponentInParent<ScrollRect>() : null;
            if (scroll != null)
                scroll.verticalNormalizedPosition = Mathf.Clamp01(normalizedPosition);
            if (status != null)
                status.text = message;
        }

        private static void ClearDynamicChildren(Transform parent)
        {
            if (parent == null)
                return;

            for (var index = parent.childCount - 1; index >= 0; index--)
                Destroy(parent.GetChild(index).gameObject);
        }

        private void BuildNightService()
        {
            var recipe = runtime.CurrentChapter.specialRecipe;
            DynamicContentFactory.CreateText(panel, $"오늘의 손님: {runtime.CurrentChapter.guest.displayName}", 34);
            DynamicContentFactory.CreateText(
                panel,
                $"전용 메뉴: {recipe.displayName}\n재료와 조리법을 선택해 기억 반응을 이끌어내세요.",
                22);
            status = DynamicContentFactory.CreateText(panel, "조리 품질에 따라 기억 단서의 명확도가 달라집니다.", 19);
            new SideGuestServicePresenter(panel, runtime, message => status.text = message).Build();
            DynamicContentFactory.CreateText(panel, "별자리 손님 전용 조리", 27);
            if (!runtime.NightIntroductionComplete)
            {
                DynamicContentFactory.CreateText(
                    panel,
                    $"입장 관찰 · {runtime.CurrentChapter.traumaReaction}\n" +
                    $"가구 반응 · {runtime.CurrentChapter.guest.description}",
                    19).GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 130;
                DynamicContentFactory.CreateButton(panel, "손님을 맞이하고 첫 대화 시작", () =>
                {
                    runtime.CompleteNightIntroduction();
                    GameBootstrapper.Instance.SaveNow();
                    if (runtime.CurrentChapter.nightDialogue != null)
                        dialoguePresenter.Play(runtime.CurrentChapter.nightDialogue,
                            () => BuildForPhase(GamePhaseType.NightService));
                    else
                        BuildForPhase(GamePhaseType.NightService);
                });
                status.text = "손님의 외형·행동·가구 반응을 관찰한 뒤 첫 대화를 시작하세요.";
                return;
            }
            if (runtime.CookingResult is { } restored)
            {
                status.text = $"저장된 조리 결과 · 품질 {ToKorean(restored.Quality)}";
                DynamicContentFactory.CreateButton(panel, "대화를 마치고 추리하기", Advance);
            }
            else
            {
                new CookingSequencePresenter().Build(panel, recipe, Cook);
            }
        }

        private void BuildDeduction()
        {
            DynamicContentFactory.CreateText(panel, "최종 추리", 34);
            if (selectedEvidenceChapterId != runtime.CurrentChapter.id)
            {
                selectedEvidenceChapterId = runtime.CurrentChapter.id;
                selectedDeductionEvidenceIds.Clear();
                foreach (var evidence in runtime.Evidence.Collected.Where(item => item.coreEvidence))
                    selectedDeductionEvidenceIds.Add(evidence.id);
            }
            DynamicContentFactory.CreateText(panel, "핵심 증거 선택 · 버튼을 눌러 제출 목록을 바꾸세요.", 20);
            foreach (var evidence in runtime.Evidence.Collected)
                CreateEvidenceSelectionButton(evidence);
            status = DynamicContentFactory.CreateText(panel, "별자리와 신화 사건을 각각 선택하세요.", 20);

            DynamicContentFactory.CreateText(panel, "별자리 후보", 23);
            foreach (var candidateId in runtime.CurrentChapter.deduction.zodiacCandidateIds)
                CreateChoiceButton(BuiltInChapterCatalog.GetLabel(candidateId), candidateId, true);

            DynamicContentFactory.CreateText(panel, "신화 후보", 23);
            foreach (var candidateId in runtime.CurrentChapter.deduction.mythCandidateIds)
                CreateChoiceButton(BuiltInChapterCatalog.GetLabel(candidateId), candidateId, false);
            DynamicContentFactory.CreateButton(panel, "추리 제출", ShowDeductionConfirmation);
            if (runtime.DeductionComplete)
            {
                status.text = "저장된 추리 성공 기록을 불러왔습니다.";
                DynamicContentFactory.CreateButton(panel, "기억공간 진입", Advance);
            }
        }

        private void BuildResult()
        {
            DynamicContentFactory.CreateText(panel, "기억 복원 결과", 34);
            var progress = GameBootstrapper.Instance.Session.Data.guestProgress
                .Find(item => item.characterId == runtime.CurrentChapter.guest.id);
            DynamicContentFactory.CreateText(
                panel,
                $"별자리 정체 · {BuiltInChapterCatalog.GetLabel(runtime.CurrentChapter.deduction.correctZodiacId)}\n" +
                $"복원 신화 · {BuiltInChapterCatalog.GetLabel(runtime.CurrentChapter.deduction.correctMythId)}\n" +
                $"복원 등급 · {(progress == null ? "판정 전" : ToKorean(progress.restorationGrade))}\n" +
                $"최종 선택 · {(progress == null ? "기록 전" : FutureChoiceLabel(progress.futureChoice))}",
                20).GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 155;
            resultBody = DynamicContentFactory.CreateText(
                panel,
                runtime.ChapterFinalized
                    ? ResolveResultNarrative()
                    : $"{runtime.CurrentChapter.guest.displayName}의 기억이 하나의 이야기로 이어졌습니다.",
                22);
            resultBody.GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 180;
            var previewGrade = runtime.PreviewRestorationGrade();
            status = DynamicContentFactory.CreateText(
                panel, $"예상 등급 {ToKorean(previewGrade)} · 손님이 선택 가능한 미래를 기록하세요.", 20);
            foreach (var choice in runtime.GetAvailableFutureChoices(previewGrade))
            {
                var captured = choice;
                DynamicContentFactory.CreateButton(panel, FutureChoiceLabel(choice), () => Finalize(captured));
            }
            if (runtime.ChapterFinalized)
            {
                status.text = "저장된 복원 결과와 미래 선택을 불러왔습니다.";
                DynamicContentFactory.CreateButton(panel, "기록 보관소 보기", ShowCurrentArchive);
                DynamicContentFactory.CreateButton(panel, "다음 챕터", Advance);
            }
        }

        private void BuildStoryEvent(GamePhaseType phase)
        {
            var midpoint = phase == GamePhaseType.MidpointEvent;
            DynamicContentFactory.CreateText(panel, midpoint ? "검은 별자리의 흔적" : "마지막 항로", 34);
            DynamicContentFactory.CreateText(
                panel,
                midpoint
                    ? "여섯 번째 복원 기록마다 같은 검은 별자리 문양이 숨어 있었습니다.\n\n" +
                      "주점의 별빛이 흔들리는 사이 패스파인더 노트의 문장이 바뀌고, 존재하지 않는 신화가 진짜 증거처럼 끼어들었습니다.\n\n" +
                      "기억공간에는 초대받지 않은 검은 형체가 나타났고, 은하 행정국 내부의 항로 정보가 유출됐다는 기록이 도착했습니다.\n\n" +
                      "추락은 자연재해가 아니었습니다. 고대 별자리 세력이 황도 12궁의 상징을 차지하려고 인간의 기억을 의도적으로 돌린 것입니다."
                    : "고대 별자리 세력의 항로가 드러나자 스텔라는 모든 책임을 혼자 감당하려 주점을 떠났습니다.\n\n" +
                      "불안정해진 주점에서 주인공은 스텔라가 흔들리지 않는 기준점이 아니라, 도움을 받아도 되는 한 사람임을 일깨웠습니다.\n\n" +
                      "각자의 미래를 선택한 열두 별이 하늘과 지상에서 동시에 빛을 보내 결계를 유지했고, 함께 복구한 항로가 검은 별자리의 계획을 멈췄습니다.\n\n" +
                      "이제 스텔라가 북극성으로 돌아갈 시간입니다.",
                22).GetComponent<UnityEngine.UI.LayoutElement>().preferredHeight = 430;
            DynamicContentFactory.CreateButton(panel, "계속", () =>
            {
                var flag = midpoint ? "midpoint_conspiracy_revealed" : "late_antagonist_stopped";
                var flags = GameBootstrapper.Instance.Session.Data.storyFlagIds;
                if (!flags.Contains(flag))
                    flags.Add(flag);
                GameBootstrapper.Instance.SaveNow();
                Advance();
            });
        }

        private void Cook(CookingSelection selection)
        {
            var evaluation = runtime.SubmitCooking(selection);
            status.text = evaluation.CanServe
                ? $"조리 완료 · 품질 {ToKorean(evaluation.Quality)} · 기억 반응이 시작되었습니다."
                : "관련 없는 재료이거나 마법 재료가 맞지 않아 제공할 수 없습니다. 다시 조리하세요.";

            if (evaluation.CanServe)
            {
                GameBootstrapper.Instance.SaveNow();
                var clue = GameBootstrapper.Instance.Session.Data.foodClueRecords
                    .Find(item => item.chapterId == runtime.CurrentChapter.id);
                if (clue != null)
                    status.text += $"\n음식 반응 기록 · {clue.clarityText}";
                DynamicContentFactory.CreateButton(panel, "음식 반응을 기록하고 추리하기", Advance);
            }
        }

        private void SubmitDeduction()
        {
            if (string.IsNullOrEmpty(selectedZodiacId) || string.IsNullOrEmpty(selectedMythId))
            {
                status.text = "별자리와 신화를 모두 선택해야 합니다.";
                return;
            }

            var result = runtime.SubmitDeduction(
                selectedZodiacId, selectedMythId, selectedDeductionEvidenceIds);
            if (!result.Success)
            {
                status.text = result.MissingEvidenceIds.Count > 0
                    ? "핵심 증거가 부족합니다. 노트의 증거를 다시 확인하세요."
                    : "추리가 맞지 않습니다. 대화와 신화 기록을 다시 검토하세요.";
                return;
            }

            status.text = "추리가 일치했습니다. 스텔라가 기억공간의 문을 엽니다.";
            GameBootstrapper.Instance.SaveNow();
            DynamicContentFactory.CreateButton(panel, "기억공간 진입", Advance);
        }

        private void ShowDeductionConfirmation()
        {
            if (string.IsNullOrEmpty(selectedZodiacId) || string.IsNullOrEmpty(selectedMythId))
            {
                status.text = "별자리와 신화를 모두 선택해야 합니다.";
                return;
            }

            status.text =
                $"{BuiltInChapterCatalog.GetLabel(selectedZodiacId)} / " +
                $"{BuiltInChapterCatalog.GetLabel(selectedMythId)}\n" +
                $"핵심 증거 {selectedDeductionEvidenceIds.Count}개로 제출할까요?";
            DynamicContentFactory.CreateButton(panel, "확인 · 추리 제출", SubmitDeduction);
            DynamicContentFactory.CreateButton(panel, "취소 · 다시 검토", () =>
                status.text = "노트와 대화를 다시 검토한 뒤 제출하세요.");
        }

        private void CreateEvidenceSelectionButton(EvidenceDefinition evidence)
        {
            Button button = null;
            void RefreshLabel()
            {
                if (button == null)
                    return;
                var selected = selectedDeductionEvidenceIds.Contains(evidence.id);
                button.GetComponentInChildren<TMP_Text>().text =
                    $"{(selected ? "✓" : "□")} {(evidence.coreEvidence ? "[핵심]" : "[보조]")} {evidence.title}";
            }

            button = DynamicContentFactory.CreateButton(panel, evidence.title, () =>
            {
                if (!selectedDeductionEvidenceIds.Add(evidence.id))
                    selectedDeductionEvidenceIds.Remove(evidence.id);
                RefreshLabel();
                status.text = $"제출할 증거 {selectedDeductionEvidenceIds.Count}개를 선택했습니다.";
            });
            RefreshLabel();
        }

        private void CreateChoiceButton(string label, string id, bool zodiac)
        {
            DynamicContentFactory.CreateButton(panel, label, () =>
            {
                if (zodiac) selectedZodiacId = id;
                else selectedMythId = id;
                status.text =
                    $"선택: {BuiltInChapterCatalog.GetLabel(selectedZodiacId)} / {BuiltInChapterCatalog.GetLabel(selectedMythId)}";
            });
        }

        private void CreatePreparationTask(string id, string label, string completedMessage)
        {
            var button = DynamicContentFactory.CreateButton(panel, label, () =>
            {
                runtime.CompletePreparationTask(id);
                status.text = completedMessage;
                GameBootstrapper.Instance.SaveNow();
            });
            if (GameBootstrapper.Instance.Session.Data.completedPreparationTaskIds.Contains(id))
            {
                button.interactable = false;
                button.GetComponentInChildren<TMP_Text>().text = $"완료 · {label}";
            }
        }

        private void Finalize(GuestFutureChoice choice)
        {
            var grade = runtime.FinalizeChapter(choice);
            if (resultBody != null)
                resultBody.text = ResolveResultNarrative();
            status.text = $"복원 등급: {ToKorean(grade)} · 선택이 기록 보관소에 저장되었습니다.";
            GameBootstrapper.Instance.SaveNow();
            DynamicContentFactory.CreateButton(panel, "기록 보관소 보기", ShowCurrentArchive);
            DynamicContentFactory.CreateButton(panel, "다음 챕터", Advance);
        }

        private void ShowCurrentArchive()
        {
            var progress = GameBootstrapper.Instance.Session.Data.guestProgress
                .Find(item => item.characterId == runtime.CurrentChapter.guest.id);
            status.text = progress == null
                ? "아직 기록된 복원 결과가 없습니다."
                : $"기록 보관소 · {runtime.CurrentChapter.guest.displayName} · " +
                  $"{ToKorean(progress.restorationGrade)} · {FutureChoiceLabel(progress.futureChoice)}";
        }

        private void Advance()
        {
            if (!runtime.TryAdvance(out var reason) && status != null)
                status.text = reason;
        }

        private string FormatInventory()
        {
            var data = GameBootstrapper.Instance.Session.Data.inventory;
            return data.Count == 0
                ? "없음"
                : string.Join("\n", data.Select(item =>
                    $"{BuiltInChapterCatalog.GetLabel(item.itemId)} × {item.quantity}"));
        }

        private string FormatEvidence()
        {
            return runtime.Evidence.Collected.Count == 0
                ? "없음"
                : string.Join("\n", runtime.Evidence.Collected.Select(item => $"• {item.title}"));
        }

        private string FormatFurniture()
        {
            return runtime.CurrentChapter.obtainableFurniture.Count == 0
                ? "없음"
                : string.Join("\n", runtime.CurrentChapter.obtainableFurniture.Select(item => item.displayName));
        }

        private static CookingSelection CreateHighQualitySelection(RecipeDefinition recipe)
        {
            return new CookingSelection
            {
                steps = recipe.steps.Select(CloneStep).ToList(),
                decorationId = recipe.decorationId,
                magicalIngredientId = recipe.magicalIngredientId
            };
        }

        private static CookingSelection CreateMediumQualitySelection(RecipeDefinition recipe)
        {
            var steps = recipe.steps.Select(CloneStep).ToList();
            if (steps.Count > 1) steps[1].method = CookingMethod.Boil;
            return new CookingSelection
            {
                steps = steps,
                decorationId = recipe.decorationId,
                magicalIngredientId = recipe.magicalIngredientId
            };
        }

        private static CookingSelection CreateLowQualitySelection(RecipeDefinition recipe)
        {
            var firstId = recipe.allowedIngredientIds.FirstOrDefault() ?? "unknown";
            return new CookingSelection
            {
                steps = new List<RecipeStep>
                    { new() { ingredientId = firstId, method = CookingMethod.Raw, order = 9 } },
                decorationId = "decoration_wrong",
                magicalIngredientId = recipe.magicalIngredientId
            };
        }

        private static RecipeStep CloneStep(RecipeStep step)
        {
            return new RecipeStep
            {
                ingredientId = step.ingredientId,
                method = step.method,
                order = step.order,
                required = step.required
            };
        }

        private static string ToKorean(CookingQuality quality) => quality switch
        {
            CookingQuality.High => "상",
            CookingQuality.Medium => "중",
            _ => "하"
        };

        private static string ToKorean(RestorationGrade grade) => grade switch
        {
            RestorationGrade.Complete => "완전 복원",
            RestorationGrade.Partial => "부분 복원",
            _ => "불안정 복원"
        };

        private static string FutureChoiceLabel(GuestFutureChoice choice) => choice switch
        {
            GuestFutureChoice.ReturnToSky => "별자리로 하늘에 돌아감",
            GuestFutureChoice.RemainHumanWithMemories => "기억을 간직한 인간으로 남음",
            _ => "천상의 정체성을 놓고 인간으로 남음"
        };

        private string ResolveResultNarrative()
        {
            var progress = GameBootstrapper.Instance.Session.Data.guestProgress
                .Find(item => item.characterId == runtime.CurrentChapter.guest.id);
            var restorationText = progress?.restorationGrade switch
            {
                RestorationGrade.Complete => runtime.CurrentChapter.completeRestorationText,
                RestorationGrade.Unstable => runtime.CurrentChapter.unstableRestorationText,
                _ => runtime.CurrentChapter.partialRestorationText
            };
            if (progress == null)
                return restorationText;

            var choiceText = progress.futureChoice switch
            {
                GuestFutureChoice.ReturnToSky =>
                    ChoiceTextOrFallback(
                        runtime.CurrentChapter.returnToSkyChoiceText,
                        $"{runtime.CurrentChapter.guest.displayName}은 복원된 이름을 품고 하늘의 자리로 돌아가기로 했다."),
                GuestFutureChoice.RemainHumanWithMemories =>
                    ChoiceTextOrFallback(
                        runtime.CurrentChapter.remainHumanWithMemoriesChoiceText,
                        $"{runtime.CurrentChapter.guest.displayName}은 별의 기억을 간직한 채 지금의 인간 생활을 이어 가기로 했다."),
                _ =>
                    ChoiceTextOrFallback(
                        runtime.CurrentChapter.remainHumanWithoutIdentityChoiceText,
                        $"{runtime.CurrentChapter.guest.displayName}은 천상의 정체성을 놓아주고 스스로 선택한 인간의 삶에 남기로 했다.")
            };
            return $"{restorationText}\n\n{choiceText}";
        }

        private static string ChoiceTextOrFallback(string chapterText, string fallback)
        {
            return string.IsNullOrWhiteSpace(chapterText) ? fallback : chapterText;
        }
    }
}
