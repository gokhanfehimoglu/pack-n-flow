using System;
using System.Collections.Generic;
using UnityEngine;

namespace PackNFlow
{
    public class UnitColumnController : IDisposable
    {
        private readonly List<UnitColumn> _columns = new();
        private readonly List<Unit> _allUnits;
        private GridLayout _grid;
        private UnitController _controller;

        public event Action OnAllColumnsEmptied;

        public UnitColumnController(List<Unit> units, GridLayout grid, UnitController controller)
        {
            _allUnits = units;
            _grid = grid;
            _controller = controller;
        }

        public void Dispose() => OnAllColumnsEmptied = null;

        public void Initialize()
        {
            BuildColumns();
            UpdateDeployableVisuals();
        }

        private void BuildColumns()
        {
            foreach (var col in _columns) col.Dispose();
            _columns.Clear();

            var levelData = LevelDirector.Instance.ActiveLevelData;
            for (var i = 0; i < levelData.unitColumnCount; i++)
            {
                var col = new UnitColumn(_grid, i);
                col.OnColumnEmptied += HandleColumnEmptied;
                _columns.Add(col);
            }

            foreach (var unit in _allUnits)
                _columns[unit.Data.Coordinates.x].AddUnit(unit);

            foreach (var col in _columns) col.InitFrontUnit();
        }

        private void HandleColumnEmptied(UnitColumn _)
        {
            if (_columns.TrueForAll(c => c.IsComplete))
                OnAllColumnsEmptied?.Invoke();
        }

        public void UpdateDeployableVisuals()
        {
            foreach (var col in _columns)
            {
                if (col.UnitBehindFront != null)
                    col.UnitBehindFront.SetDeployable(false);

                var front = col.FrontUnit;
                if (front == null) continue;

                bool canDeploy = _controller.CanUnitDeploy(front, out _);
                front.SetDeployable(canDeploy);

                if (canDeploy && front.IsTethered && col.UnitBehindFront != null)
                {
                    if (col.UnitBehindFront == front.TetheredPartner)
                        col.UnitBehindFront.SetDeployable(true);
                }
            }
        }

        public UnitColumn GetColumn(int index)
        {
            return index >= 0 && index < _columns.Count ? _columns[index] : null;
        }

        public void RefreshDeployableVisuals() => UpdateDeployableVisuals();

        public void OnDeployedFromColumn(Unit unit)
        {
            _columns[unit.Data.Coordinates.x].OnFrontDeployed();
            UpdateDeployableVisuals();
        }
    }
}
