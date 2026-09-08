using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarlightBar.UI
{
    // 설정 창: 효과음/배경음 슬라이더, 진동 토글, 언어 드롭다운을 관리.
    // InventoryPanelUI와 같은 스타일 — 프리팹에 미리 배치된 UI를 참조만 받아서
    // 제어하고, 배경색·모서리·토글 모양 같은 비주얼은 전부 프리팹(Inspector)에서 처리.
    // 사운드 매니저(AudioManager/SoundManager)가 아직 프로젝트에 없어서, 값이 바뀌면
    // 이벤트로만 느슨하게 알림 — 나중에 매니저가 생기면 이 이벤트들을 구독하면 됨.
    public sealed class SettingsPanelUI : MonoBehaviour
    {
        // 설정값을 간단히 영구 저장하기 위한 PlayerPrefs 키
        // (아직 SaveData 쪽에 설정 관련 필드가 없어서, 여기서만 자체적으로 관리)
        private const string EffectVolumeKey = "Settings_EffectVolume";
        private const string BackgroundVolumeKey = "Settings_BackgroundVolume";
        private const string VibrationKey = "Settings_VibrationEnabled";
        private const string LanguageIndexKey = "Settings_LanguageIndex";

        private const float DefaultVolume = 1f;
        private const bool DefaultVibrationEnabled = true;
        private const int DefaultLanguageIndex = 0;

        // 언어 드롭다운 선택지. PretendardVariable SDF에 한글/영문 글리프만 들어있어서
        // 일본어/중국어를 넣으면 글리프가 없어 네모(tofu)로 깨짐 — 폰트에 CJK 폴백을
        // 추가하기 전까지는 지원 언어만 등록. (Awake마다 이 배열로 드롭다운을 다시
        // 채우므로, 에디터에서 드롭다운 옵션을 직접 지워도 Play 시 여기 값으로 되돌아감)
        private static readonly string[] LanguageOptions = { "한국어", "English" };

        // 진동 토글 배경색: 켜짐=초록, 꺼짐=회색 (체크마크 없이 배경색만으로 상태 표시)
        private static readonly Color VibrationOnColor = new Color(0.3f, 0.75f, 0.4f, 1f);
        private static readonly Color VibrationOffColor = new Color(0.5f, 0.5f, 0.55f, 1f);

        [SerializeField] private Button closeButton;
        [SerializeField] private Slider effectVolumeSlider;
        [SerializeField] private Slider backgroundVolumeSlider;
        [SerializeField] private Toggle vibrationToggle;
        [SerializeField] private TMP_Dropdown languageDropdown;

        // 효과음 슬라이더 값이 바뀔 때 발생 (사운드 매니저가 구독해서 실제 볼륨에 반영하면 됨)
        public event Action<float> OnEffectVolumeChanged;

        // 배경음 슬라이더 값이 바뀔 때 발생
        public event Action<float> OnBackgroundVolumeChanged;

        // 진동 설정이 바뀔 때 발생
        public event Action<bool> OnVibrationChanged;

        // 언어 설정이 바뀔 때 발생 (선택된 언어 이름을 그대로 전달)
        public event Action<string> OnLanguageChanged;

        public float EffectVolume => effectVolumeSlider != null ? effectVolumeSlider.value : DefaultVolume;
        public float BackgroundVolume => backgroundVolumeSlider != null ? backgroundVolumeSlider.value : DefaultVolume;
        public bool IsVibrationEnabled => vibrationToggle != null && vibrationToggle.isOn;

        public string SelectedLanguage =>
            languageDropdown != null && languageDropdown.value < LanguageOptions.Length
                ? LanguageOptions[languageDropdown.value]
                : LanguageOptions[DefaultLanguageIndex];

        // 리스너 연결 + 저장된 설정값을 불러와서 초기 상태로 반영
        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Close);

            if (effectVolumeSlider != null)
                effectVolumeSlider.onValueChanged.AddListener(HandleEffectVolumeChanged);

            if (backgroundVolumeSlider != null)
                backgroundVolumeSlider.onValueChanged.AddListener(HandleBackgroundVolumeChanged);

            if (vibrationToggle != null)
                vibrationToggle.onValueChanged.AddListener(HandleVibrationChanged);

            if (languageDropdown != null)
            {
                SetupLanguageOptions();
                languageDropdown.onValueChanged.AddListener(HandleLanguageIndexChanged);
            }

            LoadSavedValues();
        }

        // 닫기(X) 버튼 클릭 시 설정 창 닫기
        public void Close()
        {
            gameObject.SetActive(false);
        }

        // 저장된 설정값(PlayerPrefs)을 읽어서 슬라이더/토글/드롭다운 초기값으로 반영.
        // SetValueWithoutNotify를 써서 초기화 중에는 OnXChanged 이벤트가 안 터지게 함.
        private void LoadSavedValues()
        {
            if (effectVolumeSlider != null)
                effectVolumeSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(EffectVolumeKey, DefaultVolume));

            if (backgroundVolumeSlider != null)
                backgroundVolumeSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat(BackgroundVolumeKey, DefaultVolume));

            if (vibrationToggle != null)
            {
                var vibrationEnabled = PlayerPrefs.GetInt(VibrationKey, DefaultVibrationEnabled ? 1 : 0) == 1;
                vibrationToggle.SetIsOnWithoutNotify(vibrationEnabled);
                UpdateVibrationToggleColor(vibrationEnabled);
            }

            if (languageDropdown != null)
            {
                // 언어 목록이 줄어든 뒤에도(예: 일본어/중국어 제거) 예전에 저장된
                // 인덱스가 범위를 벗어나지 않도록 클램프.
                var savedIndex = PlayerPrefs.GetInt(LanguageIndexKey, DefaultLanguageIndex);
                var safeIndex = Mathf.Clamp(savedIndex, 0, LanguageOptions.Length - 1);
                languageDropdown.SetValueWithoutNotify(safeIndex);
            }
        }

        // 드롭다운 옵션 목록을 언어 목록으로 채움
        private void SetupLanguageOptions()
        {
            languageDropdown.ClearOptions();
            languageDropdown.AddOptions(new List<string>(LanguageOptions));
        }

        // 효과음 슬라이더 값 변경 처리: 저장 + 외부(사운드 매니저 등) 알림
        private void HandleEffectVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat(EffectVolumeKey, value);
            OnEffectVolumeChanged?.Invoke(value);
        }

        // 배경음 슬라이더 값 변경 처리: 저장 + 외부 알림
        private void HandleBackgroundVolumeChanged(float value)
        {
            PlayerPrefs.SetFloat(BackgroundVolumeKey, value);
            OnBackgroundVolumeChanged?.Invoke(value);
        }

        // 진동 토글 변경 처리: 저장 + 배경색 갱신 + 외부 알림
        private void HandleVibrationChanged(bool isOn)
        {
            PlayerPrefs.SetInt(VibrationKey, isOn ? 1 : 0);
            UpdateVibrationToggleColor(isOn);
            OnVibrationChanged?.Invoke(isOn);
        }

        // 진동 토글 배경(targetGraphic) 색을 켜짐/꺼짐 상태에 맞게 변경
        private void UpdateVibrationToggleColor(bool isOn)
        {
            if (vibrationToggle != null && vibrationToggle.targetGraphic != null)
                vibrationToggle.targetGraphic.color = isOn ? VibrationOnColor : VibrationOffColor;
        }

        // 언어 드롭다운 변경 처리: 저장 + 외부 알림 (인덱스 대신 언어 이름 문자열로 전달)
        private void HandleLanguageIndexChanged(int index)
        {
            PlayerPrefs.SetInt(LanguageIndexKey, index);
            var language = index >= 0 && index < LanguageOptions.Length
                ? LanguageOptions[index]
                : LanguageOptions[DefaultLanguageIndex];
            OnLanguageChanged?.Invoke(language);
        }
    }
}
