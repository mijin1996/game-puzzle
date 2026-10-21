using System;
using Quadrop.Core;
using Quadrop.Game;
using UnityEngine;

namespace Quadrop.Presentation
{
    /// <summary>홀드 칸과 다음 피스 미리보기. 판과 달리 회전하지 않는다.</summary>
    public sealed class PreviewView : MonoBehaviour
    {
        private const int PreviewOrder = 3;
        private const float NextSpacingCells = 3f;
        private const float HeldUnavailableBrightness = 0.4f;

        private GameSession _session;
        private SpriteCellPool _holdCells;
        private SpriteCellPool _nextCells;

        public void Construct(GameSession session, Sprite cellSprite, Vector3 holdCenter, Vector3 nextTopCenter, float cellScale)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            if (cellSprite == null)
            {
                throw new ArgumentNullException(nameof(cellSprite));
            }

            Transform holdRoot = CreateRoot("Hold", holdCenter, cellScale);
            Transform nextRoot = CreateRoot("Next", nextTopCenter, cellScale);
            _holdCells = new SpriteCellPool(holdRoot, cellSprite, TetrominoShapes.CellCount, PreviewOrder, "HoldCell");
            _nextCells = new SpriteCellPool(nextRoot, cellSprite,
                TetrominoShapes.CellCount * Mathf.Max(1, session.NextPreviewCount), PreviewOrder, "NextCell");

            _session.Events.PieceSpawned += OnPieceSpawned;
            _session.Events.HoldChanged += Refresh;
            _session.Events.StateChanged += OnStateChanged;
        }

        private void OnDestroy()
        {
            if (_session == null)
            {
                return;
            }
            _session.Events.PieceSpawned -= OnPieceSpawned;
            _session.Events.HoldChanged -= Refresh;
            _session.Events.StateChanged -= OnStateChanged;
        }

        private void OnPieceSpawned(Side side)
        {
            Refresh();
        }

        private void OnStateChanged(GameStateId state)
        {
            Refresh();
        }

        private void Refresh()
        {
            TetrominoType held = _session.HoldType;
            if (held == TetrominoType.None)
            {
                _holdCells.HideFrom(0);
            }
            else
            {
                Color color = BlockPalette.For(held);
                if (!_session.CanHold)
                {
                    color = BlockPalette.Dim(color, HeldUnavailableBrightness);
                }
                DrawPiece(_holdCells, 0, held, Vector3.zero, color);
            }

            int count = _session.NextPreviewCount;
            for (int i = 0; i < count; i++)
            {
                TetrominoType next = _session.PeekNext(i);
                Vector3 center = new Vector3(0f, -i * NextSpacingCells, 0f);
                DrawPiece(_nextCells, i * TetrominoShapes.CellCount, next, center, BlockPalette.For(next));
            }
            _nextCells.HideFrom(count * TetrominoShapes.CellCount);
        }

        private static void DrawPiece(SpriteCellPool pool, int startIndex, TetrominoType type, Vector3 center, Color color)
        {
            float offset = (TetrominoShapes.BoxSize(type) - 1) * 0.5f;
            for (int i = 0; i < TetrominoShapes.CellCount; i++)
            {
                GridPos cell = TetrominoShapes.GetCell(type, 0, i);
                Vector3 position = center + new Vector3(cell.X - offset, cell.Y - offset, 0f);
                pool.Show(startIndex + i, position, color);
            }
        }

        private Transform CreateRoot(string objectName, Vector3 position, float scale)
        {
            GameObject root = new GameObject(objectName);
            root.transform.SetParent(transform, false);
            root.transform.position = position;
            root.transform.localScale = new Vector3(scale, scale, 1f);
            return root.transform;
        }
    }
}
