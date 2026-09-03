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

    // [JungHo 추가] 인벤토리 한 칸 (아이템 id + 보유 수량).
    // 필드를 private로 캡슐화하고, 수량 변경은 ChangeQuantity()로만 가능하게 해서
    // InventoryManager를 거치지 않고 외부(UI 등)에서 직접 값을 바꾸는 걸 막음.
    [Serializable]
    public sealed class InventoryEntry
    {
        [SerializeField] private string itemId;
        [SerializeField] private int quantity;

        public string ItemId => itemId;
        public int Quantity => quantity;

        public InventoryEntry(string itemId, int quantity)
        {
            this.itemId = itemId;
            this.quantity = quantity;
        }

        // 수량 증감. delta가 음수면 차감. 내부 quantity는 오직 이 메서드로만 바뀜.
        public void ChangeQuantity(int delta) => quantity += delta;
    }

    [Serializable]
    public sealed class SaveData
    {
        public int version = JsonSaveStore.CurrentVersion;
        public StoryProgress storyProgress = StoryProgress.Storygame1;

        // [JungHo 추가] 인벤토리 저장 필드
        public List<InventoryEntry> inventory = new List<InventoryEntry>(); // 보유 아이템 목록
    }

    internal sealed class JsonSaveStore
    {
        internal const int CurrentVersion = 2; // [JungHo 수정] 인벤토리 필드 추가로 1 → 2 상향

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
