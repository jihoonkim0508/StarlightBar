using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StarlightBar.Core
{
    /// <summary>
    /// 게임 단계 변경을 실제 Unity 씬 전환으로 변환합니다.
    /// </summary>
    [RequireComponent(typeof(GameFlowController))]
    public sealed class GameSceneRouter : MonoBehaviour
    {
        private GameFlowController flow;
        private bool loading;

        private void Awake()
        {
            flow = GetComponent<GameFlowController>();
        }

        private void OnEnable()
        {
            if (flow != null)
                flow.PhaseChanged += OnPhaseChanged;
        }

        private void OnDisable()
        {
            if (flow != null)
                flow.PhaseChanged -= OnPhaseChanged;
        }

        private void Start()
        {
            if (flow == null)
                return;

            var sceneName = ResolveSceneName(flow.CurrentPhase);
            if (!string.IsNullOrEmpty(sceneName) && SceneManager.GetActiveScene().name != sceneName)
                StartCoroutine(LoadScene(sceneName));
        }

        private void OnPhaseChanged(GamePhaseType previous, GamePhaseType next)
        {
            var sceneName = ResolveSceneName(next);
            if (!string.IsNullOrEmpty(sceneName) && SceneManager.GetActiveScene().name != sceneName && !loading)
                StartCoroutine(LoadScene(sceneName));
        }

        /// <summary>
        /// 게임 진행 단계를 대응하는 Unity 씬 이름으로 변환합니다.
        /// </summary>
        public static string ResolveSceneName(GamePhaseType phase)
        {
            return phase switch
            {
                GamePhaseType.MainMenu => "MainMenu",
                GamePhaseType.Prologue => "Prologue",
                GamePhaseType.MorningBriefing => "Tavern",
                GamePhaseType.DayExploration => "Hyehwa",
                GamePhaseType.TavernPreparation => "Tavern",
                GamePhaseType.NightService => "Tavern",
                GamePhaseType.Deduction => "Tavern",
                GamePhaseType.MemorySpace => "MemorySpace",
                GamePhaseType.ChapterResult => "Tavern",
                GamePhaseType.MidpointEvent => "Tavern",
                GamePhaseType.LateGameEvent => "Tavern",
                GamePhaseType.Ending => "Ending",
                _ => string.Empty
            };
        }

        private IEnumerator LoadScene(string sceneName)
        {
            loading = true;
            var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            while (operation != null && !operation.isDone)
                yield return null;
            loading = false;
        }
    }
}
