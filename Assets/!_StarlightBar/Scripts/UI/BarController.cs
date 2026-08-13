using StarlightBar.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

namespace StarlightBar.UI
{
    public sealed class BarController : MonoBehaviour
    {
        [SerializeField] private Button storyButton;
        [SerializeField] private TMP_Text storyButtonText;
        [SerializeField] private Button dialogueButton;
        [SerializeField] private CanvasGroup barUiGroup;
        [SerializeField] private DialogueRunner dialogueRunner;

        private void Start()
        {
            if (dialogueRunner != null)
                dialogueRunner.onDialogueComplete.AddListener(UnlockBarUi);
            if (dialogueButton != null)
                dialogueButton.interactable = dialogueRunner != null;

            var gameManager = GameManager.Instance;
            if (gameManager == null || !gameManager.HasSave)
            {
                SetStoryButton("새 게임에서 시작하세요", false);
                return;
            }

            switch (gameManager.StoryProgress)
            {
                case StoryProgress.Storygame1:
                    SetStoryButton("Storygame1 시작", true);
                    break;
                case StoryProgress.Storygame2:
                    SetStoryButton("Storygame2 시작", true);
                    break;
                default:
                    SetStoryButton("스토리 완료", false);
                    break;
            }
        }

        private void OnDestroy()
        {
            if (dialogueRunner != null)
                dialogueRunner.onDialogueComplete.RemoveListener(UnlockBarUi);
        }

        public void PlayTestDialogue()
        {
            if (dialogueRunner == null || dialogueRunner.IsDialogueRunning)
                return;

            SetBarUiEnabled(false);
            _ = dialogueRunner.StartDialogue("Start");
        }

        public void EnterNextStory()
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null || !gameManager.HasSave)
                return;

            if (gameManager.StoryProgress == StoryProgress.Storygame1)
                gameManager.LoadScene("Storygame1");
            else if (gameManager.StoryProgress == StoryProgress.Storygame2)
                gameManager.LoadScene("Storygame2");
        }

        private void SetStoryButton(string label, bool interactable)
        {
            if (storyButtonText != null)
                storyButtonText.text = label;
            if (storyButton != null)
                storyButton.interactable = interactable;
        }

        private void UnlockBarUi()
        {
            SetBarUiEnabled(true);
        }

        private void SetBarUiEnabled(bool enabled)
        {
            if (barUiGroup == null)
                return;

            barUiGroup.interactable = enabled;
            barUiGroup.blocksRaycasts = enabled;
        }
    }
}
