using StarlightBar.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarlightBar.UI
{
    /// <summary>
    /// 메인 메뉴의 새 게임, 이어하기, 종료 동작을 게임 전역 서비스에 전달합니다.
    /// </summary>
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField, Tooltip("새 게임에 사용할 주인공 이름 입력창입니다.")]
        private TMP_InputField playerNameInput;
        [SerializeField, Tooltip("저장 데이터 유무에 따라 활성화할 이어하기 버튼입니다.")]
        private Button continueButton;

        private void Start()
        {
            if (continueButton != null)
                continueButton.interactable = GameBootstrapper.Instance != null &&
                                              GameBootstrapper.Instance.HasContinueData;
        }

        /// <summary>
        /// 입력한 한국어 이름으로 새 게임을 시작합니다.
        /// </summary>
        public void StartNewGame()
        {
            var playerName = playerNameInput == null ? "별지기" : playerNameInput.text;
            GameBootstrapper.Instance?.StartNewGame(playerName);
        }

        /// <summary>
        /// 정상 저장이나 복구 가능한 백업이 있으면 이어하기를 시작합니다.
        /// </summary>
        public void ContinueGame()
        {
            GameBootstrapper.Instance?.ContinueGame();
        }

        /// <summary>
        /// 빌드에서는 게임을 종료하고 에디터에서는 플레이 모드를 끝냅니다.
        /// </summary>
        public void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>
        /// 런타임에 생성된 한국어 이름 입력창을 새 게임 동작에 연결합니다.
        /// </summary>
        public void BindPlayerNameInput(TMP_InputField inputField)
        {
            playerNameInput = inputField;
        }
    }
}
