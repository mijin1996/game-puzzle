using System;
using System.Collections.Generic;
using Quadrop.Core;
using Quadrop.Game;
using UnityEngine;

namespace Quadrop.Presentation
{
    /// <summary>
    /// 십자 모양 판을 그린다. 판 데이터는 고정이고, 조작 중인 변이 바뀌면 이 Transform만 Z축으로 돌린다.
    /// </summary>
    public sealed class BoardView : MonoBehaviour
    {
        private const int LaneOrder = 0;
        private const int MarkerOrder = 0;
        private const int LockedOrder = 1;
        private const int GhostOrder = 2;
        private const int PieceOrder = 3;
        private const float MarkerGap = 0.7f;
        private const float MarkerThickness = 0.35f;
        private const float MinRotateSeconds = 0.01f;
        private const float InactiveLaneBrightness = 0.45f;

        private readonly GridPos[] _cellBuffer = new GridPos[TetrominoShapes.CellCount];

        private GameSession _session;
        private SpriteCellPool _laneCells;
        private SpriteCellPool _lockedCells;
        private SpriteCellPool _ghostCells;
        private SpriteCellPool _pieceCells;
        private SpriteCellPool _markers;
        private int _size;
        private float _half;
        private float _degreesPerSecond;
        private float _angle;
        private float _targetAngle;

        public void Construct(GameSession session, Sprite cellSprite, float rotateSeconds)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }
            if (cellSprite == null)
            {
                throw new ArgumentNullException(nameof(cellSprite));
            }

            _session = session;
            _size = session.BoardSize;
            _half = (_size - 1) * 0.5f;
            _degreesPerSecond = SideExtensions.QuarterTurnDegrees / Mathf.Max(MinRotateSeconds, rotateSeconds);

            int cellCount = _size * _size;
            _laneCells = new SpriteCellPool(transform, cellSprite, cellCount, LaneOrder, "LaneCell");
            _markers = new SpriteCellPool(transform, cellSprite, SideExtensions.Count, MarkerOrder, "SideMarker");
            _lockedCells = new SpriteCellPool(transform, cellSprite, cellCount, LockedOrder, "Cell");
            _ghostCells = new SpriteCellPool(transform, cellSprite, TetrominoShapes.CellCount, GhostOrder, "Ghost");
            _pieceCells = new SpriteCellPool(transform, cellSprite,
                SideExtensions.Count * TetrominoShapes.CellCount, PieceOrder, "PieceCell");
            ScaleMarkers();
            RefreshLanes();

