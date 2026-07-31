using System.Collections;
using System.Linq;
using StarlightBar.Content;
using StarlightBar.Core;
using StarlightBar.Exploration;
using StarlightBar.Systems;
using StarlightBar.UI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace StarlightBar.Gameplay
{
    /// <summary>
    /// 혜화동의 시간 진행, 목표 배치, F 조사와 복귀 조건을 실제 플레이로 연결합니다.
    /// </summary>
    public sealed class DayExplorationPresenter : MonoBehaviour
    {
        [SerializeField, Tooltip("혜화동 씬에 배치된 HUD와 스폰 위치 참조입니다.")]
        private DayExplorationView view;
        [SerializeField, Tooltip("씬에 배치된 조사 상세 Presenter입니다.")]
        private InvestigationDetailPresenter detailPresenter;
        private GameRuntimeCoordinator runtime;
        private GameClock clock;
        private Transform player;
        private TMP_Text statusText;
        private TMP_Text objectiveText;
        private TMP_Text timeText;
        private UnityEngine.UI.Image timeDial;
        private TMP_Text contextPrompt;
        private TMP_Text notificationText;
        private GameObject returnActions;
        private UnityEngine.UI.Button returnButton;
        private bool finalTimeWarningShown;

        private void Start()
        {
            if (GameBootstrapper.Instance == null)
            {
                Debug.LogError("Bootstrap 씬에서 게임을 시작해야 합니다.");
                enabled = false;
                return;
            }

            runtime = GameBootstrapper.Instance.Runtime;
            player = Object.FindFirstObjectByType<PlayerMover2D>()?.transform;
            clock = new GameClock(GameBootstrapper.Instance.Session.Data.currentGameMinute);
            clock.MinuteChanged += minute =>
            {
                GameBootstrapper.Instance.Session.Data.currentGameMinute = minute;
                Refresh();
                TryShowFinalTimeWarning(minute);
            };
            clock.MandatoryObjectiveGraceStarted += () => StartCoroutine(ShowMandatoryGuidance());

            BindView();
            if (!enabled)
                return;
            SpawnObjectives();
            Refresh();
            TryShowFinalTimeWarning(clock.CurrentMinute);
        }

        private void Update()
        {
            if (runtime == null)
                return;

            // 낮 시간은 설정 화면에서만 멈추며 대화·조사·노트·망원경 중에는 계속 흐릅니다.
            clock.SetPaused(SettingsMenuPresenter.AnyOpen);
            clock.Tick(Time.deltaTime, runtime.Objectives.MandatoryObjectivesComplete);
            if (RuntimeDialoguePresenter.AnyPlaying || SettingsMenuPresenter.AnyOpen ||
                PathfinderNotebookPresenter.AnyOpen || RuntimeTelescopePresenter.AnyOpen ||
                InvestigationDetailPresenter.AnyOpen)
                return;
            if (GameInput.WasPressedThisFrame(GameInputAction.Inspect))
                TryInteract(false);
            if (GameInput.WasPressedThisFrame(GameInputAction.Talk))
                TryInteract(true);
        }

        private void SpawnObjectives()
        {
            var index = 0;
            foreach (var progress in runtime.Objectives.All.Where(item => !item.IsComplete))
            {
                var markerObject = Instantiate(
                    RuntimePrefabLibrary.Instance.objectiveMarkerPrefab,
                    transform);
                var spawnPoints = view.ObjectiveSpawnPoints;
                var positionIndex =
                    (runtime.CurrentChapter.chapterIndex * 3 + index * 2) % spawnPoints.Length;
                markerObject.transform.position = spawnPoints[positionIndex].position;
                var marker = markerObject.GetComponent<WorldObjectiveMarker>();
                marker.Initialize(progress.Definition);
                marker.InteractionRequested += BeginInteraction;
                index++;
            }
        }

        private void TryInteract(bool talking)
        {
            if (player == null)
                return;

            var marker = Object.FindObjectsByType<WorldObjectiveMarker>(FindObjectsSortMode.None)
                .Where(item => item != null)
                .OrderBy(item => Vector2.Distance(player.position, item.transform.position))
                .FirstOrDefault();

            if (marker == null || Vector2.Distance(player.position, marker.transform.position) > 1.4f)
            {
                SetStatus("조사 대상에 더 가까이 다가가세요.");
                return;
            }

            if (marker.RequiresTalk != talking)
            {
                SetStatus(marker.RequiresTalk
                    ? "이 대상은 조사물이 아니라 인물입니다. E키로 대화하세요."
                    : "가까이에서 F키로 조사하세요.");
                return;
            }

            if (!marker.IsTelescopeAnalyzed)
            {
                SetStatus("희미한 별자리 흔적입니다. 1키로 망원경을 열고 좌클릭으로 분석하세요.");
                return;
            }
            marker.Interact(player.gameObject);
        }

        private void BeginInteraction(WorldObjectiveMarker marker)
        {
            if (marker == null || marker.Definition == null)
                return;
            detailPresenter?.Show(marker.Definition, () => CompleteInteraction(marker, marker.Definition));
        }

        private void CompleteInteraction(WorldObjectiveMarker marker, ObjectiveDefinition definition)
        {
            if (marker == null || definition == null)
                return;
            if (runtime.CompleteObjective(definition.id))
            {
                clock.AdvanceMinutes(definition.timeCostMinutes, runtime.Objectives.MandatoryObjectivesComplete);
                SetStatus($"획득: {definition.title}");
                Destroy(marker.gameObject);
                Refresh();
                GameBootstrapper.Instance.SaveNow();
            }
        }

        private void BindView()
        {
            if (view == null || view.ObjectiveSpawnPoints == null ||
                view.ObjectiveSpawnPoints.Length == 0)
            {
                Debug.LogError("DayExplorationView 참조 또는 조사 스폰 위치가 없습니다.", this);
                enabled = false;
                return;
            }
            timeText = view.TimeText;
            timeDial = view.TimeDial;
            objectiveText = view.ObjectiveText;
            statusText = view.StatusText;
            contextPrompt = view.ContextPrompt;
            notificationText = view.NotificationText;
            returnActions = view.ReturnActions;
            returnButton = view.ReturnButton;
            Bind(returnButton, ReturnToTavern);
            Bind(view.ContinueExplorationButton, () =>
            {
                SetStatus("선택 목표를 계속 조사할 수 있습니다. 언제든 복귀 버튼을 누르세요.");
            });
            returnActions.SetActive(false);
        }

        private static void Bind(
            UnityEngine.UI.Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
                return;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        private void Refresh()
        {
            if (timeText != null)
            {
                timeText.text = $"현재 시각 {clock.CurrentMinute / 60:00}:{clock.CurrentMinute % 60:00}";
                var remaining = GameClock.DayEndMinute - clock.CurrentMinute;
                timeText.color = view.TimeColor(remaining);
                if (timeDial != null)
                {
                    timeDial.fillAmount = Mathf.InverseLerp(
                        GameClock.DayEndMinute, GameClock.DayStartMinute, clock.CurrentMinute);
                    timeDial.color = timeText.color;
                }
                if (Camera.main != null)
                {
                    var dayProgress = Mathf.InverseLerp(
                        GameClock.DayStartMinute, GameClock.DayEndMinute, clock.CurrentMinute);
                    Camera.main.backgroundColor = Color.Lerp(
                        view.MorningCameraColor, view.EveningCameraColor, dayProgress);
                }
            }
            if (objectiveText != null)
            {
                objectiveText.text = string.Join("\n", runtime.Objectives.All.Select(item =>
                    $"{(item.IsComplete ? "✓" : "□")} {(item.Definition.mandatory ? "[필수]" : "[선택]")} {item.Definition.title}"));
            }

            if (runtime.Objectives.MandatoryObjectivesComplete)
            {
                SetStatus("필수 조사를 완료했습니다. 선택 목표를 더 찾거나 Enter로 주점에 돌아가세요.");
                if (returnActions != null)
                    returnActions.SetActive(true);
                if (returnButton != null)
                {
                    var preparationMinutes = 120 + Mathf.Max(0, GameClock.DayEndMinute - clock.CurrentMinute);
                    returnButton.GetComponentInChildren<TMP_Text>().text =
                        $"주점으로 복귀 · 예상 준비 시간 {preparationMinutes}분";
                }
            }
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
                statusText.text = message;
            if (contextPrompt != null)
                contextPrompt.text = message;
            if (notificationText != null &&
                (message.StartsWith("획득:") || message.Contains("완료") || message.Contains("기록")))
                notificationText.text = message;
        }

        private void TryShowFinalTimeWarning(int currentMinute)
        {
            var remaining = GameClock.DayEndMinute - currentMinute;
            if (finalTimeWarningShown || remaining is <= 0 or > 30)
                return;

            finalTimeWarningShown = true;
            RuntimeAudioService.PlayUiConfirm();
            SetStatus("스텔라: 낮 시간이 30분도 남지 않았답니다. 필수 흔적을 우선 확인해 주세요.");
        }

        private void ReturnToTavern()
        {
            if (!runtime.TryAdvance(out var reason))
                SetStatus(reason);
        }

        private IEnumerator ShowMandatoryGuidance()
        {
            RuntimeAudioService.PlayUiConfirm();
            SetStatus("15:00 · 망원경이 남은 별자리 흔적에 반응합니다.");
            HighlightNearestMandatory();
            yield return new WaitForSecondsRealtime(1.4f);
            SetStatus("패스파인더 노트에 미완료 필수 목표가 표시되었습니다. J키로 확인하세요.");
            yield return new WaitForSecondsRealtime(1.4f);
            SetStatus("스텔라: 아직 닿지 못한 별빛이 있답니다. 시계는 기다려 줄 거예요.");
            yield return new WaitForSecondsRealtime(1.4f);
            SetStatus("아기별이 가장 가까운 필수 조사 표식 쪽으로 빛의 길을 만들었습니다.");
        }

        private static void HighlightNearestMandatory()
        {
            var marker = Object.FindObjectsByType<WorldObjectiveMarker>(FindObjectsSortMode.None)
                .FirstOrDefault(item => item.Definition.mandatory);
            if (marker == null)
                return;
            marker.ShowGuidance();
        }
    }
}
