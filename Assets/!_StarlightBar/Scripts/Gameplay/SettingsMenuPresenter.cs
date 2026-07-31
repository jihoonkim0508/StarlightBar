using System;
using System.Collections.Generic;
using System.Linq;
using StarlightBar.Core;
using StarlightBar.UI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace StarlightBar.Gameplay
{
    /// <summary>
    /// 해상도, 접근성, 대화, 화면 효과와 음량 설정을 제공하고 즉시 저장·적용합니다.
    /// </summary>
    public sealed class SettingsMenuPresenter : MonoBehaviour
    {
        [SerializeField, Tooltip("씬에 배치된 설정·접근성 화면 참조입니다.")]
        private SettingsView view;
        private static readonly Vector2Int[] Resolutions =
        {
            new(1280, 720), new(1920, 1080), new(2560, 1440), new(3440, 1440)
        };

        private GameObject root;
        private RectTransform panel;
        private GameSettingsData settings;
        private Image brightnessOverlay;
        private bool isOpen;
        private float lastTextScale = 1f;
        private readonly Dictionary<TMP_Text, float> existingTextSizes = new();
        private readonly Dictionary<GameInputAction, TMP_Text> bindingLabels = new();
        private GameInputAction? waitingForBinding;
        private TMP_Text bindingStatus;

        public bool IsOpen => isOpen;
        public static bool AnyOpen { get; private set; }

        private void Start()
        {
            if (GameBootstrapper.Instance == null)
                return;
            if (view == null)
            {
                Debug.LogError("SettingsView 참조가 연결되지 않았습니다.", this);
                enabled = false;
                return;
            }
            settings = GameBootstrapper.Instance.Session.Data.settings ??= new GameSettingsData();
            root = view.Root;
            panel = view.Content;
            brightnessOverlay = view.BrightnessOverlay;
            PopulateMenu();
            root.SetActive(false);
            ApplyDisplaySettings();
            ApplyAll();
        }

        private void Update()
        {
            if (waitingForBinding.HasValue)
            {
                var pressed = Keyboard.current?.allKeys.FirstOrDefault(key => key.wasPressedThisFrame);
                if (pressed != null && pressed.keyCode != Key.None)
                {
                    GameInput.SetKey(waitingForBinding.Value, pressed.keyCode);
                    bindingLabels[waitingForBinding.Value].text =
                        $"{InputTitle(waitingForBinding.Value)}: {pressed.keyCode}";
                    bindingStatus.text = "키 설정을 저장했습니다.";
                    waitingForBinding = null;
                    SaveSettings();
                }
                return;
            }

            if (!GameInput.WasPressedThisFrame(GameInputAction.Menu))
                return;

            var telescope = UnityEngine.Object.FindFirstObjectByType<RuntimeTelescopePresenter>();
            if (!isOpen && telescope != null && telescope.IsOpen)
                return;
            if (!isOpen && PathfinderNotebookPresenter.AnyOpen)
                return;
            if (!isOpen && InvestigationDetailPresenter.AnyOpen)
                return;
            Toggle();
        }

        private void LateUpdate()
        {
            if (brightnessOverlay != null)
                brightnessOverlay.transform.SetAsLastSibling();
            if (isOpen && root != null)
                root.transform.SetAsLastSibling();
        }

        private void OnDestroy()
        {
            if (isOpen)
                Time.timeScale = 1f;
            AnyOpen = false;
        }

        /// <summary>
        /// 설정 화면을 열고 닫습니다. 열려 있는 동안 게임 시간을 일시 정지합니다.
        /// </summary>
        public void Toggle()
        {
            isOpen = !isOpen;
            AnyOpen = isOpen;
            root.SetActive(isOpen);
            Time.timeScale = isOpen ? 0f : 1f;
            if (!isOpen)
                GameBootstrapper.Instance?.SaveNow();
        }

        private void PopulateMenu()
        {
            for (var index = panel.childCount - 1; index >= 0; index--)
                Destroy(panel.GetChild(index).gameObject);
            DynamicContentFactory.CreateText(panel, "설정 · 접근성", 34);
            AddCycle("해상도", ResolutionLabel, CycleResolution);
            AddCycle("화면 모드", FullscreenLabel, CycleFullscreen);
            AddFloatCycle("밝기", () => settings.brightness, 0.1f, 0.6f, 1.4f, ApplyAll);
            AddFloatCycle("텍스트 크기", () => settings.textScale, 0.1f, 0.8f, 1.6f, ApplyAll);
            AddFloatCycle("UI 배율", () => settings.uiScale, 0.1f, 0.75f, 1.5f, ApplyAll);
            AddFloatCycle("대화 속도", () => settings.dialogueSpeed, 0.25f, 0.5f, 3f, null);
            AddFloatCycle("자동 진행 속도", () => settings.autoAdvanceSpeed, 0.25f, 0.5f, 3f, null);
            AddFloatCycle("대화창 불투명도", () => settings.dialogueOpacity, 0.1f, 0.4f, 1f, ApplyAll);
            AddFloatCycle("마우스 감도", () => settings.mouseSensitivity, 0.1f, 0.5f, 2f, null);
            AddToggle("대화 자동 진행", () => settings.autoAdvance, value => settings.autoAdvance = value);
            AddToggle("읽은 문장 건너뛰기", () => settings.skipReadText, value => settings.skipReadText = value);
            AddToggle("전체 건너뛰기 허용", () => settings.allowFullSkip, value => settings.allowFullSkip = value);
            AddToggle("화면 흔들림 감소", () => settings.reduceScreenShake, value => settings.reduceScreenShake = value);
            AddToggle("점멸 감소", () => settings.reduceFlashing, value => settings.reduceFlashing = value);
            AddToggle("색수차", () => settings.chromaticAberration, value => settings.chromaticAberration = value);
            AddFloatCycle("마스터 음량", () => settings.masterVolume, 0.1f, 0f, 1f, ApplyAll);
            AddFloatCycle("음악 음량", () => settings.musicVolume, 0.1f, 0f, 1f, null);
            AddFloatCycle("효과음 음량", () => settings.effectsVolume, 0.1f, 0f, 1f, null);
            AddFloatCycle("음성 음량", () => settings.voiceVolume, 0.1f, 0f, 1f, null);
            AddFloatCycle("환경음 음량", () => settings.ambientVolume, 0.1f, 0f, 1f, null);
            DynamicContentFactory.CreateText(panel, "키 재설정", 26);
            bindingStatus = DynamicContentFactory.CreateText(
                panel, "바꿀 동작을 누른 뒤 새 키를 입력하세요.", 18);
            foreach (GameInputAction action in Enum.GetValues(typeof(GameInputAction)))
                AddKeyBinding(action);
            DynamicContentFactory.CreateButton(panel, "키 설정 기본값 복원", ResetBindings);
            DynamicContentFactory.CreateButton(panel, "현재 진행 수동 저장", () =>
            {
                GameBootstrapper.Instance.SaveNow();
                if (bindingStatus != null)
                    bindingStatus.text = "현재 진행을 수동 저장 슬롯에 기록했습니다.";
            });
            DynamicContentFactory.CreateButton(panel, "설정 저장 후 닫기", Toggle);
        }

        private void AddCycle(string title, Func<string> value, Action cycle)
        {
            Button button = null;
            button = DynamicContentFactory.CreateButton(panel, $"{title}: {value()}", () =>
            {
                cycle();
                button.GetComponentInChildren<TMP_Text>().text = $"{title}: {value()}";
                SaveSettings();
            });
        }

        private void AddFloatCycle(
            string title,
            Func<float> getter,
            float step,
            float minimum,
            float maximum,
            Action afterChange)
        {
            Button button = null;
            button = DynamicContentFactory.CreateButton(panel, $"{title}: {getter():0.00}", () =>
            {
                var next = getter() + step;
                if (next > maximum + 0.001f)
                    next = minimum;
                SetFloatSetting(title, Mathf.Round(next * 100f) / 100f);
                afterChange?.Invoke();
                button.GetComponentInChildren<TMP_Text>().text = $"{title}: {getter():0.00}";
                SaveSettings();
            });
        }

        private void AddToggle(string title, Func<bool> getter, Action<bool> setter)
        {
            Button button = null;
            button = DynamicContentFactory.CreateButton(panel, $"{title}: {OnOff(getter())}", () =>
            {
                setter(!getter());
                button.GetComponentInChildren<TMP_Text>().text = $"{title}: {OnOff(getter())}";
                SaveSettings();
            });
        }

        private void SetFloatSetting(string title, float value)
        {
            switch (title)
            {
                case "밝기": settings.brightness = value; break;
                case "텍스트 크기": settings.textScale = value; break;
                case "UI 배율": settings.uiScale = value; break;
                case "대화 속도": settings.dialogueSpeed = value; break;
                case "자동 진행 속도": settings.autoAdvanceSpeed = value; break;
                case "대화창 불투명도": settings.dialogueOpacity = value; break;
                case "마우스 감도": settings.mouseSensitivity = value; break;
                case "마스터 음량": settings.masterVolume = value; break;
                case "음악 음량": settings.musicVolume = value; break;
                case "효과음 음량": settings.effectsVolume = value; break;
                case "음성 음량": settings.voiceVolume = value; break;
                case "환경음 음량": settings.ambientVolume = value; break;
            }
        }

        private void CycleResolution()
        {
            var index = Array.FindIndex(Resolutions,
                item => item.x == settings.resolutionWidth && item.y == settings.resolutionHeight);
            var next = Resolutions[(index + 1 + Resolutions.Length) % Resolutions.Length];
            settings.resolutionWidth = next.x;
            settings.resolutionHeight = next.y;
            Screen.SetResolution(next.x, next.y, (FullScreenMode)settings.fullscreenMode);
        }

        private void CycleFullscreen()
        {
            settings.fullscreenMode = settings.fullscreenMode switch
            {
                (int)FullScreenMode.Windowed => (int)FullScreenMode.FullScreenWindow,
                (int)FullScreenMode.FullScreenWindow => (int)FullScreenMode.ExclusiveFullScreen,
                _ => (int)FullScreenMode.Windowed
            };
            Screen.fullScreenMode = (FullScreenMode)settings.fullscreenMode;
        }

        private void ApplyDisplaySettings()
        {
            var width = Mathf.Max(640, settings.resolutionWidth);
            var height = Mathf.Max(360, settings.resolutionHeight);
            var mode = (FullScreenMode)settings.fullscreenMode;
            if (Screen.width != width || Screen.height != height || Screen.fullScreenMode != mode)
                Screen.SetResolution(width, height, mode);
        }

        private string ResolutionLabel() => $"{settings.resolutionWidth}×{settings.resolutionHeight}";

        private string FullscreenLabel() => (FullScreenMode)settings.fullscreenMode switch
        {
            FullScreenMode.Windowed => "창 모드",
            FullScreenMode.ExclusiveFullScreen => "독점 전체 화면",
            _ => "테두리 없는 전체 화면"
        };

        private void ApplyAll()
        {
            AudioListener.volume = Mathf.Clamp01(settings.masterVolume);
            if (brightnessOverlay != null)
            {
                var brightness = Mathf.Clamp(settings.brightness, 0.6f, 1.4f);
                brightnessOverlay.color = brightness <= 1f
                    ? new Color(0, 0, 0, (1f - brightness) * 0.55f)
                    : new Color(1f, 0.96f, 0.88f, (brightness - 1f) * 0.20f);
            }

            foreach (var scaler in UnityEngine.Object.FindObjectsByType<CanvasScaler>(FindObjectsSortMode.None))
            {
                var scale = Mathf.Clamp(settings.uiScale, 0.75f, 1.5f);
                if (scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
                    scaler.referenceResolution = new Vector2(1920f / scale, 1080f / scale);
                else
                    scaler.scaleFactor = scale;
            }
            foreach (var scalable in UnityEngine.Object.FindObjectsByType<RuntimeTextScale>(FindObjectsSortMode.None))
                scalable.Apply(settings.textScale);
            foreach (var text in UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None))
            {
                if (text.GetComponent<RuntimeTextScale>() != null)
                    continue;
                if (!existingTextSizes.TryGetValue(text, out var baseSize))
                {
                    baseSize = text.fontSize / Mathf.Max(0.01f, lastTextScale);
                    existingTextSizes[text] = baseSize;
                }
                text.fontSize = Mathf.Clamp(baseSize * settings.textScale, 12f, 96f);
            }
            lastTextScale = settings.textScale;
        }

        private void AddKeyBinding(GameInputAction action)
        {
            var button = DynamicContentFactory.CreateButton(
                panel,
                $"{InputTitle(action)}: {GameInput.GetKey(action)}",
                () =>
                {
                    waitingForBinding = action;
                    bindingStatus.text = $"‘{InputTitle(action)}’에 사용할 새 키를 누르세요.";
                });
            bindingLabels[action] = button.GetComponentInChildren<TMP_Text>();
        }

        private void ResetBindings()
        {
            settings.keyBindings = KeyBindingData.CreateDefault();
            foreach (var pair in bindingLabels)
                pair.Value.text = $"{InputTitle(pair.Key)}: {GameInput.GetKey(pair.Key)}";
            bindingStatus.text = "기본 키 설정으로 복원했습니다.";
            SaveSettings();
        }

        private static string InputTitle(GameInputAction action) => action switch
        {
            GameInputAction.MoveUp => "위로 이동",
            GameInputAction.MoveDown => "아래로 이동",
            GameInputAction.MoveLeft => "왼쪽 이동",
            GameInputAction.MoveRight => "오른쪽 이동",
            GameInputAction.Inspect => "조사·정화",
            GameInputAction.Talk => "대화",
            GameInputAction.Notebook => "패스파인더 노트",
            GameInputAction.Objectives => "목표 목록",
            GameInputAction.Telescope => "망원경",
            _ => "메뉴"
        };

        private void SaveSettings() => GameBootstrapper.Instance?.SaveNow();
        private static string OnOff(bool value) => value ? "켜짐" : "꺼짐";
    }

}
