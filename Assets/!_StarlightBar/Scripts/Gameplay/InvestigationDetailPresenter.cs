using System;
using StarlightBar.Content;
using StarlightBar.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarlightBar.Gameplay
{
    /// <summary>
    /// 조사 대상을 즉시 소비하지 않고 설명·소요 시간·획득 종류를 확인한 뒤 조사하도록 합니다.
    /// </summary>
    public sealed class InvestigationDetailPresenter : MonoBehaviour
    {
        [SerializeField, Tooltip("씬에 배치된 조사 상세 화면 참조입니다.")]
        private InvestigationView view;
        private Action confirmAction;

        public static bool AnyOpen { get; private set; }

        private void Start()
        {
            if (view == null)
                return;
            Bind(view.AddMemoButton, () =>
                view.NotebookNotice.text = "✦ 대상의 위치와 첫인상을 패스파인더 노트에 임시 기록했습니다.");
            Bind(view.CompareButton, () =>
                view.NotebookNotice.text = "✦ 기존 증거와의 공통 별빛 파장을 대조했습니다.");
            Bind(view.ConfirmButton, Confirm);
            Bind(view.CloseButton, Close);
            view.Root.SetActive(false);
        }

        private void Update()
        {
            if (AnyOpen && UnityEngine.InputSystem.Keyboard.current?.escapeKey.wasPressedThisFrame == true)
                Close();
        }

        private void OnDisable()
        {
            AnyOpen = false;
        }

        /// <summary>
        /// 목표 상세 정보를 표시하고 사용자가 확인하면 전달된 조사 동작을 실행합니다.
        /// </summary>
        public void Show(ObjectiveDefinition definition, Action onConfirm)
        {
            if (definition == null)
                return;
            if (view == null)
                return;

            view.Title.text = definition.title;
            view.Body.text = string.IsNullOrWhiteSpace(definition.description)
                ? "별빛의 흔적을 자세히 살펴봅니다."
                : definition.description;
            view.Metadata.text =
                $"{(definition.mandatory ? "필수 목표" : "선택 목표")} · {TypeLabel(definition.type)}\n" +
                $"예상 소요 시간 {definition.timeCostMinutes}분";
            view.ObjectImage.color = view.ColorFor(definition.type);
            view.NotebookNotice.text = "패스파인더 노트에 아직 기록되지 않았습니다.";
            confirmAction = onConfirm;
            view.ConfirmButton.GetComponentInChildren<TMP_Text>().text =
                definition.type is ObjectiveType.HumanLifeTrace or ObjectiveType.SpecialDialogue
                    ? "대화 기록하기"
                    : "조사 진행하기";
            view.Root.SetActive(true);
            view.Root.transform.SetAsLastSibling();
            AnyOpen = true;
        }

        /// <summary>
        /// 상세 화면을 닫고 대기 중인 조사 동작을 취소합니다.
        /// </summary>
        public void Close()
        {
            confirmAction = null;
            view?.Root?.SetActive(false);
            AnyOpen = false;
        }

        private void Confirm()
        {
            var action = confirmAction;
            Close();
            action?.Invoke();
        }

        private static void Bind(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
                return;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        private static string TypeLabel(ObjectiveType type) => type switch
        {
            ObjectiveType.RequiredIngredient => "재료 채집",
            ObjectiveType.Furniture => "가구 발견",
            ObjectiveType.HumanLifeTrace => "인물 대화",
            ObjectiveType.MythEvidence => "신화 흔적",
            ObjectiveType.AntagonistEvidence => "오염 흔적",
            ObjectiveType.SpecialDialogue => "특별 대화",
            _ => "현장 조사"
        };

    }
}
