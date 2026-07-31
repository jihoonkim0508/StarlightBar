using StarlightBar.Core;
using StarlightBar.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace StarlightBar.UI
{
    /// <summary>
    /// 게임 화면에서 현재 진행 단계와 한국어 조작 안내를 표시합니다.
    /// </summary>
    public sealed class PhaseHud : MonoBehaviour
    {
        [SerializeField, Tooltip("현재 게임 진행 단계를 한국어로 표시할 텍스트입니다.")]
        private TMP_Text phaseText;
        [SerializeField, Tooltip("현재 단계의 조작 또는 진행 불가 사유를 표시할 텍스트입니다.")]
        private TMP_Text hintText;

        private void Start()
        {
            var bootstrapper = GameBootstrapper.Instance;
            if (bootstrapper == null) return;
            bootstrapper.Flow.PhaseChanged += OnPhaseChanged;
            Refresh(bootstrapper.Flow.CurrentPhase);
        }

        private void OnDestroy()
        {
            if (GameBootstrapper.Instance != null)
                GameBootstrapper.Instance.Flow.PhaseChanged -= OnPhaseChanged;
        }

        private void OnPhaseChanged(GamePhaseType previous, GamePhaseType next) => Refresh(next);

        private void Update()
        {
            if (Keyboard.current == null || GameBootstrapper.Instance == null)
                return;
            if (RuntimeDialoguePresenter.AnyPlaying || SettingsMenuPresenter.AnyOpen ||
                PathfinderNotebookPresenter.AnyOpen || RuntimeTelescopePresenter.AnyOpen)
                return;

            if (Keyboard.current.enterKey.wasPressedThisFrame ||
                Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                if (!GameBootstrapper.Instance.Runtime.TryAdvance(out var reason) && hintText != null &&
                    !string.IsNullOrEmpty(reason))
                {
                    hintText.text = reason;
                }
            }
        }

        private void Refresh(GamePhaseType phase)
        {
            if (phaseText != null) phaseText.text = $"현재 단계: {ToKorean(phase)}";
            if (hintText != null)
            {
                hintText.text = phase == GamePhaseType.DayExploration
                    ? "WASD 이동 · Enter/Space 다음 단계 · F 조사 · E 대화 · J 노트 · 1 망원경"
                    : "Enter 또는 Space: 다음 단계";
            }
        }

        /// <summary>
        /// 게임 진행 단계를 HUD에 표시할 한국어 이름으로 변환합니다.
        /// </summary>
        public static string ToKorean(GamePhaseType phase) => phase switch
        {
            GamePhaseType.MainMenu => "메인 메뉴",
            GamePhaseType.Prologue => "프롤로그",
            GamePhaseType.MorningBriefing => "아침 브리핑",
            GamePhaseType.DayExploration => "낮 탐색",
            GamePhaseType.TavernPreparation => "주점 준비",
            GamePhaseType.NightService => "야간 접객",
            GamePhaseType.Deduction => "최종 추리",
            GamePhaseType.MemorySpace => "기억공간",
            GamePhaseType.ChapterResult => "챕터 결과",
            GamePhaseType.MidpointEvent => "중반 사건",
            GamePhaseType.LateGameEvent => "최종 사건",
            GamePhaseType.Ending => "엔딩",
            _ => phase.ToString()
        };
    }
}
