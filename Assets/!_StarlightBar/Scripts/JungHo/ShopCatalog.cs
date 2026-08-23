using System.Collections.Generic;

namespace StarlightBar.Core
{
    // 상점에서 판매하는 아이템 한 종류 (아이템 id, 표시 이름, 가격)
    public readonly struct ShopOffer
    {
        public readonly string itemId;
        public readonly string displayName;
        public readonly int price;

        public ShopOffer(string itemId, string displayName, int price)
        {
            this.itemId = itemId;
            this.displayName = displayName;
            this.price = price;
        }
    }

    // 상점 판매 목록 (고정 데이터). 실제 챕터1 아이템으로 나중에 교체 예정
    public static class ShopCatalog
    {
        // 현재 판매 중인 아이템 전체 목록
        public static readonly IReadOnlyList<ShopOffer> Offers = new List<ShopOffer>
        {
            new ShopOffer("warm_milk", "따뜻한 우유", 3),
            new ShopOffer("star_cookie", "별모양 쿠키", 5),
            new ShopOffer("herb_tea", "허브차", 4),
        };
    }
}
