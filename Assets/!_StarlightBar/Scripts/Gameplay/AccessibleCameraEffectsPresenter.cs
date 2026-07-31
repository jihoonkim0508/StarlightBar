using System.Collections;
using System.Reflection;
using StarlightBar.Core;
using StarlightBar.UI;
using UnityEngine;
using UnityEngine.UI;

namespace StarlightBar.Gameplay
{
    /// <summary>
    /// 화면 흔들림·점멸·색수차 설정을 실제 카메라와 URP 볼륨에 연결합니다.
    /// </summary>
    public sealed class AccessibleCameraEffectsPresenter : MonoBehaviour
    {
        [SerializeField, Tooltip("씬에 배치한 점멸 오버레이 View입니다.")]
        private AccessibilityEffectsView view;

        private static AccessibleCameraEffectsPresenter instance;
        private Image flashOverlay;
        private Coroutine shakeRoutine;
        private Coroutine flashRoutine;
        private float nextVolumeRefresh;

        private void Awake()
        {
            instance = this;
            flashOverlay = view != null ? view.FlashOverlay : null;
            if (flashOverlay == null)
                Debug.LogError("AccessibilityEffectsView의 점멸 오버레이를 연결해야 합니다.", this);
        }

        private void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }

        private void Update()
        {
            if (Time.unscaledTime < nextVolumeRefresh)
                return;
            nextVolumeRefresh = Time.unscaledTime + 0.75f;
            ApplyChromaticAberration();
        }

        /// <summary>
        /// 접근성 설정을 존중하며 짧은 카메라 흔들림과 위험 점멸을 요청합니다.
        /// </summary>
        public static void PlayImpact(float strength = 0.12f, float duration = 0.22f)
        {
            if (instance == null || GameBootstrapper.Instance == null)
                return;
            var settings = GameBootstrapper.Instance.Session.Data.settings;
            if (!settings.reduceScreenShake)
            {
                if (instance.shakeRoutine != null)
                    instance.StopCoroutine(instance.shakeRoutine);
                instance.shakeRoutine = instance.StartCoroutine(instance.Shake(strength, duration));
            }
            if (!settings.reduceFlashing)
            {
                if (instance.flashRoutine != null)
                    instance.StopCoroutine(instance.flashRoutine);
                instance.flashRoutine = instance.StartCoroutine(instance.Flash());
            }
        }

        private IEnumerator Shake(float strength, float duration)
        {
            var camera = Camera.main;
            if (camera == null)
                yield break;
            var origin = camera.transform.localPosition;
            var end = Time.unscaledTime + duration;
            while (Time.unscaledTime < end)
            {
                camera.transform.localPosition = origin + (Vector3)(Random.insideUnitCircle * strength);
                yield return null;
            }
            camera.transform.localPosition = origin;
            shakeRoutine = null;
        }

        private IEnumerator Flash()
        {
            if (flashOverlay == null)
                yield break;
            var color = flashOverlay.color;
            for (var time = 0f; time < 0.18f; time += Time.unscaledDeltaTime)
            {
                color.a = Mathf.Sin(time / 0.18f * Mathf.PI) * 0.24f;
                flashOverlay.color = color;
                yield return null;
            }
            color.a = 0f;
            flashOverlay.color = color;
            flashRoutine = null;
        }

        private static void ApplyChromaticAberration()
        {
            var enabled = GameBootstrapper.Instance?.Session?.Data?.settings?.chromaticAberration == true;
            // URP 패키지 형식을 런타임 어셈블리에 직접 결합하지 않고 현재 로드된 볼륨 컴포넌트를 갱신합니다.
            foreach (var item in Resources.FindObjectsOfTypeAll<ScriptableObject>())
            {
                if (item == null || item.GetType().FullName !=
                    "UnityEngine.Rendering.Universal.ChromaticAberration")
                    continue;
                var property = item.GetType().GetProperty(
                    "active", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                property?.SetValue(item, enabled);
            }
        }
    }
}
