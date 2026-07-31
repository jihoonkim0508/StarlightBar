using StarlightBar.Core;
using StarlightBar.Content;
using StarlightBar.UI;
using TMPro;
using UnityEngine;

namespace StarlightBar.Gameplay
{
    /// <summary>
    /// 메인 메뉴에 플레이어 이름 입력과 현재 게임 안내를 추가합니다.
    /// </summary>
    public sealed class MainMenuEnhancer : MonoBehaviour
    {
        [SerializeField, Tooltip("MainMenu 씬에 배치된 편집 가능한 UI 참조입니다.")]
        private MainMenuView view;
        [SerializeField, Tooltip("메뉴 버튼 동작을 전역 게임 서비스로 전달하는 컨트롤러입니다.")]
        private MainMenuController controller;
        [SerializeField, Tooltip("MainMenu 씬에 배치된 설정 화면 컨트롤러입니다.")]
        private SettingsMenuPresenter settingsPresenter;

        private void Start()
        {
            if (view == null || controller == null)
            {
                Debug.LogError("MainMenuView 또는 MainMenuController 참조가 연결되지 않았습니다.", this);
                return;
            }

            controller.BindPlayerNameInput(view.PlayerNameInput);
            Bind(view.ContinueButton, controller.ContinueGame);
            Bind(view.NewGameButton, controller.StartNewGame);
            Bind(view.ArchiveButton, OpenArchive);
            Bind(view.SettingsButton, () => settingsPresenter?.Toggle());
            Bind(view.ExitButton, controller.ExitGame);
            Bind(view.ArchiveCloseButton, CloseArchive);
            view.ContinueButton.interactable =
                GameBootstrapper.Instance != null && GameBootstrapper.Instance.HasContinueData;
            CloseArchive();
        }

        private static void Bind(
            UnityEngine.UI.Button button,
            UnityEngine.Events.UnityAction action)
        {
            if (button == null)
                return;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
            button.onClick.AddListener(RuntimeAudioService.PlayUiConfirm);
        }

        private void OpenArchive()
        {
            if (view.ArchiveRoot == null || view.ArchiveContent == null ||
                view.ArchiveEntryPrefab == null)
                return;

            foreach (Transform child in view.ArchiveContent)
                Destroy(child.gameObject);

            var progress = GameBootstrapper.Instance.Session.Data.guestProgress;
            view.ArchiveEmptyMessage?.SetActive(progress.Count == 0);
            foreach (var item in progress)
            {
                var entry = Instantiate(view.ArchiveEntryPrefab, view.ArchiveContent);
                entry.GetComponent<TMP_Text>().text =
                    $"{BuiltInChapterCatalog.GetLabel(item.characterId)}\n" +
                    $"복원: {ToKorean(item.restorationGrade)} · 미래: {ToKorean(item.futureChoice)}";
            }
            view.ArchiveRoot.SetActive(true);
        }

        private void CloseArchive()
        {
            view?.ArchiveRoot?.SetActive(false);
        }

        private static string ToKorean(RestorationGrade grade) => grade switch
        {
            RestorationGrade.Complete => "완전 복원",
            RestorationGrade.Partial => "부분 복원",
            _ => "불안정 복원"
        };

        private static string ToKorean(GuestFutureChoice choice) => choice switch
        {
            GuestFutureChoice.ReturnToSky => "하늘로 복귀",
            GuestFutureChoice.RemainHumanWithMemories => "기억을 지닌 인간",
            _ => "천상의 정체성을 놓은 인간"
        };
    }
}
