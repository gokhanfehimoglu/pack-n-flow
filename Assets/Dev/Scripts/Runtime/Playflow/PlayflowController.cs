using System;
using System.Collections.Generic;
using UnityEngine;
using PackNFlow.Core;

namespace PackNFlow
{
    public class PlayflowController : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private ConveyorSystem conveyor;
        [SerializeField] private PixelBlockController pixelBlockController;
        [SerializeField] private UnitController unitController;
        [SerializeField] private UnitRackController rackController;
        [SerializeField] private RackManager rackManager;
        [SerializeField] private GridManager gridManager;

        [Header("Prefabs")]
        [SerializeField] private Unit unitPrefab;

        private GameBootstrap _bootstrap;
        private LevelDirector _levelDirector;

        private PlayflowState _state = PlayflowState.Menu;
        private float _lastDeployTime;
        private Dictionary<PlayflowState, Action> _stateHandlers;

        public bool IsSetup { get; private set; }
        public bool IsReady { get; private set; }
        public IReadOnlyList<Unit> ActiveUnits => unitController?.ActiveUnits;

        public void Bootstrap(GameBootstrap bootstrap)
        {
            _bootstrap = bootstrap;
            _levelDirector = bootstrap.Levels;
            BuildStateHandlers();
            TransitionTo(PlayflowState.Active);
            IsSetup = true;

            unitController.OnUnitDeployRequest += HandleUnitDeployRequest;
            unitController.OnUnitCompletedPath += HandleUnitPathCompleted;
            unitController.OnUnitConsumed += HandleUnitDestroyed;
            unitController.OnAllUnitsDispatched += HandleAllUnitsDispatched;
            unitController.OnDeployBlocked += HandleDeployBlocked;

            pixelBlockController.OnAllBlocksCleared += HandleAllBlocksCleared;
        }

        private void BuildStateHandlers()
        {
            _stateHandlers = new Dictionary<PlayflowState, Action>
            {
                [PlayflowState.Active] = EnterActive,
                [PlayflowState.Cleared] = EnterCleared,
                [PlayflowState.Failed] = EnterFailed
            };
        }

        public void TransitionTo(PlayflowState next)
        {
            var previous = _state;
            _state = next;

            SignalBus<PlayflowStateChangedEvent>.Publish(new PlayflowStateChangedEvent
            {
                PreviousState = previous,
                CurrentState = next
            });

            Debug.Log($"Playflow state: {next}");

            if (_stateHandlers.TryGetValue(next, out var handler))
                handler.Invoke();
        }

        public void LoadLevel()
        {
            if (!IsSetup) return;

            _levelDirector.PrepareForPlay();
            var levelData = _levelDirector.ActiveLevelData;

            conveyor.Prepare(levelData.conveyorCarriageCount);
            unitController.Initialize(conveyor.Bounds.Value);
            unitController.ReadyCarriageCount = levelData.conveyorCarriageCount;
            unitController.Prepare(conveyor.Bounds.Value);
            pixelBlockController.Initialize(conveyor.Bounds.Value);
            pixelBlockController.Prepare(conveyor.Bounds.Value);
            rackManager.Prepare(unitController.UnitGrid);
            rackController.Prepare(rackManager.Racks);
            gridManager.Prepare(unitController.UnitGrid);

            IsReady = true;
        }

        private void Update()
        {
            if (!IsReady) return;

            BlockScanner.Sweep(
                unitController.ActiveUnitsList,
                pixelBlockController.TryFindBlockForUnit,
                unitController.TryPullBlock
            );
        }

        private void EnterActive() { }

        private void EnterCleared()
        {
            Debug.Log("Level cleared!");
        }

        private void EnterFailed()
        {
            Debug.Log("Level failed.");
        }

        private void HandleUnitDeployRequest(Unit unit, bool skipInterval)
        {
            if (!conveyor.TryGetReadyCarriage(out var carriage))
                return;

            if (!skipInterval && Time.time < _lastDeployTime + GameplaySettings.Instance.units.minDeployInterval)
                return;

            _lastDeployTime = Time.time;

            conveyor.DispatchCarriage(carriage);
            unitController.ReadyCarriageCount--;
            unitController.AddActiveUnit(unit);

            unit.OnBoardingCompleted += OnUnitBoarded;
            unit.BoardTheCarriage(carriage);

            if (rackController.TryReleaseUnit(unit, out var slot))
            {
                slot.Clear();
                rackController.Reorganize();
                unitController.RefreshDeployableVisuals();
            }
            else
            {
                unitController.TransferFromColumn(unit);
            }
        }

        private void OnUnitBoarded(Unit unit, ConveyorCarriage carriage)
        {
            unit.OnBoardingCompleted -= OnUnitBoarded;
            carriage.BeginMovement();
        }

        private void HandleUnitPathCompleted(Unit unit)
        {
            unitController.RemoveActiveUnit(unit);
            unitController.ReadyCarriageCount++;

            if (unit.IsCapacityDepleted)
            {
                unitController.RefreshDeployableVisuals();
                return;
            }

            if (!rackController.TryStoreUnit(unit))
            {
                Debug.Log("Storage rack is full!");
                unit.LeaveConveyor();
                TransitionTo(PlayflowState.Failed);
            }
            else
            {
                unit.LeaveConveyor();
            }

            unitController.RefreshDeployableVisuals();
        }

        private void HandleUnitDestroyed(Unit unit)
        {
            unitController.RemoveActiveUnit(unit);
            conveyor.ReleaseCarriageForUnit(unit);
            unitController.ReadyCarriageCount++;
            unitController.RefreshDeployableVisuals();
        }

        private void HandleAllUnitsDispatched()
        {
            Debug.Log("All units dispatched");
        }

        private void HandleDeployBlocked()
        {
            conveyor.PlayCapacityWarning();
        }

        private void HandleAllBlocksCleared()
        {
            TransitionTo(PlayflowState.Cleared);
        }
    }
}
