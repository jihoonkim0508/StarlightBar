using StarlightBar.Core;
using UnityEngine;

namespace StarlightBar.Gameplay
{
    /// <summary>
    /// 음악·효과·음성·환경음 설정을 실제 AudioSource 채널에 적용하는 런타임 오디오 서비스입니다.
    /// </summary>
    public sealed class RuntimeAudioService : MonoBehaviour
    {
        [Header("에디터에서 배치한 오디오 채널")]
        [SerializeField] private AudioSource music;
        [SerializeField] private AudioSource effects;
        [SerializeField] private AudioSource voice;
        [SerializeField] private AudioSource ambient;
        [Header("교체 가능한 오디오 클립")]
        [SerializeField] private AudioClip clickClip;
        [SerializeField] private AudioClip voiceClip;
        [SerializeField] private AudioClip musicClip;
        [SerializeField] private AudioClip ambientClip;

        private static RuntimeAudioService instance;

        private void Awake()
        {
            instance = this;
            if (music == null || effects == null || voice == null || ambient == null)
            {
                Debug.LogError("RuntimeAudioService의 네 오디오 채널을 Inspector에서 연결해야 합니다.", this);
                enabled = false;
                return;
            }
            music.clip = musicClip;
            music.loop = true;
            if (music.clip != null)
                music.Play();
            ambient.clip = ambientClip;
            ambient.loop = true;
            if (ambient.clip != null)
                ambient.Play();
        }

        private void Update()
        {
            var settings = GameBootstrapper.Instance?.Session?.Data?.settings;
            if (settings == null)
                return;
            AudioListener.volume = Mathf.Clamp01(settings.masterVolume);
            music.volume = Mathf.Clamp01(settings.musicVolume) * 0.08f;
            effects.volume = Mathf.Clamp01(settings.effectsVolume) * 0.35f;
            voice.volume = Mathf.Clamp01(settings.voiceVolume) * 0.28f;
            ambient.volume = Mathf.Clamp01(settings.ambientVolume) * 0.10f;
        }

        private void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        /// <summary>
        /// 현재 효과음 음량으로 짧은 UI 확인음을 재생합니다.
        /// </summary>
        public static void PlayUiConfirm()
        {
            if (instance != null && instance.clickClip != null)
                instance.effects.PlayOneShot(instance.clickClip);
        }

        /// <summary>
        /// 실제 음성 에셋을 넣기 전에도 음성 채널 설정을 확인할 수 있는 대사 시작음을 재생합니다.
        /// </summary>
        public static void PlayVoiceCue()
        {
            if (instance != null && instance.voiceClip != null)
                instance.voice.PlayOneShot(instance.voiceClip);
        }

    }
}
