using UnityEngine;

namespace StarlightBar.Gameplay
{
    /// <summary>
    /// 최종 아트가 없는 시스템 테스트 단계에서 사용할 단색 2D 스프라이트를 제공합니다.
    /// </summary>
    internal static class RuntimeWorldSprite
    {
        private static Sprite square;
        private static Sprite roundedPanel;

        public static Sprite Square
        {
            get
            {
                if (square != null)
                    return square;

                var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
                {
                    name = "RuntimeSquareTexture",
                    hideFlags = HideFlags.HideAndDontSave
                };
                texture.SetPixel(0, 0, Color.white);
                texture.Apply();
                square = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1);
                square.hideFlags = HideFlags.HideAndDontSave;
                return square;
            }
        }

        /// <summary>
        /// 패널과 버튼 크기에 맞춰 늘어나도 모서리 반경을 유지하는 9-slice 둥근 사각형입니다.
        /// </summary>
        public static Sprite RoundedPanel
        {
            get
            {
                if (roundedPanel != null)
                    return roundedPanel;

                const int size = 64;
                const float radius = 13f;
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
                {
                    name = "RuntimeRoundedPanelTexture",
                    hideFlags = HideFlags.HideAndDontSave,
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp
                };
                var pixels = new Color32[size * size];
                var center = (size - 1) * 0.5f;
                var straightHalf = center - radius;
                for (var y = 0; y < size; y++)
                for (var x = 0; x < size; x++)
                {
                    var dx = Mathf.Max(0f, Mathf.Abs(x - center) - straightHalf);
                    var dy = Mathf.Max(0f, Mathf.Abs(y - center) - straightHalf);
                    var distance = Mathf.Sqrt(dx * dx + dy * dy);
                    var alpha = Mathf.Clamp01(radius + 0.75f - distance);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
                texture.SetPixels32(pixels);
                texture.Apply(false, true);
                roundedPanel = Sprite.Create(
                    texture,
                    new Rect(0, 0, size, size),
                    new Vector2(0.5f, 0.5f),
                    100f,
                    0,
                    SpriteMeshType.FullRect,
                    new Vector4(16f, 16f, 16f, 16f));
                roundedPanel.name = "RuntimeRoundedPanel";
                roundedPanel.hideFlags = HideFlags.HideAndDontSave;
                return roundedPanel;
            }
        }
    }
}
