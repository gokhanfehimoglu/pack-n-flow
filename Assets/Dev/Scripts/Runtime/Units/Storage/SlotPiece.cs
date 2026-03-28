using UnityEngine;

namespace PackNFlow
{
    public class SlotPiece : MonoBehaviour
    {
        public Unit Occupant { get; private set; }

        public void Assign(Unit unit) => Occupant = unit;
        public void Clear() => Occupant = null;
    }
}
