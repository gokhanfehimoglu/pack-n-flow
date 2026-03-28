using System;
using System.Collections.Generic;
using UnityEngine;

namespace PackNFlow
{
    public class UnitController : MonoBehaviour
    {
        public event Action<Unit, bool> OnUnitDeployRequest;
        public event Action<Unit> OnUnitCompletedPath;
        public event Action<Unit> OnUnitConsumed;
        public event Action OnAllUnitsDispatched;
        public event Action OnDeployBlocked;

        public IReadOnlyList<Unit> ActiveUnits => _activeUnits;
        public List<Unit> ActiveUnitsList => _activeUnits;
        public GridLayout UnitGrid { get; private set; }

        private readonly List<Unit> _activeUnits = new();

        public void Initialize(Bounds conveyorBounds) { }
        public void Prepare(Bounds conveyorBounds) { }

        public void AddActiveUnit(Unit unit) => _activeUnits.Add(unit);
        public void RemoveActiveUnit(Unit unit) => _activeUnits.Remove(unit);

        public bool TryPullBlock(Unit unit, PixelBlock block, Edge edge) => false;
        public void RefreshDeployableVisuals() { }
        public void TransferFromColumn(Unit unit) { }
    }
}
