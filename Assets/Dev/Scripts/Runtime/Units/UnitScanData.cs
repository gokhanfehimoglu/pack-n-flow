using System.Collections.Generic;
using UnityEngine;

namespace PackNFlow
{
    public class UnitScanData
    {
        private readonly HashSet<int> _southScan = new();
        private readonly HashSet<int> _northScan = new();
        private readonly HashSet<int> _eastScan = new();
        private readonly HashSet<int> _westScan = new();

        private static int HashCoord(Vector2Int c) => (c.x << 16) | (c.y & 0xFFFF);

        public Vector3? LastCheckPosition { get; private set; }

        public void UpdateCheckPosition(Vector3 pos) => LastCheckPosition = pos;

        public void Reset()
        {
            _southScan.Clear();
            _northScan.Clear();
            _eastScan.Clear();
            _westScan.Clear();
            LastCheckPosition = null;
        }

        public bool HasScanned(Edge edge, Vector2Int coords)
        {
            return edge switch
            {
                Edge.South => _southScan.Contains(HashCoord(coords)),
                Edge.East => _eastScan.Contains(HashCoord(coords)),
                Edge.North => _northScan.Contains(HashCoord(coords)),
                Edge.West => _westScan.Contains(HashCoord(coords)),
                _ => false
            };
        }

        public bool IsUnscanned(Edge edge, Vector2Int coords) => !HasScanned(edge, coords);

        public void RecordScan(Edge edge, Vector2Int coords)
        {
            var hash = HashCoord(coords);
            switch (edge)
            {
                case Edge.South: _southScan.Add(hash); break;
                case Edge.East: _eastScan.Add(hash); break;
                case Edge.North: _northScan.Add(hash); break;
                case Edge.West: _westScan.Add(hash); break;
            }
        }
    }
}
