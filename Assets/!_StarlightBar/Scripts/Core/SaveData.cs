using System;
using System.Collections.Generic;
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

    // [JungHo 추가] 퀘스트 진행 상태
    public enum QuestState
    {
        NotStarted, // 시작 전
        InProgress, // 진행 중
        Complete    // 완료
    }

    // [JungHo 추가] 인벤토리 한 칸 (아이템 id + 보유 수량)
    [Serializable]
    public sealed class InventoryEntry
    {
        public string itemId;
        public int quantity;
    }

    // [JungHo 추가] 퀘스트 기록 한 개 (퀘스트 id + 현재 상태)
    [Serializable]
    public sealed class QuestEntry
    {
        public string questId;
        public QuestState state;
    }

    [Serializable]
    public sealed class SaveData
    {
        public int version = JsonSaveStore.CurrentVersion;
        public StoryProgress storyProgress = StoryProgress.Storygame1;

        // [JungHo 추가] 인벤토리/퀘스트/재화 저장 필드
        public List<InventoryEntry> inventory = new List<InventoryEntry>(); // 보유 아이템 목록
        public List<QuestEntry> quests = new List<QuestEntry>(); // 퀘스트 진행 목록
        public int currency; // 보유 재화
    }

    internal sealed class JsonSaveStore
    {
        internal const int CurrentVersion = 2; // [JungHo 수정] 인벤토리/퀘스트/재화 필드 추가로 1 → 2 상향

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
