using StarlightBar.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace StarlightBar.UI
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject cantContinuePanel;

        private void Start()
        {
            settingsPanel?.SetActive(false);
            cantContinuePanel?.SetActive(false);
            EventSystem.current?.SetSelectedGameObject(null);

            var gameManager = GameManager.Instance;
            if (newGameButton != null)
                newGameButton.interactable = gameManager != null;
            if (continueButton != null)
                continueButton.interactable = gameManager != null;
        }

        public void StartNewGame()
        {
            GameManager.Instance?.StartNewGame();
        }

        public void ContinueGame()
        {
            if (GameManager.Instance?.ContinueGame() == false)
            {
                if (cantContinuePanel != null)
                    cantContinuePanel.SetActive(true);
                else
                    Debug.LogError("MainMenu의 'cant continue' 팝업을 찾을 수 없습니다.", this);
            }
        }

        public void ToggleSettings()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(!settingsPanel.activeSelf);
        }

        public void CloseSettings()
        {
            settingsPanel?.SetActive(false);
        }

        public void CloseCantContinue()
        {
            cantContinuePanel?.SetActive(false);
        }

        public void Quit()
        {
            GameManager.Instance?.Quit();
        }

    }
}
