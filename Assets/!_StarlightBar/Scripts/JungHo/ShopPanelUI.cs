using StarlightBar.Core;
using TMPro;
using UnityEngine;

namespace StarlightBar.UI
{
    // 상점 패널: 토글 버튼 + 판매 목록 패널을 코드로 생성, 구매 버튼 클릭까지 처리
    public sealed class ShopPanelUI : MonoBehaviour
    {
        private UITheme theme;
        private GameObject panel;
        private RectTransform content;
        private TextMeshProUGUI currencyLabel;

        // 토글 버튼과 패널 틀 + 재화 표시 라벨을 생성 (아직 목록은 안 채움)
        private void Awake()
        {
            theme = GetComponent<UITheme>();

            var toggle = UIFactory.CreateButton("OpenShopButton", transform, "상점", theme.Font, UITheme.PanelColor, UITheme.TextPrimary, TogglePanel);
            UIFactory.AnchorTopLeftStack((RectTransform)toggle.transform, 2, new Vector2(168, 53), 24f, 92f, 12f);

            var chrome = UIFactory.CreatePanelChrome("ShopPanel", transform, "상점", theme.Font, new Vector2(480, 460));
            panel = chrome.panel;
            content = chrome.content;

            currencyLabel = UIFactory.CreateLabel("Currency", panel.transform, "★ 0", theme.Font, 22f, UITheme.AccentGold, TextAlignmentOptions.Right);
            UIFactory.AnchorTopRight(currencyLabel.rectTransform, marginX: 60f, marginY: 18f, size: new Vector2(100, 32));
        }

        // 상태 변경 이벤트 구독 + 최초 1회 화면 갱신
        private void Start()
        {
            GameEvents.OnStateChanged += Refresh;
            Refresh();
        }

        // 파괴될 때 이벤트 구독 해제 (메모리 누수 방지)
        private void OnDestroy()
        {
            GameEvents.OnStateChanged -= Refresh;
        }

        // 토글 버튼 클릭 시 패널 열기/닫기
        private void TogglePanel()
        {
            var next = !panel.activeSelf;
            panel.SetActive(next);
            if (next)
                Refresh();
        }

        // 재화·판매 목록을 다시 그림. 재화 부족한 상품은 구매 버튼 비활성화
        private void Refresh()
        {
            var gameManager = GameManager.Instance;
            currencyLabel.text = $"★ {gameManager?.Currency ?? 0}";

            for (var i = content.childCount - 1; i >= 0; i--)
                Destroy(content.GetChild(i).gameObject);

            foreach (var offer in ShopCatalog.Offers)
            {
                var row = UIFactory.CreateRow($"Row_{offer.itemId}", content, 60f);

                var nameLabel = UIFactory.CreateLabel("Name", row.transform, offer.displayName, theme.Font, 20f, UITheme.TextPrimary, TextAlignmentOptions.MidlineLeft);
                UIFactory.StretchWithMargins(nameLabel.rectTransform, 16f, 0f, 220f, 0f);

                var priceLabel = UIFactory.CreateLabel("Price", row.transform, $"★ {offer.price}", theme.Font, 18f, UITheme.AccentGold, TextAlignmentOptions.MidlineRight);
                UIFactory.StretchWithMargins(priceLabel.rectTransform, 0f, 0f, 108f, 0f);

                var currentOffer = offer;
                var buyButton = UIFactory.CreateButton("BuyButton", row.transform, "구매", theme.Font, UITheme.AccentCoral, UITheme.TextPrimary, () => Purchase(currentOffer.itemId));
                UIFactory.AnchorTopRight((RectTransform)buyButton.transform, marginX: 8f, marginY: 8f, size: new Vector2(88, 44));

                buyButton.interactable = (gameManager?.Currency ?? 0) >= offer.price;
            }
        }

        // 구매 버튼 클릭 시: 상점 매니저에게 구매 요청 후 화면 갱신
        private void Purchase(string itemId)
        {
            GameManager.Instance?.Shop?.TryPurchase(itemId, 1);
            Refresh();
        }
    }
}
