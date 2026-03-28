using UnityEngine;

namespace PackNFlow
{
    public class GridManager : MonoBehaviour
    {
        [SerializeField] private SlotPiece gridCellPrefab;
        
        public void Prepare(GridLayout unitGrid)
        {
            var cellPositions = GridMath.ComputeAllCellPositions(unitGrid);
            foreach (var pos in cellPositions)
            {
                var cell = Instantiate(gridCellPrefab, transform);
                cell.transform.position = pos;
                float size = unitGrid.CellSize * 0.75f * 0.7f;
                cell.transform.localScale = new Vector3(size, 0.1f, size);
            }
        }
    }
}