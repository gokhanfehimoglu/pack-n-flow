using UnityEngine;

namespace PackNFlow
{
    public static class GridMath
    {
        public static bool TryGetPositionFromCoords(GridLayout layout, Vector2Int coords, out Vector3 cellCenter)
        {
            cellCenter = Vector3.zero;

            if (coords.x < 0 || coords.x >= layout.ColumnCount || coords.y < 0 || coords.y >= layout.RowCount)
                return false;

            float offsetX = (layout.ColumnCount - 1) * layout.CellSize * 0.5f;
            float originZ = layout.Center.z + (layout.RowCount * 0.5f * layout.CellSize);
            Vector3 origin = new Vector3(-offsetX, 0f, originZ);

            cellCenter = origin + (new Vector3(coords.x, 0f, -coords.y) * layout.CellSize);
            return true;
        }

        public static bool TryGetCoordsFromPosition(GridLayout layout, Vector3 worldPos, out Vector2Int coords,
            out Vector3 cellCenter)
        {
            float cellSize = layout.CellSize;
            int columns = layout.ColumnCount;
            int rows = layout.RowCount;

            coords = Vector2Int.zero;
            cellCenter = Vector3.zero;

            float half = cellSize * 0.5f;
            float offsetX = (columns - 1) * cellSize * 0.5f;
            float originZ = layout.Center.z + (rows * 0.5f * cellSize);
            Vector3 origin = new Vector3(-offsetX, 0f, originZ);

            worldPos.y = 0f;

            int x = Mathf.FloorToInt(((worldPos.x - origin.x) + half) / cellSize);
            int y = Mathf.FloorToInt(((-(worldPos.z - origin.z)) + half) / cellSize);

            if (x < 0 || x >= columns) return false;
            if (y < 0 || y >= rows) return false;

            cellCenter = origin + new Vector3(x, 0f, -y) * cellSize;
            coords = new Vector2Int(x, y);
            return true;
        }

        public static GridLayout BuildUnitGrid(LevelData levelData, float conveyorMinZ)
        {
            int columns = levelData.unitColumnCount;
            float cellSize = levelData.unitGridCellSize;
            int rows = levelData.unitColumnDepth;

            float centerZ = conveyorMinZ -
                            (rows * cellSize * 0.5f) -
                            (GameplaySettings.Instance.units.unitGridZOffsetByCellSize * cellSize);

            Vector3 center = Vector3.forward * centerZ;
            return new GridLayout(cellSize, columns, rows, center);
        }

        public static Vector3[] ComputeRackSlotPositions(LevelData levelData, GridLayout unitGrid)
        {
            int slotCount = levelData.rackSlotCount;
            float cellSize = levelData.unitGridCellSize;
            Vector3[] positions = new Vector3[slotCount];
            float startX = -((cellSize / 2f) * slotCount);

            float topZ = unitGrid.Center.z + (unitGrid.RowCount * 0.5f * unitGrid.CellSize);
            Vector3 anchor = new Vector3(startX, 0f, topZ + cellSize);

            for (int i = 0; i < slotCount; i++)
            {
                positions[i] = anchor + (Vector3.right * (cellSize * i)) + (Vector3.right * cellSize * 0.5f);
            }

            return positions;
        }

        public static Vector3[] ComputeAllCellPositions(GridLayout layout)
        {
            Vector3[] positions = new Vector3[layout.ColumnCount * layout.RowCount];
            int idx = 0;
            for (int x = 0; x < layout.ColumnCount; x++)
            {
                for (int y = 0; y < layout.RowCount; y++)
                {
                    positions[idx] = new Vector3(x, 0f, -y) * layout.CellSize + layout.Origin;
                    idx++;
                }
            }

            return positions;
        }

        public static GridLayout BuildBlockZoneGrid(LevelData levelData, Vector3 conveyorCenter)
        {
            int columns = levelData.blockZoneWidth;
            float cellSize = levelData.blockZoneCellSize;
            int rows = levelData.blockZoneHeight;
            Vector3 center = new Vector3(conveyorCenter.x, 0f, conveyorCenter.z);
            return new GridLayout(cellSize, columns, rows, center);
        }

        public static Bounds GetGridBounds(GridLayout layout)
        {
            Vector3 size = new Vector3(layout.ColumnCount * layout.CellSize, 0f, layout.RowCount * layout.CellSize);
            Vector3 boundsCenter = layout.Center + (Vector3.forward * (layout.CellSize * 0.5f));
            return new Bounds(boundsCenter, size);
        }
    }
}