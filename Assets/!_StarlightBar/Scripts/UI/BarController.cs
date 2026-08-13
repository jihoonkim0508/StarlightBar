using StarlightBar.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarlightBar.UI
{
    public sealed class BarController : MonoBehaviour
    {
        [SerializeField] private Button storyButton;
        [SerializeField] private TMP_Text storyButtonText;

        private void Start()
        {
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
    }
}
