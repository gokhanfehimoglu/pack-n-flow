using UnityEngine;

namespace PackNFlow
{
    public class GridLayout
    {
        public float CellSize;
        public int ColumnCount;
        public int RowCount;
        public Vector3 Center;
        public Vector3 Origin;

        public GridLayout(float cellSize, int columnCount, int rowCount, Vector3 center)
        {
            CellSize = cellSize;
            ColumnCount = columnCount;
            RowCount = rowCount;
            Center = center;
            float originX = -((ColumnCount - 1) * CellSize * 0.5f);
            float originZ = Center.z + (RowCount * 0.5f * CellSize);
            Origin = new Vector3(originX, 0f, originZ);
        }
    }
}
