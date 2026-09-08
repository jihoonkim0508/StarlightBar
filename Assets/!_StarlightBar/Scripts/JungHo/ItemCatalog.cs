using System.Collections.Generic;

namespace StarlightBar.Core
{
    // 아이템 id와 화면에 보여줄 한글 이름을 연결하는 사전.
    // 인벤토리 UI(InventoryPanelUI)와 대화(InventoryYarnCommands) 양쪽이
    // 같은 이름을 쓰도록 여기 한 곳에서만 관리한다.
    public static class ItemCatalog
    {
        // itemId -> 표시 이름. 새 아이템이 생기면 여기에 등록.
        private static readonly Dictionary<string, string> DisplayNames = new Dictionary<string, string>
        {
            { "warm_milk", "따뜻한 우유" },
            { "star_honey_milk", "별꿀 우유" },
            { "star_lantern", "별등" },
        };

        // itemId에 해당하는 표시 이름 조회. 등록이 안 된 id면 itemId를 그대로 반환
        // (등록 누락을 감추지 않고 바로 눈에 띄게 하기 위함).
        public static string GetDisplayName(string itemId) =>
            DisplayNames.TryGetValue(itemId, out var name) ? name : itemId;
    }
}
