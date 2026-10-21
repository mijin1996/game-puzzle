using UnityEngine;

namespace Quadrop.Presentation
{
    /// <summary>블록 셀 스프라이트를 코드로 만든다 (외부 아트 파일 없이 바로 플레이 가능하게).</summary>
    public static class SpriteFactory
    {
        private const int TextureSize = 16;
        private const float EdgeShade = 0.72f;

        /// <summary>테두리가 살짝 어두운 흰 사각형. 1 셀 = 1 월드 유닛.</summary>
        public static Sprite CreateCellSprite()
        {
            Texture2D texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            Color edge = new Color(EdgeShade, EdgeShade, EdgeShade, 1f);
            Color[] pixels = new Color[TextureSize * TextureSize];
            for (int y = 0; y < TextureSize; y++)
            {
                for (int x = 0; x < TextureSize; x++)
                {
                    bool isEdge = x == 0 || y == 0 || x == TextureSize - 1 || y == TextureSize - 1;
                    pixels[y * TextureSize + x] = isEdge ? edge : Color.white;
                }
            }
            texture.SetPixels(pixels);
            texture.Apply();

            Rect rect = new Rect(0f, 0f, TextureSize, TextureSize);
            return Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), TextureSize);
        }
    }
}
