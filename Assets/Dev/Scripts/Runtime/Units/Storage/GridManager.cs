using UnityEngine;

namespace PackNFlow
{
    public class GridManager : MonoBehaviour
    {
        [SerializeField] private SlotPiece gridCellPrefab;
        
        public void Prepare(GridLayout unitGrid)
        {
            PurgeCells();
            var cellPositions = GridMath.ComputeAllCellPositions(unitGrid);
            foreach (var pos in cellPositions)
            {
                var cell = Instantiate(gridCellPrefab, transform);
                cell.transform.position = pos;
                float size = unitGrid.CellSize * 0.75f * 0.7f;
                cell.transform.localScale = new Vector3(size, 0.1f, size);
            }
        }

        void PurgeCells()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                if (child != null)
                    DestroyImmediate(child.gameObject);
            }
        }
    }
}