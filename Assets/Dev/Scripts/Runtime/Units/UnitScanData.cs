using System.Collections.Generic;
using UnityEngine;

namespace PackNFlow
{
    public class UnitScanData
    {
        private readonly HashSet<int> _rowsUsedFromSouth = new();
        private readonly HashSet<int> _rowsUsedFromNorth = new();
        private readonly HashSet<int> _colsUsedFromEast = new();
        private readonly HashSet<int> _colsUsedFromWest = new();

        public Vector3? LastCheckPosition { get; private set; }

        public void UpdateCheckPosition(Vector3 pos) => LastCheckPosition = pos;

        public void Reset()
        {
            _rowsUsedFromSouth.Clear();
            _rowsUsedFromNorth.Clear();
            _colsUsedFromEast.Clear();
            _colsUsedFromWest.Clear();
            LastCheckPosition = null;
        }

        public bool CanEngageLine(Edge edge, Vector2Int coords)
        {
            return edge switch
            {
                Edge.South => !_rowsUsedFromSouth.Contains(coords.x),
                Edge.East => !_colsUsedFromEast.Contains(coords.y),
                Edge.North => !_rowsUsedFromNorth.Contains(coords.x),
                Edge.West => !_colsUsedFromWest.Contains(coords.y),
                _ => false
            };
        }

        public void SealLine(Edge edge, Vector2Int coords)
        {
            switch (edge)
            {
                case Edge.South:
                    _rowsUsedFromSouth.Add(coords.x);
                    break;
                case Edge.East:
                    _colsUsedFromEast.Add(coords.y);
                    break;
                case Edge.North:
                    _rowsUsedFromNorth.Add(coords.x);
                    break;
                case Edge.West:
                    _colsUsedFromWest.Add(coords.y);
                    break;
            }
        }
    }
}
