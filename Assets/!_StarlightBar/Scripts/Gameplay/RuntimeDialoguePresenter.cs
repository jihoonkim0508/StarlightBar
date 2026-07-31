using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using StarlightBar.Content;
using StarlightBar.Core;
using StarlightBar.Systems;
using StarlightBar.UI;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace StarlightBar.Gameplay
{
    /// <summary>
    /// 한국어 타이핑, 선택지, 자동 진행, 로그, 읽은 문장 및 전체 건너뛰기를 제공하는 대화 화면입니다.
    /// </summary>
    public sealed class RuntimeDialoguePresenter : MonoBehaviour
    {
        [SerializeField, Tooltip("씬에 배치된 대화 및 로그 화면 참조입니다.")]
        private DialogueView view;
        [SerializeField, Tooltip("씬에 배치된 대화 데이터 실행기입니다.")]
        private SimpleDialogueRunner runner;
        private GameObject root;
        private RectTransform choiceRoot;
        private TMP_Text speakerText;
        private TMP_Text expressionText;
        private Image portraitImage;
        private TMP_Text bodyText;
        private TMP_Text autoLabel;
        private GameObject logRoot;
        private Coroutine typing;
        private Coroutine autoAdvance;
        private DialogueLine currentLine;
        private Action onCompleted;
        private bool lineFullyShown;
        private bool wasReadBefore;

        public static bool AnyPlaying { get; private set; }
        public bool IsPlaying => runner != null && runner.IsPlaying;

        private void Awake()
        {
            if (runner == null || view == null)
            {
                Debug.LogError("DialogueView 또는 SimpleDialogueRunner 참조가 없습니다.", this);
                enabled = false;
                return;
            }
            root = view.Root;
            choiceRoot = view.ChoiceRoot;
            speakerText = view.Speaker;
            expressionText = view.Expression;
            portraitImage = view.Portrait;
            bodyText = view.Body;
            autoLabel = view.AutoButtonLabel;
            logRoot = view.LogRoot;
            Bind(view.NextButton, AdvanceOrReveal);
            Bind(view.AutoButton, ToggleAuto);
            Bind(view.LogButton, ToggleLog);
            Bind(view.SkipButton, Skip);
            Bind(view.LogCloseButton, ToggleLog);
            root.SetActive(false);
            logRoot.SetActive(false);
            runner.LineChanged += ShowLine;
            runner.DialogueCompleted += Complete;
        }

        private void OnDestroy()
        {
            AnyPlaying = false;
            if (runner == null)
                return;
            runner.LineChanged -= ShowLine;
            runner.DialogueCompleted -= Complete;
        }

        private void Update()
        {
            if (!IsPlaying || logRoot != null && logRoot.activeSelf)
                return;
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return;
            if (keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)
                AdvanceOrReveal();
        }

        /// <summary>
        /// 대화 데이터를 처음부터 재생하고 종료 시 지정된 동작을 실행합니다.
        /// </summary>
        public void Play(DialogueDefinition dialogue, Action completed = null)
        {
            if (dialogue == null)
            {
                completed?.Invoke();
                return;
            }

            onCompleted = completed;
            root.SetActive(true);
            AnyPlaying = true;
            runner.Play(dialogue);
        }

        private static void Bind(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
                return;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        private void ShowLine(DialogueLine line)
        {
            RuntimeAudioService.PlayVoiceCue();
            currentLine = line;
            wasReadBefore = GameBootstrapper.Instance.Session.Data.readDialogueLineIds.Contains(line.id);
            ClearChoices();
            speakerText.text = KoreanTextFormatter.Format(line.speakerId, PlayerName());
            expressionText.text = string.IsNullOrWhiteSpace(line.expressionId)
                ? "표정 · 차분히 바라봄"
                : $"표정 · {BuiltInChapterCatalog.GetLabel(line.expressionId)}";
            portraitImage.color = ResolvePortraitColor(line.speakerId);
            var formatted = KoreanTextFormatter.Format(line.text, PlayerName());
            RecordPersistentHistory(line, formatted);
            if (typing != null)
                StopCoroutine(typing);
            typing = StartCoroutine(TypeLine(formatted));

            if (!string.IsNullOrWhiteSpace(line.evidenceId))
                GameBootstrapper.Instance.Runtime.CollectEvidence(line.evidenceId);
            if (line.choices != null && line.choices.Count > 0)
            {
                foreach (var choice in line.choices)
                {
                    var captured = choice;
                    var evidenceMark = string.IsNullOrWhiteSpace(choice.evidenceId) ? string.Empty : "  ✦";
                    DynamicContentFactory.CreateButton(
                        choiceRoot,
                        KoreanTextFormatter.Format(choice.text, PlayerName()) + evidenceMark,
                        () => SelectChoice(captured));
                }
            }
        }

        private IEnumerator TypeLine(string value)
        {
            lineFullyShown = false;
            bodyText.text = string.Empty;
            var speed = Mathf.Clamp(
                GameBootstrapper.Instance.Session.Data.settings.dialogueSpeed, 0.5f, 3f);
            foreach (var character in value)
            {
                bodyText.text += character;
                yield return new WaitForSecondsRealtime(0.025f / speed);
            }
            lineFullyShown = true;
            typing = null;
            MarkCurrentLineRead();
            ScheduleAutoAdvance();
        }

        private Color ResolvePortraitColor(string speakerId)
        {
            var stella = speakerId?.Contains("스텔라") == true ||
                         speakerId?.Contains("stella") == true;
            var guest = GameBootstrapper.Instance?.Runtime?.CurrentChapter?.guest;
            var isGuest = guest != null &&
                          (speakerId == guest.id || speakerId == guest.displayName);
            return view.PortraitColor(stella, isGuest, guest == null ? Color.white : guest.themeColor);
        }

        private void AdvanceOrReveal()
        {
            if (!IsPlaying || currentLine == null)
                return;
            if (!lineFullyShown)
            {
                if (typing != null)
                    StopCoroutine(typing);
                bodyText.text = KoreanTextFormatter.Format(currentLine.text, PlayerName());
                lineFullyShown = true;
                typing = null;
                MarkCurrentLineRead();
                ScheduleAutoAdvance();
                return;
            }
            if (currentLine.choices != null && currentLine.choices.Count > 0)
                return;
            runner.Advance();
        }

        private void SelectChoice(DialogueChoice choice)
        {
            if (!lineFullyShown)
                return;
            var index = currentLine.choices.IndexOf(choice);
            GameBootstrapper.Instance.Runtime.ApplyDialogueChoice(choice);
            runner.SelectChoice(index);
        }

        private void ToggleAuto()
        {
            var settings = GameBootstrapper.Instance.Session.Data.settings;
            settings.autoAdvance = !settings.autoAdvance;
            autoLabel.text = $"자동: {(settings.autoAdvance ? "켜짐" : "꺼짐")}";
            ScheduleAutoAdvance();
        }

        private void ScheduleAutoAdvance()
        {
            if (autoAdvance != null)
                StopCoroutine(autoAdvance);
            if (!GameBootstrapper.Instance.Session.Data.settings.autoAdvance ||
                currentLine?.choices?.Count > 0 || !lineFullyShown)
                return;
            autoAdvance = StartCoroutine(AutoAdvanceAfterDelay());
        }

        private IEnumerator AutoAdvanceAfterDelay()
        {
            var speed = Mathf.Clamp(
                GameBootstrapper.Instance.Session.Data.settings.autoAdvanceSpeed, 0.5f, 3f);
            yield return new WaitForSecondsRealtime(1.8f / speed);
            runner.Advance();
        }

        private void Skip()
        {
            var settings = GameBootstrapper.Instance.Session.Data.settings;
            if (settings.allowFullSkip || settings.skipReadText && wasReadBefore)
            {
                runner.Stop();
                GameBootstrapper.Instance.SaveNow();
            }
            else
                bodyText.text = "건너뛰기는 설정에서 ‘읽은 문장’ 또는 ‘전체’ 허용 후 사용할 수 있습니다.";
        }

        private void ToggleLog()
        {
            if (logRoot.activeSelf)
            {
                logRoot.SetActive(false);
                return;
            }

            for (var index = view.LogContent.childCount - 1; index >= 0; index--)
                Destroy(view.LogContent.GetChild(index).gameObject);
            DynamicContentFactory.CreateText(view.LogContent, "대화 로그", 32);
            var persistentHistory = GameBootstrapper.Instance.Session.Data.dialogueHistory;
            foreach (var entry in persistentHistory.Skip(Mathf.Max(0, persistentHistory.Count - 100)))
                DynamicContentFactory.CreateText(
                    view.LogContent, $"{entry.speaker}\n{entry.text}", 21);
            logRoot.SetActive(true);
        }

        private void Complete()
        {
            if (typing != null)
                StopCoroutine(typing);
            if (autoAdvance != null)
                StopCoroutine(autoAdvance);
            ClearChoices();
            if (root != null)
                root.SetActive(false);
            AnyPlaying = false;
            var callback = onCompleted;
            onCompleted = null;
            callback?.Invoke();
        }

        private void ClearChoices()
        {
            if (choiceRoot == null)
                return;
            for (var index = choiceRoot.childCount - 1; index >= 0; index--)
                Destroy(choiceRoot.GetChild(index).gameObject);
        }

        private string PlayerName() => GameBootstrapper.Instance.Session.Data.playerName;

        private void MarkCurrentLineRead()
        {
            if (currentLine == null)
                return;
            var read = GameBootstrapper.Instance.Session.Data.readDialogueLineIds;
            if (!read.Contains(currentLine.id))
                read.Add(currentLine.id);
            GameBootstrapper.Instance.SaveNow();
        }

        private void RecordPersistentHistory(DialogueLine line, string formattedText)
        {
            var data = GameBootstrapper.Instance.Session.Data;
            data.dialogueHistory ??= new List<DialogueHistoryEntry>();
            if (data.dialogueHistory.Any(entry => entry.lineId == line.id))
                return;
            data.dialogueHistory.Add(new DialogueHistoryEntry
            {
                chapterId = GameBootstrapper.Instance.Runtime.CurrentChapter?.id ?? string.Empty,
                lineId = line.id,
                speaker = KoreanTextFormatter.Format(line.speakerId, PlayerName()),
                text = formattedText
            });
        }
    }

    /// <summary>
    /// 플레이어 이름과 한국어 조사 토큰을 자연스러운 문장으로 치환합니다.
    /// </summary>
    public static class KoreanTextFormatter
    {
        /// <summary>
        /// 주인공 이름 치환과 받침에 맞는 한국어 조사를 대사에 적용합니다.
        /// </summary>
        public static string Format(string source, string playerName)
        {
            if (string.IsNullOrEmpty(source))
                return string.Empty;
            var name = string.IsNullOrWhiteSpace(playerName) ? "별지기" : playerName.Trim();
            return source
                .Replace("{player:은}", name + Particle(name, "은", "는"))
                .Replace("{player:이}", name + Particle(name, "이", "가"))
                .Replace("{player:을}", name + Particle(name, "을", "를"))
                .Replace("{player:과}", name + Particle(name, "과", "와"))
                .Replace("{player}", name);
        }

        private static string Particle(string word, string withBatchim, string withoutBatchim)
        {
            if (string.IsNullOrEmpty(word))
                return withoutBatchim;
            var last = word[^1];
            if (last < '가' || last > '힣')
                return withoutBatchim;
            return (last - '가') % 28 == 0 ? withoutBatchim : withBatchim;
        }
    }
}
