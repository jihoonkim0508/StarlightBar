using StarlightBar.Core;
using UnityEngine;
using UnityEngine.UI;

namespace StarlightBar.UI
{
    public sealed class StorySceneController : MonoBehaviour
    {
        [SerializeField] private StoryProgress story;
        [SerializeField] private Button completeButton;

        private void Start()
        {
            var gameManager = GameManager.Instance;
            if (completeButton != null)
                completeButton.interactable = gameManager != null && gameManager.HasSave &&
                                              gameManager.StoryProgress == story;
        }

        public void CompleteAndReturnToBar()
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null)
                return;

            if (gameManager.CompleteStory(story))
                gameManager.LoadScene("Bar");
        }
    }
}
