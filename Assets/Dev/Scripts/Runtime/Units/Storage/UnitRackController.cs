using System.Collections.Generic;
using UnityEngine;

namespace PackNFlow
{
    public class UnitRackController : MonoBehaviour
    {
        public IReadOnlyList<SlotPiece> RackPieces { get; private set; }

        private readonly List<SlotPiece> _rackSlots = new();

        public void Initialize() { }
        public void Prepare(IReadOnlyList<SlotPiece> pieces) { }

        public bool TryStoreUnit(Unit unit) => false;
        public bool TryReleaseUnit(Unit unit, out SlotPiece slot)
        {
            slot = null;
            return false;
        }

        public void Reorganize() { }
    }
}
