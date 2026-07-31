using System;

namespace StarlightBar.Core
{
    /// <summary>
    /// 낮 탐색 시간과 필수 목표 미완료 시의 안전 정지를 관리합니다.
    /// </summary>
    public sealed class GameClock
    {
        public const int DayStartMinute = 9 * 60;
        public const int DayEndMinute = 15 * 60;
        public const int PreparationEndMinute = 17 * 60;

        private readonly float gameMinutesPerRealSecond;
        private float fractionalMinutes;

        public event Action<int> MinuteChanged;
        public event Action MandatoryObjectiveGraceStarted;

        public int CurrentMinute { get; private set; }
        public bool IsPaused { get; private set; }
        public bool IsMandatoryGraceActive { get; private set; }

        /// <summary>
        /// 지정한 시작 시각과 시간 배율로 낮 시간 시계를 만듭니다.
        /// </summary>
        public GameClock(int startMinute = DayStartMinute, float gameSecondsPerRealSecond = 30f)
        {
            CurrentMinute = Math.Max(0, startMinute);
            gameMinutesPerRealSecond = Math.Max(0f, gameSecondsPerRealSecond) / 60f;
        }

        /// <summary>
        /// 현실 시간을 게임 시간으로 변환합니다. 필수 목표가 남은 15시에는 시계를 멈춥니다.
        /// </summary>
        public void Tick(float realDeltaSeconds, bool mandatoryObjectivesComplete)
        {
            if (IsPaused || IsMandatoryGraceActive || realDeltaSeconds <= 0f)
                return;

            fractionalMinutes += realDeltaSeconds * gameMinutesPerRealSecond;
            var wholeMinutes = (int)fractionalMinutes;
            if (wholeMinutes <= 0)
                return;

            fractionalMinutes -= wholeMinutes;
            AdvanceMinutes(wholeMinutes, mandatoryObjectivesComplete);
        }

        /// <summary>
        /// 대화와 조사에 지정된 고정 시간을 추가합니다.
        /// </summary>
        public void AdvanceMinutes(int minutes, bool mandatoryObjectivesComplete)
        {
            if (minutes <= 0 || IsMandatoryGraceActive)
                return;

            var target = CurrentMinute + minutes;
            if (CurrentMinute < DayEndMinute && target >= DayEndMinute && !mandatoryObjectivesComplete)
            {
                CurrentMinute = DayEndMinute;
                IsMandatoryGraceActive = true;
                MinuteChanged?.Invoke(CurrentMinute);
                MandatoryObjectiveGraceStarted?.Invoke();
                return;
            }

            CurrentMinute = target;
            MinuteChanged?.Invoke(CurrentMinute);
        }

        /// <summary>
        /// 설정 메뉴나 시스템 확인창이 열린 동안 시간 진행을 일시 정지하거나 재개합니다.
        /// </summary>
        public void SetPaused(bool paused) => IsPaused = paused;

        /// <summary>
        /// 필수 목표가 모두 완료되면 안전 정지를 해제합니다.
        /// </summary>
        public void ResolveMandatoryGrace()
        {
            IsMandatoryGraceActive = false;
        }
    }
}
