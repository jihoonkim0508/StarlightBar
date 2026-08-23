using StarlightBar.Core;
using UnityEngine;
using Yarn.Unity;

namespace StarlightBar.UI
{
    // Yarn 대화 스크립트(.yarn)에서 인벤토리/퀘스트/재화를 조작·조회할 수 있게 해주는 창구.
    // 여기 있는 메서드 이름이 곧 대화문 안에서 쓰는 명령어/함수 이름(<<give_item>> 등).
    public sealed class QuestInventoryYarnCommands : MonoBehaviour
    {
        // <<give_item "아이템id" 수량>> — 아이템 지급
        [YarnCommand("give_item")]
        public static void GiveItem(string itemId, int quantity = 1)
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null || gameManager.Inventory == null)
                return;

            gameManager.Inventory.AddItem(itemId, quantity);
            gameManager.SaveGame();
        }

        // <<take_item "아이템id" 수량>> — 아이템 회수
        [YarnCommand("take_item")]
        public static void TakeItem(string itemId, int quantity = 1)
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null || gameManager.Inventory == null)
                return;

            gameManager.Inventory.RemoveItem(itemId, quantity);
            gameManager.SaveGame();
        }

        // <<start_quest "퀘스트id">> — 퀘스트 시작
        [YarnCommand("start_quest")]
        public static void StartQuest(string questId)
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null || gameManager.Quests == null)
                return;

            gameManager.Quests.StartQuest(questId);
            gameManager.SaveGame();
        }

        // <<complete_quest "퀘스트id">> — 퀘스트 완료 처리
        [YarnCommand("complete_quest")]
        public static void CompleteQuest(string questId)
        {
            var gameManager = GameManager.Instance;
            if (gameManager == null || gameManager.Quests == null)
                return;

            gameManager.Quests.CompleteQuest(questId);
            gameManager.SaveGame();
        }

        // <<add_currency 수치>> — 재화 지급
        [YarnCommand("add_currency")]
        public static void AddCurrency(int amount)
        {
            GameManager.Instance?.AddCurrency(amount);
        }

        // has_item("아이템id") — 아이템 보유 여부 (대화 조건문 <<if>>에서 사용)
        [YarnFunction("has_item")]
        public static bool HasItem(string itemId) =>
            GameManager.Instance?.Inventory?.HasItem(itemId) ?? false;

        // is_quest_complete("퀘스트id") — 퀘스트 완료 여부
        [YarnFunction("is_quest_complete")]
        public static bool IsQuestComplete(string questId) =>
            GameManager.Instance?.Quests?.IsComplete(questId) ?? false;

        // get_item_count("아이템id") — 보유 수량, 대사 안에 {get_item_count(...)}처럼 인라인 표시 가능
        [YarnFunction("get_item_count")]
        public static int GetItemCount(string itemId) =>
            GameManager.Instance?.Inventory?.GetQuantity(itemId) ?? 0;

        // get_currency() — 보유 재화, 대사 안에 {get_currency()}처럼 인라인 표시 가능
        [YarnFunction("get_currency")]
        public static int GetCurrency() => GameManager.Instance?.Currency ?? 0;
    }
}
