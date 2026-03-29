using System.Collections.Generic;
using PackNFlow.Core;
using UnityEngine;

namespace PackNFlow.UI
{
    public class PanelManager : MonoBehaviour
    {
        [SerializeField] private GamePanel gamePanel;
        [SerializeField] private LevelClearedPanel levelClearedPanel;
        [SerializeField] private LevelFailedPanel levelFailedPanel;

        private readonly Dictionary<System.Type, PanelBase> _registry = new();

        private SignalBinding<PlayflowStateChangedEvent> _stateBinding;

        private void Awake()
        {
            Register(gamePanel);
            Register(levelClearedPanel);
            Register(levelFailedPanel);

            foreach (var panel in _registry.Values)
                panel.Initialize();

            _stateBinding = new SignalBinding<PlayflowStateChangedEvent>(OnPlayflowStateChanged);
            SignalBus<PlayflowStateChangedEvent>.Subscribe(_stateBinding);
        }

        private void OnDestroy()
        {
            SignalBus<PlayflowStateChangedEvent>.Unsubscribe(_stateBinding);
        }

        public void ShowOnly<T>() where T : PanelBase
        {
            foreach (var entry in _registry)
            {
                if (entry.Value is T)
                    entry.Value.Reveal();
                else
                    entry.Value.Conceal();
            }
        }

        private void Register(PanelBase panel)
        {
            if (panel != null)
                _registry[panel.GetType()] = panel;
        }

        private void OnPlayflowStateChanged(PlayflowStateChangedEvent signal)
        {
            switch (signal.CurrentState)
            {
                case PlayflowState.Active:
                    ShowOnly<GamePanel>();
                    break;
                case PlayflowState.Cleared:
                    ShowOnly<LevelClearedPanel>();
                    break;
                case PlayflowState.Failed:
                    ShowOnly<LevelFailedPanel>();
                    break;
            }
        }
    }
}
