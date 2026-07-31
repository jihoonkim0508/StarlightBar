using System;
using System.Collections.Generic;
using StarlightBar.Content;
using StarlightBar.Core;
using UnityEngine;

namespace StarlightBar.Systems
{
    /// <summary>
    /// ScriptableObject 대사를 순차 재생하고 선택지 분기를 처리합니다.
    /// </summary>
    public sealed class SimpleDialogueRunner : MonoBehaviour, IDialogueRunner
    {
        private readonly List<DialogueLine> history = new();
        private DialogueDefinition dialogue;
        private DialogueLine currentLine;

        public bool IsPlaying { get; private set; }
        public IReadOnlyList<DialogueLine> History => history;
        public event Action<DialogueLine> LineChanged;
        public event Action DialogueCompleted;

        /// <summary>
        /// 대화 정의의 진입 노드부터 한국어 대사를 재생합니다.
        /// </summary>
        public void Play(DialogueDefinition definition)
        {
            if (definition == null) throw new ArgumentNullException(nameof(definition));

            dialogue = definition;
            history.Clear();
            IsPlaying = true;
            ShowLine(dialogue.FindLine(dialogue.entryLineId));
        }

        /// <summary>
        /// 현재 대사의 선택지를 선택하고 연결된 다음 노드로 이동합니다.
        /// </summary>
        public void SelectChoice(int choiceIndex)
        {
            if (!IsPlaying || currentLine?.choices == null ||
                choiceIndex < 0 || choiceIndex >= currentLine.choices.Count)
                return;

            ShowLine(dialogue.FindLine(currentLine.choices[choiceIndex].nextLineId));
        }

        /// <summary>
        /// 선택지가 없는 현재 대사에서 다음 노드로 진행합니다.
        /// </summary>
        public void Advance()
        {
            if (!IsPlaying || currentLine == null || currentLine.choices.Count > 0)
                return;

            var currentIndex = dialogue.lines.IndexOf(currentLine);
            var next = currentIndex >= 0 && currentIndex + 1 < dialogue.lines.Count
                ? dialogue.lines[currentIndex + 1]
                : null;
            ShowLine(next);
        }

        /// <summary>
        /// 현재 대화를 종료하고 완료 이벤트를 보냅니다.
        /// </summary>
        public void Stop()
        {
            if (!IsPlaying) return;
            IsPlaying = false;
            currentLine = null;
            DialogueCompleted?.Invoke();
        }

        private void ShowLine(DialogueLine line)
        {
            if (line == null)
            {
                Stop();
                return;
            }

            currentLine = line;
            history.Add(line);
            LineChanged?.Invoke(line);
        }
    }
}
