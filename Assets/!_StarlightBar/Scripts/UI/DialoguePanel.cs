using System.Collections.Generic;
using StarlightBar.Content;
using StarlightBar.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarlightBar.UI
{
    /// <summary>
    /// 한국어 대사, 인물 이름, 선택지 버튼을 표시하는 uGUI 패널입니다.
    /// </summary>
    public sealed class DialoguePanel : MonoBehaviour
    {
        [SerializeField, Tooltip("현재 대사의 화자 이름을 표시할 텍스트입니다.")]
        private TMP_Text speakerText;
        [SerializeField, Tooltip("현재 한국어 대사를 표시할 본문 텍스트입니다.")]
        private TMP_Text dialogueText;
        [SerializeField, Tooltip("화자의 표정 또는 스탠딩 초상을 표시할 이미지입니다.")]
        private Image portraitImage;
        [SerializeField, Tooltip("현재 대사의 선택지 버튼을 배치할 부모 오브젝트입니다.")]
        private Transform choiceRoot;
        [SerializeField, Tooltip("대화 선택지를 생성할 때 복제하는 버튼 프리팹입니다.")]
        private Button choiceButtonPrefab;

        private readonly List<Button> spawnedChoices = new();
        private IDialogueRunner runner;

        /// <summary>
        /// 대화 재생기의 이벤트를 UI에 연결합니다.
        /// </summary>
        public void Bind(IDialogueRunner dialogueRunner)
        {
            Unbind();
            runner = dialogueRunner;
            if (runner == null) return;
            runner.LineChanged += ShowLine;
            runner.DialogueCompleted += Hide;
        }

        /// <summary>
        /// 대사 재생기에 다음 문장 진행을 요청합니다.
        /// </summary>
        public void Advance()
        {
            runner?.Advance();
        }

        private void OnDestroy() => Unbind();

        private void Unbind()
        {
            if (runner == null) return;
            runner.LineChanged -= ShowLine;
            runner.DialogueCompleted -= Hide;
            runner = null;
        }

        private void ShowLine(DialogueLine line)
        {
            gameObject.SetActive(true);
            if (speakerText != null) speakerText.text = line.speakerId;
            if (dialogueText != null) dialogueText.text = line.text;
            ClearChoices();

            if (choiceRoot == null || choiceButtonPrefab == null) return;
            for (var index = 0; index < line.choices.Count; index++)
            {
                var capturedIndex = index;
                var button = Instantiate(choiceButtonPrefab, choiceRoot);
                var label = button.GetComponentInChildren<TMP_Text>();
                if (label != null) label.text = line.choices[index].text;
                button.onClick.AddListener(() => runner?.SelectChoice(capturedIndex));
                spawnedChoices.Add(button);
            }
        }

        private void Hide()
        {
            ClearChoices();
            gameObject.SetActive(false);
        }

        private void ClearChoices()
        {
            foreach (var button in spawnedChoices)
                if (button != null) Destroy(button.gameObject);
            spawnedChoices.Clear();
        }
    }
}
