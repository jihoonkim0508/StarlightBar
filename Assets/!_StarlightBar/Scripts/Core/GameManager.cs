using UnityEngine;
using UnityEngine.SceneManagement;
using StarlightBar.Gameplay;
using StarlightBar.Gameplay.Items;

namespace StarlightBar.Core
{
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public bool HasSave { get; private set; }
        public StoryProgress StoryProgress => saveData?.storyProgress ?? StoryProgress.Storygame1;
        public Inventory Inventory { get; } = new();

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
            if (HasSave)
                RestoreInventory();
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
            var newSave = new SaveData();
            if (!saveStore.Save(newSave))
                return;

            saveData = newSave;
            HasSave = true;
            Inventory.Clear();
            LoadScene("Bar");
        }

        public bool ContinueGame()
        {
            if (!saveStore.TryLoad(out var loadedSave))
            {
                HasSave = false;
                return false;
            }

            saveData = loadedSave;
            HasSave = true;
            RestoreInventory();
            LoadScene("Bar");
            return true;
        }

        public bool SaveGame()
        {
            if (saveData == null)
                return false;

            var nextSave = CreateSave(saveData.storyProgress);
            if (!saveStore.Save(nextSave))
                return false;

            saveData = nextSave;
            HasSave = true;
            return true;
        }

        public bool CompleteStory(StoryProgress completedStory)
        {
            if ((!HasSave || saveData == null) && !saveStore.TryLoad(out saveData))
                return false;

            if (completedStory == StoryProgress.Complete ||
                completedStory != saveData.storyProgress)
                return false;

            var nextProgress = completedStory switch
            {
                StoryProgress.Storygame1 => StoryProgress.Storygame2,
                StoryProgress.Storygame2 => StoryProgress.Complete,
                _ => StoryProgress.Complete
            };
            var nextSave = CreateSave(nextProgress);

            if (!saveStore.Save(nextSave))
                return false;

            saveData = nextSave;
            HasSave = true;
            return true;
        }

        private SaveData CreateSave(StoryProgress progress)
        {
            var data = new SaveData { storyProgress = progress };
            foreach (var pair in Inventory.Items)
                data.inventory.Add(new InventorySaveEntry(pair.Key.Id, pair.Key.DisplayName, pair.Value));
            return data;
        }

        private void RestoreInventory()
        {
            Inventory.Clear();
            if (saveData?.inventory == null)
                return;

            foreach (var entry in saveData.inventory)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.itemId) || entry.quantity <= 0)
                    continue;

                Inventory.Add(ItemCatalog.FromSave(entry.itemId, entry.displayName), entry.quantity);
            }
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
