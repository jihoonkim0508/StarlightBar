using StarlightBar.Core;

namespace StarlightBar.Systems
{
    /// <summary>
    /// 내부 수치를 한국어 단계형 손님 상태로 변환합니다.
    /// </summary>
    public sealed class GuestStateModel
    {
        public int TrustValue { get; private set; }
        public int StabilityValue { get; private set; }
        public int MemoryValue { get; private set; }

        public GuestTrustStage TrustStage => TrustValue switch
        {
            < 20 => GuestTrustStage.Guarded,
            < 40 => GuestTrustStage.Low,
            < 60 => GuestTrustStage.Normal,
            < 80 => GuestTrustStage.High,
            _ => GuestTrustStage.Trusting
        };

        public GuestStabilityStage StabilityStage => StabilityValue switch
        {
            < 25 => GuestStabilityStage.Distressed,
            < 50 => GuestStabilityStage.Tense,
            < 75 => GuestStabilityStage.Normal,
            _ => GuestStabilityStage.Calm
        };

        public GuestMemoryStage MemoryStage => MemoryValue switch
        {
            <= 0 => GuestMemoryStage.None,
            < 25 => GuestMemoryStage.Faint,
            < 50 => GuestMemoryStage.Reacting,
            < 100 => GuestMemoryStage.Active,
            _ => GuestMemoryStage.Restored
        };

        /// <summary>
        /// 신뢰·안정·기억 변화량을 0~100 범위의 내부 상태에 적용합니다.
        /// </summary>
        public void Apply(int trustDelta, int stabilityDelta, int memoryDelta)
        {
            TrustValue = Clamp(TrustValue + trustDelta);
            StabilityValue = Clamp(StabilityValue + stabilityDelta);
            MemoryValue = Clamp(MemoryValue + memoryDelta);
        }

        /// <summary>
        /// 저장된 내부 상태를 불러오되 유효 범위로 제한합니다.
        /// </summary>
        public void Restore(int trust, int stability, int memory)
        {
            TrustValue = Clamp(trust);
            StabilityValue = Clamp(stability);
            MemoryValue = Clamp(memory);
        }

        /// <summary>
        /// 신뢰 단계를 기획서의 한국어 표시명으로 변환합니다.
        /// </summary>
        public static string ToKorean(GuestTrustStage stage) => stage switch
        {
            GuestTrustStage.Guarded => "경계함",
            GuestTrustStage.Low => "낮음",
            GuestTrustStage.Normal => "보통",
            GuestTrustStage.High => "높음",
            GuestTrustStage.Trusting => "신뢰함",
            _ => "-"
        };

        /// <summary>
        /// 안정 단계를 기획서의 한국어 표시명으로 변환합니다.
        /// </summary>
        public static string ToKorean(GuestStabilityStage stage) => stage switch
        {
            GuestStabilityStage.Distressed => "불안정",
            GuestStabilityStage.Tense => "긴장",
            GuestStabilityStage.Normal => "보통",
            GuestStabilityStage.Calm => "평온",
            _ => "-"
        };

        /// <summary>
        /// 기억 단계를 기획서의 한국어 표시명으로 변환합니다.
        /// </summary>
        public static string ToKorean(GuestMemoryStage stage) => stage switch
        {
            GuestMemoryStage.None => "없음",
            GuestMemoryStage.Faint => "희미함",
            GuestMemoryStage.Reacting => "반응",
            GuestMemoryStage.Active => "활성",
            GuestMemoryStage.Restored => "복원됨",
            _ => "-"
        };

        private static int Clamp(int value) => value < 0 ? 0 : value > 100 ? 100 : value;
    }
}
