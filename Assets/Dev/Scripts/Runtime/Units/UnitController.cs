using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace PackNFlow
{
    public class UnitController : MonoBehaviour
    {
        [SerializeField] private Unit unitPrefab;
        [SerializeField] private Transform unitAnchor;
        [SerializeField] private TetherLine tetherPrefab;

        private GridLayout _unitGrid;
        private Bounds _conveyorBounds;
        private UnitColumnController _laneController;
        private readonly List<Unit> _allUnits = new();
        private readonly List<Unit> _activeUnits = new();
        private readonly List<TetherLine> _tethers = new();

        public IReadOnlyList<Unit> ActiveUnits => _activeUnits;
        public List<Unit> ActiveUnitsList => _activeUnits;
        public GridLayout UnitGrid => _unitGrid;
        public int ReadyCarriageCount { get; set; }

        public event Action<Unit, bool> OnUnitDeployRequest;
        public event Action<Unit> OnUnitCompletedPath;
        public event Action<Unit> OnUnitConsumed;
        public event Action OnAllUnitsDispatched;
        public event Action OnDeployBlocked;

        public void Initialize(Bounds conveyorBounds)
        {
            _conveyorBounds = conveyorBounds;
        }

        public void Prepare(Bounds conveyorBounds)
        {
            _conveyorBounds = conveyorBounds;
            _unitGrid = GridMath.BuildUnitGrid(
                LevelDirector.Instance.ActiveLevelData, _conveyorBounds.min.z);

            PurgeAllUnits();
            SpawnAllUnits();
            BuildLanes();
        }

        private void SpawnAllUnits()
        {
            var levelData = LevelDirector.Instance.ActiveLevelData;

            for (var i = 0; i < levelData.unitColumnCount; i++)
            {
                var laneData = levelData.columnDataList[i];
                foreach (var unitData in laneData.Entries)
                    _allUnits.Add(CreateUnit(unitData));
            }

            foreach (var u in _allUnits)
            {
                if (u.Data.TetheredUnitId == -1 || u.IsTethered) continue;

                foreach (var partner in _allUnits)
                {
                    if (u.Data.TetheredUnitId != partner.Data.Id) continue;

                    var tether = Instantiate(tetherPrefab);
                    tether.Connect(u, partner);
                    u.SetTethered(partner, tether);
                    partner.SetTethered(u, tether);
                    _tethers.Add(tether);
                }
            }
        }

        private Unit CreateUnit(UnitData data)
        {
            if (!GridMath.TryGetPositionFromCoords(_unitGrid, data.Coordinates, out var pos))
            {
                Debug.LogError($"Unit coordinates {data.Coordinates} are outside the grid!");
                return null;
            }

            var unit = Instantiate(unitPrefab, unitAnchor);
            unit.transform.position = pos;
            unit.Configure(data);
            unit.OnDeployRequest += HandleUnitDeployRequest;
            unit.OnPathFinished += HandleUnitPathFinished;
            unit.OnCapacityDepleted += HandleUnitDepleted;
            return unit;
        }

        private void BuildLanes()
        {
            _laneController?.Dispose();
            _laneController = new UnitColumnController(_allUnits, _unitGrid, this);
            _laneController.Initialize();
            _laneController.OnAllColumnsEmptied += HandleAllDispatched;
        }

        private void HandleUnitDeployRequest(Unit unit) => EvaluateDeploy(unit);
        private void HandleUnitPathFinished(Unit unit) => OnUnitCompletedPath?.Invoke(unit);
        private void HandleUnitDepleted(Unit unit) => OnUnitConsumed?.Invoke(unit);
        private void HandleAllDispatched() => OnAllUnitsDispatched?.Invoke();

        private void OnDestroy()
        {
            OnUnitDeployRequest = null;
            OnUnitCompletedPath = null;
            OnUnitConsumed = null;
            OnAllUnitsDispatched = null;
            OnDeployBlocked = null;
        }

        public void AddActiveUnit(Unit unit) => _activeUnits.Add(unit);
        public void RemoveActiveUnit(Unit unit) => _activeUnits.Remove(unit);

        public bool TryPullBlock(Unit unit, PixelBlock block, Edge edge)
        {
            if (block == null || unit.IsCapacityDepleted) return false;

            var levelData = LevelDirector.Instance.ActiveLevelData;
            int resolvedColorId = levelData.ResolveUnitColorToken(block.Data.ColorId);
            if (unit.Data.ColorId != resolvedColorId) return false;

            if (!unit.ScanData.IsUnscanned(edge, block.Data.Coordinates)) return false;

            unit.TriggerPull(block, edge);
            return true;
        }

        public bool CanUnitDeploy(Unit unit, out bool withPartner)
        {
            withPartner = false;

            if (ReadyCarriageCount <= 0 || unit.IsInConveyor)
            {
                OnDeployBlocked?.Invoke();
                return false;
            }

            if (!unit.IsTethered)
                return unit.IsInFront;

            if (ReadyCarriageCount <= 1)
            {
                OnDeployBlocked?.Invoke();
                return false;
            }

            var partner = unit.TetheredPartner;
            if (partner == null || partner.IsInConveyor) return false;

            if (unit.IsInFront)
            {
                if (partner.IsInFront)
                {
                    withPartner = true;
                    return true;
                }

                var lane = _laneController.GetColumn(unit.Data.Coordinates.x);
                if (lane != null && lane.GetSlotIndex(partner) == 1)
                {
                    withPartner = true;
                    return true;
                }

                return false;
            }

            if (partner.IsInFront)
            {
                var partnerLane = _laneController.GetColumn(partner.Data.Coordinates.x);
                if (partnerLane != null && partnerLane.GetSlotIndex(unit) == 1)
                {
                    withPartner = true;
                    return true;
                }
            }

            return false;
        }

        private void EvaluateDeploy(Unit unit)
        {
            if (!CanUnitDeploy(unit, out var withPartner))
            {
                unit.PlayShakeOffAnim();
                return;
            }

            if (!withPartner)
            {
                OnUnitDeployRequest?.Invoke(unit, false);
                return;
            }

            var partner = unit.TetheredPartner;
            var tether = unit.Tether;

            Unit first = unit.IsInFront ? unit : partner;
            Unit second = unit.IsInFront ? partner : unit;

            unit.SeverTether();
            partner.SeverTether();
            tether.Disconnect();

            OnUnitDeployRequest?.Invoke(first, false);

            DOVirtual.DelayedCall(GameplaySettings.Instance.units.minDeployInterval, () =>
            {
                if (second == null || second.IsInConveyor) return;
                OnUnitDeployRequest?.Invoke(second, true);
            });
        }

        public void RefreshDeployableVisuals() => _laneController?.RefreshDeployableVisuals();

        public void TransferFromColumn(Unit unit) => _laneController.OnDeployedFromColumn(unit);

        private void PurgeAllUnits()
        {
            _activeUnits.Clear();

            foreach (var unit in _allUnits)
                DestroyImmediate(unit.gameObject);
            _allUnits.Clear();

            foreach (var t in _tethers)
            {
                t.Disconnect();
                DestroyImmediate(t.gameObject);
            }
            _tethers.Clear();
        }
    }
}
