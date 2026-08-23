using System;

namespace StarlightBar.Core
{
    // 인벤토리/퀘스트/재화 등 게임 상태가 바뀔 때마다 알려주는 전역 이벤트.
    // 매니저들이 상태를 바꾼 뒤 호출하면, 열려있는 UI 패널들이 자동으로 새로고침됨.
    public static class GameEvents
    {
        // 상태 변경을 구독하는 이벤트 (예: 패널이 자기 Refresh()를 등록)
        public static event Action OnStateChanged;

        // 상태가 바뀌었다고 구독자 전원에게 알림
        public static void RaiseStateChanged()
        {
            OnStateChanged?.Invoke();
        }
    }
}
