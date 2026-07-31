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
    /// 기억공간의 이동, 감정 장애물 회피, 파편 정화와 완료 조건을 제공합니다.
    /// </summary>
    public sealed class MemorySpacePresenter : MonoBehaviour
    {
        [SerializeField, Tooltip("기억공간 씬에 배치된 HUD와 스폰 위치 참조입니다.")]
        private MemorySpaceView view;
        [SerializeField, Tooltip("씬에 배치된 별자리별 기억 기믹 제어기입니다.")]
        private MemoryMechanicDirector mechanicDirector;
        private GameRuntimeCoordinator runtime;
        private Transform player;
        private TMP_Text status;
        private TMP_Text objectiveText;

        private void Start()
        {
            if (GameBootstrapper.Instance == null)
            {
                Debug.LogError("Bootstrap 씬에서 게임을 시작해야 합니다.");
                return;
            }

            runtime = GameBootstrapper.Instance.Runtime;
            if (view == null || mechanicDirector == null)
            {
                Debug.LogError("MemorySpaceView 또는 MemoryMechanicDirector 참조가 없습니다.", this);
                enabled = false;
                return;
            }
            objectiveText = view.ObjectiveText;
            status = view.StatusText;
            player = CreatePlayer();
            runtime.MemorySpace?.AddCheckpoint("entry", player.position);
            mechanicDirector.Initialize(runtime.MemorySpace.Definition, player, this, objectiveText.transform.parent);
            SpawnFragments();
            SpawnMemoryEcho();
            Refresh();
        }

        private void Update()
        {
            if (RuntimeDialoguePresenter.AnyPlaying || SettingsMenuPresenter.AnyOpen ||
                PathfinderNotebookPresenter.AnyOpen || RuntimeTelescopePresenter.AnyOpen)
                return;
            if (GameInput.WasPressedThisFrame(GameInputAction.Inspect))
                TryPurify();
            if (GameInput.WasPressedThisFrame(GameInputAction.Talk))
                TryTalkOrProtect();
        }

        private Transform CreatePlayer()
        {
            var existing = Object.FindFirstObjectByType<PlayerMover2D>();
            if (existing != null)
                return existing.transform;

            var playerObject = Instantiate(
                RuntimePrefabLibrary.Instance.memoryPlayerPrefab,
                view.PlayerSpawnPoint.position,
                view.PlayerSpawnPoint.rotation,
                view.PlayerSpawnPoint.parent);
            playerObject.name = "MemoryPlayer";
            return playerObject.transform;
        }

        private void SpawnFragments()
        {
            var objectives = runtime.MemorySpace?.Definition.objectiveIds;
            if (objectives == null)
                return;

            for (var index = 0; index < objectives.Count; index++)
            {
                if (runtime.MemorySpace.CompletedObjectiveIds.Contains(objectives[index]))
                    continue;
                var gameObject = Instantiate(
                    RuntimePrefabLibrary.Instance.trueMemoryFragmentPrefab,
                    view.FragmentRoot);
                var positions = mechanicDirector.FragmentPositions;
                gameObject.transform.position = positions[index % positions.Count];
                var role = index switch
                {
                    1 => MemoryFragmentRole.KeyMemory,
                    2 => MemoryFragmentRole.Acceptance,
                    _ => MemoryFragmentRole.Truth
                };
                var fragment = gameObject.GetComponent<MemoryFragmentMarker>();
                fragment.Initialize(objectives[index], false, index == 0, role);
                fragment.InteractionRequested += HandleFragmentInteraction;
            }

            for (var index = 0; index < mechanicDirector.FalseMemoryCount; index++)
            {
                var falseMemory = Instantiate(
                    RuntimePrefabLibrary.Instance.falseMemoryFragmentPrefab,
                    view.FragmentRoot);
                falseMemory.transform.position = new Vector3(-0.8f + index * 1.6f, 2.9f, 0);
                var fragment = falseMemory.GetComponent<MemoryFragmentMarker>();
                fragment.Initialize(string.Empty, true, true);
                fragment.InteractionRequested += HandleFragmentInteraction;
            }
        }

        private void TryPurify()
        {
            var fragment = Object.FindObjectsByType<MemoryFragmentMarker>(FindObjectsSortMode.None)
                .OrderBy(item => Vector2.Distance(player.position, item.transform.position))
                .FirstOrDefault();
            if (fragment == null || Vector2.Distance(player.position, fragment.transform.position) > 1.3f)
            {
                SetStatus("오염 파편에 더 가까이 다가가세요.");
                return;
            }

            if (!fragment.IsAnalyzed)
            {
                SetStatus("진짜 기억인지 판별할 수 없습니다. 1키로 망원경을 열어 분석하세요.");
                return;
            }

            PurifyFragment(fragment);
        }

        private void PurifyFragment(MemoryFragmentMarker fragment)
        {
            if (fragment == null)
                return;

            if (fragment.IsFalseMemory)
            {
                Destroy(fragment.gameObject);
                SetStatus("오염이 만든 가짜 기억을 판별해 제거했습니다.");
                return;
            }

            if (fragment.Role == MemoryFragmentRole.KeyMemory && !fragment.IsProtected)
            {
                SetStatus("핵심 기억이 오염에 노출되어 있습니다. 가까이에서 E키로 스텔라의 결계를 고정하세요.");
                return;
            }
            if (fragment.Role == MemoryFragmentRole.Acceptance &&
                !GameBootstrapper.Instance.Session.Data.currentMemoryEchoHeard)
            {
                SetStatus("아직 손님의 목소리를 듣지 못했습니다. 빛나는 인물의 메아리 가까이에서 E키로 대화하세요.");
                return;
            }

            if (mechanicDirector != null && !mechanicDirector.CanPurify(fragment, out var reason))
            {
                SetStatus(reason);
                return;
            }

            if (runtime.CompleteMemoryObjective(fragment.ObjectiveId))
            {
                Destroy(fragment.gameObject);
                SetStatus($"정화 완료: {ToKorean(fragment.ObjectiveId)}");
                runtime.MemorySpace?.AddCheckpoint(fragment.ObjectiveId, player.position);
                Refresh();
                GameBootstrapper.Instance.SaveNow();
            }
        }

        private void SpawnMemoryEcho()
        {
            if (GameBootstrapper.Instance.Session.Data.currentMemoryEchoHeard)
                return;
            var echo = Instantiate(
                RuntimePrefabLibrary.Instance.memoryEchoPrefab,
                new Vector3(0f, -0.25f, 0f),
                Quaternion.identity,
                view.FragmentRoot);
            var marker = echo.GetComponent<MemoryEchoMarker>();
            marker.Initialize(runtime.MemorySpace.Definition.palette);
            marker.InteractionRequested += HandleMemoryEchoInteraction;
        }

        private void TryTalkOrProtect()
        {
            var keyMemory = Object.FindObjectsByType<MemoryFragmentMarker>(FindObjectsSortMode.None)
                .Where(item => item.Role == MemoryFragmentRole.KeyMemory)
                .OrderBy(item => Vector2.Distance(player.position, item.transform.position))
                .FirstOrDefault();
            if (keyMemory != null && Vector2.Distance(player.position, keyMemory.transform.position) <= 1.35f)
            {
                keyMemory.Interact(player.gameObject);
                return;
            }

            var echo = Object.FindFirstObjectByType<MemoryEchoMarker>();
            if (echo != null && Vector2.Distance(player.position, echo.transform.position) <= 1.5f)
            {
                echo.Interact(player.gameObject);
                return;
            }
            SetStatus("대화하거나 보호할 기억 오브젝트에 더 가까이 다가가세요.");
        }

        private void HandleFragmentInteraction(MemoryFragmentMarker fragment)
        {
            if (fragment.Role == MemoryFragmentRole.KeyMemory && !fragment.IsProtected)
            {
                fragment.Protect();
                SetStatus("스텔라의 결계를 핵심 기억에 고정했습니다. 이제 F키로 기억을 정화할 수 있습니다.");
                return;
            }

            PurifyFragment(fragment);
        }

        private void HandleMemoryEchoInteraction(MemoryEchoMarker echo)
        {
            GameBootstrapper.Instance.Session.Data.currentMemoryEchoHeard = true;
            SetStatus(
                $"{runtime.CurrentChapter.guest.displayName}: “두려움만 남은 장면 뒤에도 내가 선택한 순간이 있어.”\n" +
                "별자리의 목소리를 들었습니다. 마지막 기억을 받아들일 수 있습니다.");
            Destroy(echo.gameObject);
            GameBootstrapper.Instance.SaveNow();
        }

        private void Refresh()
        {
            if (objectiveText == null || runtime.MemorySpace == null)
                return;

            var remaining = Object.FindObjectsByType<MemoryFragmentMarker>(FindObjectsSortMode.None)
                .Where(item => !item.IsFalseMemory)
                .Select(item => item.ObjectiveId).ToHashSet();
            objectiveText.text = string.Join("\n", runtime.MemorySpace.Definition.objectiveIds.Select(id =>
                $"{(remaining.Contains(id) ? "□" : "✓")} {BuiltInChapterCatalog.GetLabel(id)}"));

            if (runtime.MemorySpace.IsComplete)
                SetStatus("핵심 기억이 안정되었습니다. Enter 또는 Space로 결과 화면으로 이동하세요.");
        }

        private void SetStatus(string message)
        {
            if (status != null)
                status.text = message;
        }

        /// <summary>
        /// 감정 장애물에 닿았을 때 최근 체크포인트로 복귀하고 재시도 횟수를 안내합니다.
        /// </summary>
        public void HandleObstacleHit()
        {
            AccessibleCameraEffectsPresenter.PlayImpact();
            if (runtime.RestoreMemoryCheckpoint(out var checkpoint))
                player.position = checkpoint.Position;
            else
                player.position = Vector3.zero;
            SetStatus($"결계가 흔들렸습니다. 최근 기억에서 다시 시작합니다. · 재시도 {runtime.MemorySpace.RetryCount}회");
            Refresh();
            GameBootstrapper.Instance.SaveNow();
        }

        /// <summary>
        /// 스텔라의 결계가 완전히 무너지면 최근 체크포인트에서 정화 상태를 보존한 채 다시 시작합니다.
        /// </summary>
        public void HandleBarrierBreak()
        {
            HandleObstacleHit();
            SetStatus($"스텔라의 결계가 무너져 최근 기억에서 다시 시작합니다. · 재시도 {runtime.MemorySpace.RetryCount}회");
        }

        private static string ToKorean(string id) => BuiltInChapterCatalog.GetLabel(id);
    }
}
