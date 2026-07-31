using System.Linq;
using StarlightBar.Content;
using StarlightBar.Core;
using StarlightBar.Systems;
using StarlightBar.UI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace StarlightBar.Gameplay
{
    /// <summary>
    /// J키로 여는 패스파인더 노트와 목표·증거·후보·음식·인물·기록 탭을 제공합니다.
    /// </summary>
    public sealed class PathfinderNotebookPresenter : MonoBehaviour
    {
        [SerializeField, Tooltip("씬에 배치된 패스파인더 노트 화면 참조입니다.")]
        private NotebookView view;
        private Canvas canvas;
        private GameObject notebookRoot;
        private RectTransform content;
        private TMP_Text status;
        private GameRuntimeCoordinator runtime;
        public static bool AnyOpen { get; private set; }

        private void Start()
        {
            if (GameBootstrapper.Instance == null)
                return;
            runtime = GameBootstrapper.Instance.Runtime;
            if (view == null)
            {
                Debug.LogError("NotebookView 참조가 연결되지 않았습니다.", this);
                enabled = false;
                return;
            }
            canvas = view.GetComponentInParent<Canvas>();
            notebookRoot = view.Root;
            content = view.Content;
            status = view.Status;
            Bind(view.ObjectivesTab, ShowObjectives);
            Bind(view.EvidenceTab, ShowEvidence);
            Bind(view.CandidatesTab, ShowCandidates);
            Bind(view.MythsTab, ShowMyths);
            Bind(view.FoodTab, ShowFood);
            Bind(view.PersonTab, ShowPerson);
            Bind(view.ArchiveTab, ShowArchive);
            Bind(view.CloseButton, Close);
            notebookRoot.SetActive(false);
        }

        private void Update()
        {
            if (RuntimeDialoguePresenter.AnyPlaying || SettingsMenuPresenter.AnyOpen)
                return;

            if (GameInput.WasPressedThisFrame(GameInputAction.Notebook) && notebookRoot != null)
            {
                notebookRoot.SetActive(!notebookRoot.activeSelf);
                AnyOpen = notebookRoot.activeSelf;
                if (notebookRoot.activeSelf)
                    ShowObjectives();
            }
            else if (GameInput.WasPressedThisFrame(GameInputAction.Objectives) && notebookRoot != null)
            {
                notebookRoot.SetActive(!notebookRoot.activeSelf);
                AnyOpen = notebookRoot.activeSelf;
                if (notebookRoot.activeSelf)
                    ShowObjectives();
            }
            else if (GameInput.WasPressedThisFrame(GameInputAction.Menu) && notebookRoot != null &&
                     notebookRoot.activeSelf)
            {
                notebookRoot.SetActive(false);
                AnyOpen = false;
            }
        }

        private void Close()
        {
            notebookRoot.SetActive(false);
            AnyOpen = false;
        }

        private static void Bind(Button button, UnityEngine.Events.UnityAction action)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        private void ShowObjectives()
        {
            var panel = ResetContent("오늘의 목표");
            foreach (var objective in runtime.Objectives.All)
            {
                DynamicContentFactory.CreateText(
                    panel,
                    $"{(objective.IsComplete ? "✓" : "□")} {(objective.Definition.mandatory ? "[필수]" : "[선택]")} " +
                    $"{objective.Definition.title}\n<size=75%>{objective.Definition.description}</size>",
                    22);
            }
        }

        private void ShowEvidence()
        {
            var panel = ResetContent("증거 카드 · 관련 카드 위로 드래그해 연결");
            if (GameBootstrapper.Instance.Session.Data.storyFlagIds.Contains("midpoint_conspiracy_revealed"))
            {
                DynamicContentFactory.CreateText(
                    panel,
                    "⚠ 조작 흔적 · 출처 없는 검은 신화 기록이 삽입되었다가 삭제되었습니다. " +
                    "아버지의 필체 및 수집한 원전과 일치하지 않습니다.",
                    19);
            }
            foreach (var evidence in runtime.Evidence.Collected)
            {
                var card = Instantiate(view.EvidenceCardPrefab, panel);
                card.Initialize(evidence, canvas, LinkEvidence);
            }
        }

        private void ShowCandidates()
        {
            var panel = ResetContent("별자리 후보");
            foreach (var id in runtime.CurrentChapter.deduction.zodiacCandidateIds)
            {
                DynamicContentFactory.CreateText(
                    panel, $"{ToZodiacName(id)} · {ToKorean(runtime.Evidence.GetConfidence(id))}", 23);
            }
        }

        private void ShowMyths()
        {
            var panel = ResetContent("신화 후보");
            foreach (var id in runtime.CurrentChapter.deduction.mythCandidateIds)
                DynamicContentFactory.CreateText(panel, ToMythName(id), 23);
        }

        private void ShowFood()
        {
            var panel = ResetContent("음식 기록");
            var recipe = runtime.CurrentChapter.specialRecipe;
            var clue = GameBootstrapper.Instance.Session.Data.foodClueRecords
                .Find(item => item.chapterId == runtime.CurrentChapter.id);
            DynamicContentFactory.CreateText(
                panel,
                $"{recipe.displayName}\n재료: {string.Join(", ", recipe.steps.Select(step => BuiltInChapterCatalog.GetLabel(step.ingredientId)))}\n" +
                $"현재 결과: {(runtime.CookingResult.HasValue ? ToKorean(runtime.CookingResult.Value.Quality) : "아직 조리하지 않음")}\n" +
                $"단서 명확도: {clue?.clarityText ?? "아직 기록 없음"}\n" +
                $"발현 효과: {(clue == null ? "-" : string.Join(" · ", clue.effectLabels))}",
                23);
        }

        private void ShowPerson()
        {
            var panel = ResetContent("인물 기록");
            DynamicContentFactory.CreateText(
                panel,
                $"{runtime.CurrentChapter.guest.displayName}\n{runtime.CurrentChapter.guest.description}\n\n" +
                $"신뢰: {GuestStateModel.ToKorean(runtime.GuestState.TrustStage)}\n" +
                $"안정: {GuestStateModel.ToKorean(runtime.GuestState.StabilityStage)}\n" +
                $"기억: {GuestStateModel.ToKorean(runtime.GuestState.MemoryStage)}",
                23);
        }

        private void ShowArchive()
        {
            var panel = ResetContent("복원 기록 보관소");
            DynamicContentFactory.CreateText(
                panel,
                "아버지의 관측 노트 · “별은 길을 잃은 사람에게 답을 주기보다, " +
                "다시 질문할 방향을 비춰 준다.”\n낡은 망원경의 경통에는 스텔라의 북극성 문양이 남아 있다.",
                20);
            if (GameBootstrapper.Instance.Session.Data.storyFlagIds.Contains("midpoint_conspiracy_revealed"))
                DynamicContentFactory.CreateText(
                    panel,
                    "보안 기록 · 여섯 번째 복원 뒤 동일한 검은 문양, 조작된 문장, " +
                    "은하 행정국 내부 접속 흔적을 확인함.",
                    20);
            var progress = GameBootstrapper.Instance.Session.Data.guestProgress;
            if (progress.Count == 0)
            {
                DynamicContentFactory.CreateText(panel, "아직 복원된 별자리가 없습니다.", 23);
                return;
            }

            foreach (var item in progress)
                DynamicContentFactory.CreateText(
                    panel,
                    $"{BuiltInChapterCatalog.GetLabel(item.characterId)} · " +
                    $"{ToKorean(item.restorationGrade)} · {ToKorean(item.futureChoice)}",
                    22);
        }

        private RectTransform ResetContent(string title)
        {
            for (var index = content.childCount - 1; index >= 0; index--)
                Destroy(content.GetChild(index).gameObject);
            DynamicContentFactory.CreateText(content, title, 30);
            return content;
        }

        private void OnDestroy()
        {
            AnyOpen = false;
        }

        private void LinkEvidence(EvidenceDefinition first, EvidenceDefinition second)
        {
            if (!runtime.Evidence.TryLink(first.id, second.id))
            {
                status.text = "두 증거 사이에서 유효한 새 관계를 찾지 못했습니다.";
                return;
            }

            var data = GameBootstrapper.Instance.Session.Data.evidenceLinks;
            data.Add(new EvidenceLinkData
            {
                firstEvidenceId = first.id,
                secondEvidenceId = second.id
            });
            GameBootstrapper.Instance.SaveNow();
            status.text = $"연결됨: {first.title} ↔ {second.title}";
        }

        private static string ToKorean(CandidateConfidence confidence) => confidence switch
        {
            CandidateConfidence.High => "높음",
            CandidateConfidence.Medium => "보통",
            CandidateConfidence.Low => "낮음",
            _ => "제외"
        };

        private static string ToZodiacName(string id) => BuiltInChapterCatalog.GetLabel(id);

        private static string ToMythName(string id) => BuiltInChapterCatalog.GetLabel(id);

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

        private static string ToKorean(GuestFutureChoice choice) => choice switch
        {
            GuestFutureChoice.ReturnToSky => "하늘로 복귀",
            GuestFutureChoice.RemainHumanWithMemories => "기억을 지닌 인간",
            _ => "천상의 정체성을 놓은 인간"
        };
    }
}
