using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace PackNFlow
{
    [Serializable]
    public class UnitColumn : IDisposable
    {
        private List<Unit> _units;
        private GridLayout _grid;
        private int _cursor;
        private readonly int _columnIndex;
        private int _remaining;

        public int TotalCount => _units.Count;
        public int Remaining => TotalCount - _cursor;
        public bool IsComplete { get; private set; }

        public event Action<UnitColumn> OnColumnEmptied;

        public UnitColumn(GridLayout grid, int columnIndex)
        {
            _grid = grid;
            _columnIndex = columnIndex;
            _cursor = 0;
        }

        public void Dispose() => OnColumnEmptied = null;

        public void AddUnit(Unit unit)
        {
            _units ??= new List<Unit>();
            if (_units.Contains(unit)) return;
            _units.Add(unit);
        }

        public void InitFrontUnit()
        {
            if (_units.Count > _cursor)
                _units[_cursor].PromoteToFront();
        }

        public Unit FrontUnit => _cursor < _units.Count ? _units[_cursor] : null;

        public Unit UnitBehindFront => _cursor + 1 < _units.Count ? _units[_cursor + 1] : null;

        public int GetSlotIndex(Unit unit)
        {
            for (var i = _cursor; i < _units.Count; i++)
            {
                if (_units[i] == unit) return i - _cursor;
            }
            return -1;
        }

        public void OnFrontDeployed()
        {
            _cursor++;
            ShiftPositions();
        }

        private void ShiftPositions()
        {
            var idx = 0;
            for (var i = _cursor; i < _units.Count; i++)
            {
                var coords = new Vector2Int(_columnIndex, idx);
                if (GridMath.TryGetPositionFromCoords(_grid, coords, out var pos))
                {
                    DOTween.Kill(_units[i].transform);
                    _units[i].transform.DOMove(pos, 0.2f);
                    idx++;
                }
            }

            if (_cursor >= _units.Count)
            {
                IsComplete = true;
                OnColumnEmptied?.Invoke(this);
                return;
            }

            _units[_cursor].PromoteToFront();
        }
    }
}
