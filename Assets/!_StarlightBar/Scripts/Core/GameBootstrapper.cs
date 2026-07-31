using System.IO;
using StarlightBar.Systems;
using UnityEngine;

namespace StarlightBar.Core
{
    /// <summary>
    /// 게임 전역 세션과 저장 서비스를 만들고 씬 전환 동안 유지합니다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GameBootstrapper : MonoBehaviour
    {
        [SerializeField, Tooltip("저장 데이터가 없을 때 프롤로그부터 새 게임을 시작합니다.")]
        private bool loadExistingSave = true;

        public static GameBootstrapper Instance { get; private set; }
        public GameSession Session { get; private set; }
        public ISaveService SaveService { get; private set; }
        public GameFlowController Flow { get; private set; }
        public GameRuntimeCoordinator Runtime { get; private set; }
        public bool HasContinueData => pendingSave != null;

        private GameSaveData pendingSave;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Session = new GameSession();
            SaveService = new JsonSaveService(Path.Combine(Application.persistentDataPath, "Saves"));
            if (loadExistingSave && SaveService.TryLoad(out var saveData))
            {
                pendingSave = saveData;
                // 메인 메뉴에서는 진행 상태를 시작하지 않지만 사용자 설정은 즉시 적용해야 한다.
                Session.RestoreSettings(saveData.settings);
            }

            Flow = GetComponent<GameFlowController>();
            if (Flow == null)
            {
                Debug.LogError("Bootstrap 씬의 GameSystems에 GameFlowController가 필요합니다.", this);
                enabled = false;
                return;
            }
            Flow.Initialize(Session);
            Runtime = new GameRuntimeCoordinator(Session, Flow, SaveNow);
            Flow.PhaseChanged += OnPhaseChanged;
        }

        /// <summary>
        /// 현재 세션을 자동 저장 슬롯에 기록합니다.
        /// </summary>
        public void SaveNow()
        {
            SaveService.Save(Session.Data);
            pendingSave = Session.Data.Clone();
        }

        /// <summary>
        /// 새 게임을 만들고 프롤로그로 이동합니다.
        /// </summary>
        public void StartNewGame(string playerName)
        {
            pendingSave = null;
            Session.Reset(playerName);
            Runtime.ReloadCurrentChapter();
            Flow.ForcePhase(GamePhaseType.Prologue);
            SaveNow();
        }

        /// <summary>
        /// 마지막 정상 저장을 세션에 적용하고 저장된 단계로 이동합니다.
        /// </summary>
        public bool ContinueGame()
        {
            if (pendingSave == null)
                return false;

            var resumePhase = pendingSave.currentPhase;
            Session.Restore(pendingSave);
            Runtime = new GameRuntimeCoordinator(Session, Flow, SaveNow);
            Flow.ForcePhase(resumePhase);
            return true;
        }

        private void OnDestroy()
        {
            if (Flow != null)
                Flow.PhaseChanged -= OnPhaseChanged;
            if (Instance == this)
                Instance = null;
        }

        private void OnPhaseChanged(GamePhaseType previous, GamePhaseType next)
        {
            // 이야기의 중요한 경계에서만 기록해 잦은 디스크 쓰기를 피한다.
            if (next is GamePhaseType.MorningBriefing or
                GamePhaseType.TavernPreparation or
                GamePhaseType.Deduction or
                GamePhaseType.MemorySpace or
                GamePhaseType.ChapterResult or
                GamePhaseType.MidpointEvent or
                GamePhaseType.LateGameEvent or
                GamePhaseType.Ending or
                GamePhaseType.MainMenu)
            {
                SaveNow();
            }
        }
    }
}
