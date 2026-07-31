using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarlightBar.Core
{
    /// <summary>
    /// 씬 오브젝트 대신 중앙에서 게임 단계 전환과 챕터 분기점을 관리합니다.
    /// </summary>
    public sealed class GameFlowController : MonoBehaviour
    {
        private static readonly GamePhaseType[] ChapterSequence =
        {
            GamePhaseType.MorningBriefing,
            GamePhaseType.DayExploration,
            GamePhaseType.TavernPreparation,
            GamePhaseType.NightService,
            GamePhaseType.Deduction,
            GamePhaseType.MemorySpace,
            GamePhaseType.ChapterResult
        };

        private readonly Dictionary<GamePhaseType, IGamePhase> phases = new();
        private GameSession session;
        private IGamePhase activePhase;

        public event Action<GamePhaseType, GamePhaseType> PhaseChanged;
        public GamePhaseType CurrentPhase => session?.Data.currentPhase ?? GamePhaseType.Prologue;
        public GameSession Session => session;

        /// <summary>
        /// 새 세션 또는 불러온 세션으로 진행 컨트롤러를 초기화합니다.
        /// </summary>
        public void Initialize(GameSession gameSession)
        {
            session = gameSession ?? throw new ArgumentNullException(nameof(gameSession));
            foreach (GamePhaseType phaseType in Enum.GetValues(typeof(GamePhaseType)))
            {
                if (!phases.ContainsKey(phaseType))
                    phases.Add(phaseType, new StandardGamePhase(phaseType));
            }
            ActivatePhase(session.Data.currentPhase, true);
        }

        /// <summary>
        /// 사용자 정의 단계 구현을 등록합니다.
        /// </summary>
        public void RegisterPhase(IGamePhase phase)
        {
            if (phase == null) throw new ArgumentNullException(nameof(phase));
            phases[phase.PhaseType] = phase;
        }

        /// <summary>
        /// 현재 단계가 완료된 경우 다음 단계로 이동합니다.
        /// </summary>
        public bool TryAdvance()
        {
            if (session == null || (activePhase != null && !activePhase.CanAdvance))
                return false;

            var current = session.Data.currentPhase;
            var completedIndex = session.Data.currentChapterIndex;
            var next = ResolveNextPhase(current, completedIndex);

            if (current == GamePhaseType.ChapterResult)
            {
                if (!string.IsNullOrWhiteSpace(session.Data.currentChapterId) &&
                    !session.Data.completedChapterIds.Contains(session.Data.currentChapterId))
                {
                    session.Data.completedChapterIds.Add(session.Data.currentChapterId);
                }

                if (completedIndex < 11)
                    session.Data.currentChapterIndex = completedIndex + 1;
            }

            ActivatePhase(next, false);
            return true;
        }

        /// <summary>
        /// 복구나 에디터 디버깅을 위해 특정 단계로 안전하게 이동합니다.
        /// </summary>
        public void ForcePhase(GamePhaseType phase)
        {
            if (session == null) throw new InvalidOperationException("게임 세션이 초기화되지 않았습니다.");
            ActivatePhase(phase, false);
        }

        /// <summary>
        /// 현재 단계와 완료 챕터 수를 바탕으로 중반·후반 사건을 포함한 다음 단계를 계산합니다.
        /// </summary>
        public static GamePhaseType ResolveNextPhase(GamePhaseType current, int completedChapterIndex)
        {
            if (current == GamePhaseType.MainMenu)
                return GamePhaseType.Prologue;

            if (current == GamePhaseType.Prologue)
                return GamePhaseType.MorningBriefing;

            if (current == GamePhaseType.MidpointEvent)
                return GamePhaseType.MorningBriefing;

            if (current == GamePhaseType.LateGameEvent)
                return GamePhaseType.Ending;

            var index = Array.IndexOf(ChapterSequence, current);
            if (index < 0)
                return GamePhaseType.MainMenu;

            if (index < ChapterSequence.Length - 1)
                return ChapterSequence[index + 1];

            // ChapterResult를 빠져나오는 시점의 인덱스는 완료한 챕터를 가리킨다.
            if (completedChapterIndex >= 11)
                return GamePhaseType.LateGameEvent;
            if (completedChapterIndex == 5)
                return GamePhaseType.MidpointEvent;
            return GamePhaseType.MorningBriefing;
        }

        private void ActivatePhase(GamePhaseType next, bool restoring)
        {
            var previous = session.Data.currentPhase;
            activePhase?.Exit(session);

            session.Data.currentPhase = next;
            phases.TryGetValue(next, out activePhase);
            activePhase?.Enter(session);

            if (!restoring)
                PhaseChanged?.Invoke(previous, next);
        }
    }

    /// <summary>
    /// 공용 상태 수명 주기를 제공하며, 개별 단계가 필요할 때 같은 계약의 구현으로 교체할 수 있습니다.
    /// </summary>
    public sealed class StandardGamePhase : IGamePhase
    {
        public GamePhaseType PhaseType { get; }
        public bool CanAdvance { get; private set; } = true;

        /// <summary>
        /// 지정한 게임 단계에 사용할 기본 단계 구현을 만듭니다.
        /// </summary>
        public StandardGamePhase(GamePhaseType phaseType)
        {
            PhaseType = phaseType;
        }

        /// <summary>
        /// 단계 진입 시 기본적으로 전환 가능 상태를 복구합니다. 세부 조건은 런타임 코디네이터가 검사합니다.
        /// </summary>
        public void Enter(GameSession session)
        {
            CanAdvance = true;
        }

        /// <summary>
        /// 단계 이탈 시 추가 자원을 보유하지 않으므로 상태만 유지합니다.
        /// </summary>
        public void Exit(GameSession session)
        {
        }

        /// <summary>
        /// 컷신이나 외부 연출이 진행 중인 동안 중앙 상태 전환을 잠급니다.
        /// </summary>
        public void SetCanAdvance(bool value)
        {
            CanAdvance = value;
        }
    }
}
