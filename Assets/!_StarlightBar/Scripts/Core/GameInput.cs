using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace StarlightBar.Core
{
    /// <summary>
    /// 플레이 중 재설정 가능한 키 동작을 구분합니다.
    /// </summary>
    public enum GameInputAction
    {
        MoveUp, MoveDown, MoveLeft, MoveRight,
        Inspect, Talk, Notebook, Objectives, Telescope, Menu
    }

    /// <summary>
    /// 저장 데이터의 키 설정을 Input System 키보드 상태에 연결합니다.
    /// </summary>
    public static class GameInput
    {
        /// <summary>
        /// 지정한 게임 동작의 현재 키가 눌린 상태인지 확인합니다.
        /// </summary>
        public static bool IsPressed(GameInputAction action)
        {
            var keyControl = ResolveControl(action);
            return keyControl != null && keyControl.isPressed;
        }

        /// <summary>
        /// 지정한 게임 동작의 키가 이번 프레임에 처음 눌렸는지 확인합니다.
        /// </summary>
        public static bool WasPressedThisFrame(GameInputAction action)
        {
            var keyControl = ResolveControl(action);
            return keyControl != null && keyControl.wasPressedThisFrame;
        }

        /// <summary>
        /// 저장된 키 설정에서 지정한 게임 동작의 키를 반환합니다.
        /// </summary>
        public static Key GetKey(GameInputAction action)
        {
            var bindings = CurrentBindings();
            return bindings.Get(action);
        }

        /// <summary>
        /// 지정한 게임 동작의 키를 변경합니다.
        /// </summary>
        public static void SetKey(GameInputAction action, Key key)
        {
            CurrentBindings().Set(action, key);
        }

        private static KeyControl ResolveControl(GameInputAction action)
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return null;
            var key = GetKey(action);
            return key == Key.None ? null : keyboard[key];
        }

        private static KeyBindingData CurrentBindings()
        {
            var settings = GameBootstrapper.Instance?.Session?.Data?.settings;
            if (settings == null)
                return KeyBindingData.CreateDefault();
            return settings.keyBindings ??= KeyBindingData.CreateDefault();
        }
    }
}
