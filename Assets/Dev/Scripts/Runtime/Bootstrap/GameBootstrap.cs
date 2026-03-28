using System;
using UnityEngine;
using PackNFlow.Core;

namespace PackNFlow
{
    public class GameBootstrap : Singleton<GameBootstrap>
    {
        [SerializeField] private GameplaySettings gameplaySettings;
        [SerializeField] private UnitAppearanceConfig appearanceConfig;
        [SerializeField] private PlayflowController playflowController;

        [SerializeField] private LevelDirector levelDirector;

        public PlayflowController Playflow => playflowController;
        public LevelDirector Levels => levelDirector;

        private SignalBinding<PlayflowStateChangedEvent> _stateBinding;

        private void Start()
        {
            Application.targetFrameRate = 60;
            BootstrapSystems();
            LoadLevel();
        }

        private void BootstrapSystems()
        {
            gameplaySettings.Initialize();
            appearanceConfig.Initialize();
            levelDirector.Initialize();
            playflowController.Bootstrap(this);

            _stateBinding = new SignalBinding<PlayflowStateChangedEvent>(HandlePlayflowStateChanged);
            SignalBus<PlayflowStateChangedEvent>.Subscribe(_stateBinding);
        }

        private void OnDestroy()
        {
            SignalBus<PlayflowStateChangedEvent>.Unsubscribe(_stateBinding);
        }

        private void HandlePlayflowStateChanged(PlayflowStateChangedEvent e)
        {
            switch (e.CurrentState)
            {
                case PlayflowState.Active:
                    LoadLevel();
                    break;
            }
        }

        private void LoadLevel()
        {
            playflowController.LoadLevel();
        }

        public void ForceLevelEnd(bool success)
        {
            playflowController.TransitionTo(success ? PlayflowState.Cleared : PlayflowState.Failed);
        }
    }
}
