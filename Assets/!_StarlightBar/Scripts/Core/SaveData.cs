using System;
using System.IO;
using UnityEngine;

namespace StarlightBar.Core
{
    public enum StoryProgress
    {
        Storygame1,
        Storygame2,
        Complete
    }

    [Serializable]
    public sealed class SaveData
    {
        public int version = JsonSaveStore.CurrentVersion;
        public StoryProgress storyProgress = StoryProgress.Storygame1;
    }

    internal sealed class JsonSaveStore
    {
        internal const int CurrentVersion = 1;

        private readonly string path = Path.Combine(Application.persistentDataPath, "save.json");

        internal bool TryLoad(out SaveData data)
        {
            data = null;

            try
            {
                if (!File.Exists(path))
                    return false;

                data = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
                if (data == null || data.version != CurrentVersion ||
                    !Enum.IsDefined(typeof(StoryProgress), data.storyProgress))
                {
                    data = null;
                    return false;
                }

                return true;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"저장 파일을 읽을 수 없습니다: {exception.Message}");
                data = null;
                return false;
            }
        }

        internal bool Save(SaveData data)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, JsonUtility.ToJson(data, true));
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"저장 파일을 쓸 수 없습니다: {exception.Message}");
                return false;
            }
        }
    }
}
