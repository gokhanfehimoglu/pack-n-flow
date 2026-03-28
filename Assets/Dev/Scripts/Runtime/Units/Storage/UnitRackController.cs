using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace PackNFlow
{
    public class UnitRackController : MonoBehaviour
    {
        public IReadOnlyList<SlotPiece> RackPieces => _slots;

        private SlotPiece[] _slots;

        public void Initialize() { }

        public void Prepare(IReadOnlyList<SlotPiece> pieces)
        {
            _slots = new SlotPiece[pieces.Count];
            for (var i = 0; i < pieces.Count; i++)
                _slots[i] = pieces[i];
        }

        public bool TryStoreUnit(Unit unit)
        {
            foreach (var slot in _slots)
            {
                if (slot.Occupant != null) continue;

                unit.MoveToRackSlot(slot);
                slot.Assign(unit);
                return true;
            }

            return false;
        }

        public bool TryReleaseUnit(Unit unit, out SlotPiece slot)
        {
            slot = null;
            for (var i = 0; i < _slots.Length; i++)
            {
                if (_slots[i].Occupant != unit) continue;
                slot = _slots[i];
                return true;
            }
            return false;
        }

        public void Reorganize()
        {
            var occupants = ListPool<Unit>.Get();

            foreach (var slot in _slots)
            {
                if (slot.Occupant == null) continue;
                occupants.Add(slot.Occupant);
                slot.Clear();
            }

            for (var i = 0; i < occupants.Count; i++)
            {
                var unit = occupants[i];
                var slot = _slots[i];
                slot.Assign(unit);
                unit.transform.SetParent(slot.transform);
                unit.transform.localPosition = Vector3.zero;
            }

            ListPool<Unit>.Release(occupants);
        }
    }
}
