namespace StarlightBar.Core
{
    /// <summary>
    /// 별빛주점의 선형 메인 진행 단계를 나타냅니다.
    /// </summary>
    public enum GamePhaseType
    {
        MainMenu,
        Prologue,
        MorningBriefing,
        DayExploration,
        TavernPreparation,
        NightService,
        Deduction,
        MemorySpace,
        ChapterResult,
        MidpointEvent,
        LateGameEvent,
        Ending
    }

    /// <summary>
    /// 별자리 손님의 복원 결과입니다.
    /// </summary>
    public enum RestorationGrade
    {
        Unstable,
        Partial,
        Complete
    }

    /// <summary>
    /// 복원된 손님이 선택할 수 있는 미래입니다.
    /// </summary>
    public enum GuestFutureChoice
    {
        ReturnToSky,
        RemainHumanWithMemories,
        RemainHumanWithoutCelestialIdentity
    }

    /// <summary>
    /// 손님 상태를 플레이어에게 표시할 때 사용하는 단계입니다.
    /// </summary>
    public enum GuestTrustStage { Guarded, Low, Normal, High, Trusting }

    /// <summary>
    /// 손님의 정서적 안정 상태를 한국어 단계명으로 표시하기 위한 값입니다.
    /// </summary>
    public enum GuestStabilityStage { Distressed, Tense, Normal, Calm }

    /// <summary>
    /// 손님의 기억 활성화 정도를 한국어 단계명으로 표시하기 위한 값입니다.
    /// </summary>
    public enum GuestMemoryStage { None, Faint, Reacting, Active, Restored }
}
