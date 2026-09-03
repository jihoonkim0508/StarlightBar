using UnityEngine;
using UnityEngine.SceneManagement;

namespace StarlightBar.Core
{
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public bool HasSave { get; private set; }
        public StoryProgress StoryProgress => saveData?.storyProgress ?? StoryProgress.Storygame1;

        // ===== [JungHo 추가] 인벤토리 접근용 프로퍼티 =====
        public InventoryManager Inventory { get; private set; } // 인벤토리 매니저

        private JsonSaveStore saveStore;
        private SaveData saveData;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            saveStore = new JsonSaveStore();
            HasSave = saveStore.TryLoad(out saveData);
            RebuildGameplayManagers(); // [JungHo 추가] 인벤토리 매니저 갱신
        }

        private void Start()
        {
            if (Instance == this && SceneManager.GetActiveScene().name == "Bootstrap")
                LoadScene("MainMenu");
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void StartNewGame()
        {
            saveData = new SaveData();
            HasSave = saveStore.Save(saveData);
            RebuildGameplayManagers(); // [JungHo 추가]
            if (HasSave)
                LoadScene("Bar");
        }

        public bool ContinueGame()
        {
            HasSave = saveStore.TryLoad(out saveData);
            RebuildGameplayManagers(); // [JungHo 추가]
            if (HasSave)
                LoadScene("Bar");

            return HasSave;
        }

        // ===== [JungHo 추가] 저장 관련 메서드 =====

        // 현재 saveData를 파일에 저장
        public bool SaveGame() => saveData != null && saveStore.Save(saveData);

        // saveData가 (재)할당될 때마다 인벤토리 매니저를 새로 만들어 연결
        private void RebuildGameplayManagers()
        {
            Inventory = saveData != null ? new InventoryManager(saveData) : null;
        }

        public bool CompleteStory(StoryProgress completedStory)
        {
            if ((!HasSave || saveData == null) && !saveStore.TryLoad(out saveData))
                return false;

            if (completedStory == StoryProgress.Complete ||
                completedStory != saveData.storyProgress)
                return false;

            saveData.storyProgress = completedStory switch
            {
                StoryProgress.Storygame1 => StoryProgress.Storygame2,
                StoryProgress.Storygame2 => StoryProgress.Complete,
                _ => StoryProgress.Complete
            };

            HasSave = saveStore.Save(saveData);
            return HasSave;
        }

        public void LoadScene(string sceneName)
        {
            if (!Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError($"Build Settings에 '{sceneName}' 씬이 없습니다.", this);
                return;
            }

            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
