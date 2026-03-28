using System.Collections.Generic;
using UnityEngine;

namespace PackNFlow
{
    public class GridAndRackVisualizer : MonoBehaviour
    {
        public IReadOnlyList<SlotPiece> RackPieces { get; private set; }

        private readonly List<SlotPiece> _pieces = new();

        public void Initialize(GridLayout unitGrid) { }
        public void Prepare(GridLayout unitGrid) { }
    }
}
