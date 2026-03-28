using System;
using System.Collections.Generic;
using UnityEngine;
using PackNFlow.Core;

namespace PackNFlow
{
    public class PixelBlockController : MonoBehaviour
    {
        [SerializeField] private PixelBlock pixelBlockPrefab;
        [SerializeField] private Transform blockAnchor;

        private PixelBlock[,] _grid;
        private Bounds _blockZoneBounds;
        private GridLayout _blockGrid;

        private int _totalBlockCount;
        private int _clearedBlockCount;

        public event Action OnAllBlocksCleared;

        public void Initialize(Bounds conveyorBounds)
        {
            _blockGrid = GridMath.BuildBlockZoneGrid(
                LevelDirector.Instance.ActiveLevelData, conveyorBounds.center);
            _blockZoneBounds = GridMath.GetGridBounds(_blockGrid);
        }

        public void Prepare(Bounds conveyorBounds)
        {
            PurgeAllBlocks();

            _clearedBlockCount = 0;
            _totalBlockCount = 0;

            var levelData = LevelDirector.Instance.ActiveLevelData;
            _grid = new PixelBlock[levelData.blockZoneWidth, levelData.blockZoneHeight];

            SpawnBlocks(levelData);
            BroadcastProgress();
        }

        private void OnDestroy() => OnAllBlocksCleared = null;

        private void SpawnBlocks(LevelData levelData)
        {
            foreach (var entry in levelData.blockEntries)
            {
                if (!GridMath.TryGetPositionFromCoords(_blockGrid, entry.Coordinates, out var pos))
                {
                    Debug.LogError($"Block coordinates {entry.Coordinates} are outside the grid!");
                    continue;
                }

                var block = Instantiate(pixelBlockPrefab, blockAnchor);
                block.transform.position = pos;
                block.Initialize(entry, levelData.blockZoneCellSize);
                block.OnPulled += HandleBlockPulled;
                _grid[entry.Coordinates.x, entry.Coordinates.y] = block;
                _totalBlockCount++;
            }
        }

        private void HandleBlockPulled(PixelBlock _)
        {
            _clearedBlockCount++;
            BroadcastProgress();

            if (_clearedBlockCount >= _totalBlockCount)
                OnAllBlocksCleared?.Invoke();
        }

        public bool TryFindBlockForUnit(Unit unit, out PixelBlock block, out Edge edge)
        {
            block = null;
            edge = Edge.South;

            if (unit == null) return false;

            var unitPos = unit.transform.position;
            edge = LineScan.DetermineEdge(unitPos, _blockZoneBounds);

            if (!IsValidEdge(edge)) return false;

            var scanSteps = ComputeInterpolationSteps(unit);
            var lastPos = unit.ScanData != null
                ? unit.ScanData.LastCheckPosition
                : (Vector3?)null;
            var currentPos = unitPos;

            scanSteps = Mathf.Max(1, scanSteps);

            for (var i = 1; i <= scanSteps; i++)
            {
                var scanPos = PositionInterpolator.Interpolate(lastPos ?? currentPos, currentPos, i, scanSteps);

                if (ScanLineForBlock(scanPos, edge, out block))
                    return true;
            }

            unit.ScanData.UpdateCheckPosition(currentPos);
            return false;
        }

        private int ComputeInterpolationSteps(Unit unit)
        {
            var lastPos = unit.ScanData?.LastCheckPosition;
            if (lastPos == null) return 1;

            var levelData = LevelDirector.Instance.ActiveLevelData;
            return PositionInterpolator.ComputeStepCount(
                lastPos, unit.transform.position, levelData.blockZoneCellSize);
        }

        private bool ScanLineForBlock(Vector3 scanOrigin, Edge edge, out PixelBlock found)
        {
            found = null;

            var gridPos = LineScan.ProjectToGridPlane(scanOrigin, edge, _blockGrid);
            if (!GridMath.TryGetCoordsFromPosition(_blockGrid, gridPos, out var coords, out _))
                return false;

            if (!LineScan.ResolveStartAndDirection(
                    edge, coords, _blockGrid.ColumnCount, _blockGrid.RowCount,
                    out var x, out var y, out var dx, out var dy, out var steps))
                return false;

            for (var i = 0; i < steps; i++)
            {
                if (IsAliveBlockAt(x, y, out found))
                    return true;

                x += dx;
                y += dy;
            }

            return false;
        }

        private bool IsAliveBlockAt(int x, int y, out PixelBlock block)
        {
            block = null;

            if (x < 0 || x >= _blockGrid.ColumnCount)
                return false;

            if (y < 0 || y >= _blockGrid.RowCount)
                return false;

            var candidate = _grid[x, y];
            if (candidate == null || candidate.IsMarkedForRemoval)
                return false;

            block = candidate;
            return true;
        }

        private static bool IsValidEdge(Edge edge) => edge is Edge.South or Edge.North or Edge.East or Edge.West;

        private void BroadcastProgress()
        {
            float ratio = _totalBlockCount > 0 ? (float)_clearedBlockCount / _totalBlockCount : 0f;
            SignalBus<ClearProgressEvent>.Publish(new ClearProgressEvent
            {
                Ratio = ratio,
                ClearedCount = _clearedBlockCount,
                TotalCount = _totalBlockCount
            });
        }

        private void PurgeAllBlocks()
        {
            if (_grid == null) return;

            for (var x = 0; x < _grid.GetLength(0); x++)
            {
                for (var y = 0; y < _grid.GetLength(1); y++)
                {
                    if (_grid[x, y] != null)
                    {
                        DestroyImmediate(_grid[x, y].gameObject);
                        _grid[x, y] = null;
                    }
                }
            }

            _grid = null;
        }
    }
}