            _session.Events.BoardChanged += RefreshLockedCells;
            _session.Events.LinesMarked += FlashCells;
            _session.Events.ControlChanged += OnControlChanged;
            _session.Events.SideUnlocked += OnSideUnlocked;
        }

        private void OnDestroy()
        {
            if (_session == null)
            {
                return;
            }
            _session.Events.BoardChanged -= RefreshLockedCells;
            _session.Events.LinesMarked -= FlashCells;
            _session.Events.ControlChanged -= OnControlChanged;
            _session.Events.SideUnlocked -= OnSideUnlocked;
        }

        private void LateUpdate()
        {
            if (_session == null)
            {
                return;
            }
            UpdateRotation(Time.deltaTime);
            RenderPieces();
        }

        private void UpdateRotation(float deltaSeconds)
        {
            _angle = Mathf.MoveTowardsAngle(_angle, _targetAngle, _degreesPerSecond * deltaSeconds);
            transform.localRotation = Quaternion.Euler(0f, 0f, _angle);
        }

        private void RenderPieces()
        {
            int used = 0;
            Side controlled = _session.ControlledSide;
            for (int i = 0; i < SideExtensions.Count; i++)
            {
                Side side = (Side)i;
                if (!_session.TryGetPieceCells(side, _cellBuffer, out TetrominoType type))
                {
                    continue;
                }

                Color color = BlockPalette.For(type);
                if (side != controlled)
                {
                    color = BlockPalette.Dim(color, BlockPalette.UncontrolledBrightness);
                }
                for (int c = 0; c < TetrominoShapes.CellCount; c++)
                {
                    _pieceCells.Show(used, ToLocal(_cellBuffer[c]), color);
                    used++;
                }
            }
            _pieceCells.HideFrom(used);

            if (_session.TryGetGhostCells(_cellBuffer, out TetrominoType ghostType))
            {
                Color ghost = BlockPalette.WithAlpha(BlockPalette.For(ghostType), BlockPalette.GhostAlpha);
                for (int c = 0; c < TetrominoShapes.CellCount; c++)
                {
                    _ghostCells.Show(c, ToLocal(_cellBuffer[c]), ghost);
                }
            }
            else
            {
                _ghostCells.HideFrom(0);
            }
        }

        private void RefreshLockedCells()
        {
            IReadOnlyGrid grid = _session.Grid;
            for (int y = 0; y < _size; y++)
            {
                for (int x = 0; x < _size; x++)
                {
                    int index = y * _size + x;
                    byte value = grid.Get(x, y);
                    if (value == GridModel.Empty)
                    {
                        _lockedCells.Hide(index);
                    }
                    else
                    {
                        _lockedCells.Show(index, ToLocal(new GridPos(x, y)), BlockPalette.For((TetrominoType)value));
                    }
                }
            }
            RefreshLanes();
        }

        /// <summary>십자 모양 레인 바닥을 그린다. 활성 변의 레인은 밝게, 아직 안 열린 레인은 어둡게.</summary>
        private void RefreshLanes()
        {
            LaneLayout lanes = _session.Lanes;
            SideSet active = _session.ActiveSides;
            for (int y = 0; y < _size; y++)
            {
                for (int x = 0; x < _size; x++)
                {
                    GridPos cell = new GridPos(x, y);
                    int index = y * _size + x;
                    if (!lanes.IsInsideAnyLane(cell))
                    {
                        _laneCells.Hide(index);
                        continue;
                    }

                    bool isActive = false;
                    for (int s = 0; s < SideExtensions.Count; s++)
                    {
                        Side side = (Side)s;
                        if (active.Contains(side) && lanes.IsInsideLane(side, cell))
                        {
                            isActive = true;
                            break;
                        }
                    }
                    Color color = isActive
                        ? BlockPalette.BoardBackground
                        : BlockPalette.Dim(BlockPalette.BoardBackground, InactiveLaneBrightness);
                    _laneCells.Show(index, ToLocal(cell), color);
                }
            }
            RefreshMarkers();
        }

        private void FlashCells(IReadOnlyList<GridPos> cells)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                GridPos cell = cells[i];
                _lockedCells.Show(cell.Y * _size + cell.X, ToLocal(cell), BlockPalette.Flash);
            }
        }

        private void OnControlChanged(Side side)
        {
            _targetAngle = side.BoardAngleDegrees();
            RefreshMarkers();
        }

        private void OnSideUnlocked(Side side)
        {
            RefreshLanes();
        }

        /// <summary>각 레인의 바깥 가장자리에 붙는 막대 (조작 중 = 노랑, 활성 = 회색, 잠김 = 어두움)</summary>
        private void ScaleMarkers()
        {
            int laneWidth = _session.Lanes.Width;
            for (int i = 0; i < SideExtensions.Count; i++)
            {
                bool horizontal = ((Side)i).Outward().Y != 0;
                Vector3 scale = horizontal
                    ? new Vector3(laneWidth, MarkerThickness, 1f)
                    : new Vector3(MarkerThickness, laneWidth, 1f);
                _markers.SetScale(i, scale);
            }
        }

        private void RefreshMarkers()
        {
            float distance = _size * 0.5f + MarkerGap;
            for (int i = 0; i < SideExtensions.Count; i++)
            {
                Side side = (Side)i;
                GridPos outward = side.Outward();
                _markers.Show(i, new Vector3(outward.X * distance, outward.Y * distance, 0f), MarkerColor(side));
            }
        }

        private Color MarkerColor(Side side)
        {
            if (!_session.ActiveSides.Contains(side))
            {
                return BlockPalette.InactiveMarker;
            }
            return side == _session.ControlledSide ? BlockPalette.ControlledMarker : BlockPalette.ActiveMarker;
        }

        private Vector3 ToLocal(GridPos cell)
        {
            return new Vector3(cell.X - _half, cell.Y - _half, 0f);
        }
    }
}
