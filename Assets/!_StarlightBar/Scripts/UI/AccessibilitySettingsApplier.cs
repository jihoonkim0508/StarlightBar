using StarlightBar.Core;
using TMPro;
using UnityEngine;

namespace StarlightBar.UI
{
    /// <summary>
    /// 저장된 텍스트 크기, UI 배율, 대화창 불투명도를 런타임 UI에 적용합니다.
    /// </summary>
    public sealed class AccessibilitySettingsApplier : MonoBehaviour
    {
        [SerializeField, Tooltip("사용자 UI 배율을 적용할 캔버스 스케일러 어댑터입니다.")]
        private CanvasScalerAdapter canvasScaler;
        [SerializeField, Tooltip("접근성 텍스트 크기 설정을 적용할 텍스트 목록입니다.")]
        private TMP_Text[] scalableTexts;
        [SerializeField, Tooltip("대화창 불투명도 설정을 적용할 캔버스 그룹입니다.")]
        private CanvasGroup dialogueGroup;

        /// <summary>
        /// 접근성 설정을 현재 씬의 UI 요소에 반영합니다.
        /// </summary>
        public void Apply(GameSettingsData settings)
        {
            if (settings == null) return;
            if (canvasScaler != null) canvasScaler.SetScale(settings.uiScale);
            foreach (var text in scalableTexts)
                if (text != null) text.fontSize *= settings.textScale;
            if (dialogueGroup != null) dialogueGroup.alpha = settings.dialogueOpacity;
        }
    }

    /// <summary>
    /// CanvasScaler의 배율 설정을 직렬화 가능한 컴포넌트로 감쌉니다.
    /// </summary>
    [RequireComponent(typeof(UnityEngine.UI.CanvasScaler))]
    public sealed class CanvasScalerAdapter : MonoBehaviour
    {
        private UnityEngine.UI.CanvasScaler scaler;

        /// <summary>
        /// 기준 해상도 배율을 유지하면서 사용자 UI 배율을 적용합니다.
        /// </summary>
        public void SetScale(float scale)
        {
            if (scaler == null) scaler = GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.scaleFactor = Mathf.Clamp(scale, 0.75f, 2f);
        }
    }
}
