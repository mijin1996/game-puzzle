using Quadrop.Core;
using UnityEngine;

namespace Quadrop.Presentation
{
    /// <summary>화면 색상을 한 곳에 모은다 (매직 넘버 금지).</summary>
    public static class BlockPalette
    {
        private static readonly Color[] TypeColors =
        {
            new Color(0.30f, 0.86f, 0.96f),   // I
            new Color(0.98f, 0.85f, 0.25f),   // O
            new Color(0.70f, 0.40f, 0.95f),   // T
            new Color(0.40f, 0.88f, 0.42f),   // S
            new Color(0.96f, 0.36f, 0.38f),   // Z
            new Color(0.32f, 0.48f, 0.98f),   // J
            new Color(0.98f, 0.60f, 0.24f)    // L
        };

        public static Color Backdrop { get; } = new Color(0.04f, 0.04f, 0.06f);
        public static Color BoardBackground { get; } = new Color(0.09f, 0.09f, 0.13f);
        public static Color Flash { get; } = new Color(1f, 1f, 1f);
        public static Color ControlledMarker { get; } = new Color(1f, 0.92f, 0.45f);
        public static Color ActiveMarker { get; } = new Color(0.45f, 0.48f, 0.60f);
        public static Color InactiveMarker { get; } = new Color(0.14f, 0.14f, 0.19f);
        public static Color Text { get; } = new Color(0.92f, 0.93f, 0.97f);
        public static Color TextDim { get; } = new Color(0.60f, 0.62f, 0.70f);

        public const float GhostAlpha = 0.28f;
        public const float UncontrolledBrightness = 0.7f;

        public static Color For(TetrominoType type)
        {
            int index = (int)type - 1;
            if (index < 0 || index >= TypeColors.Length)
            {
                return Color.white;
            }
            return TypeColors[index];
        }

        public static Color Dim(Color color, float brightness)
        {
            return new Color(color.r * brightness, color.g * brightness, color.b * brightness, color.a);
        }

        public static Color WithAlpha(Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }
    }
}
