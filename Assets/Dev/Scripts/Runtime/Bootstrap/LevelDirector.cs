using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using PackNFlow.Core;
using UnityEngine.SceneManagement;

namespace PackNFlow
{
    public class LevelDirector : Singleton<LevelDirector>
    {
        [SerializeField] private List<LevelData> levels = new();

        [SerializeField] private bool useTestLevel;
        [ShowIf("useTestLevel")]
        [SerializeField] private LevelData testLevel;

        private LevelProgression _progression;
        private SignalBinding<PlayflowStateChangedEvent> _stateBinding;

        public LevelData ActiveLevelData => useTestLevel ? testLevel : levels[_progression.WrappedIndex];
        public int DisplayNumber => _progression.Index + 1;
        public bool IsReady { get; private set; }

        public void Initialize()
        {
            _progression = new LevelProgression(GameSaveKeys.LevelIndexKey, levels.Count);
            _stateBinding = new SignalBinding<PlayflowStateChangedEvent>(OnPlayflowStateChanged);
            SignalBus<PlayflowStateChangedEvent>.Subscribe(_stateBinding);
        }

        private void OnPlayflowStateChanged(PlayflowStateChangedEvent e)
        {
            if (e.CurrentState == PlayflowState.Cleared)
                _progression.Advance();
            else if (e.CurrentState == PlayflowState.Home)
                SceneManager.LoadScene("Dev/Scenes/Home");
        }

        private void OnDestroy()
        {
            SignalBus<PlayflowStateChangedEvent>.Unsubscribe(_stateBinding);
        }

        public void PrepareForPlay()
        {
            IsReady = true;
        }

        [System.Serializable]
        public struct LevelProgression
        {
            private readonly string _prefsKey;
            private readonly int _levelCount;

            public int Index
            {
                get => PlayerPrefs.GetInt(_prefsKey, 0);
                private set => PlayerPrefs.SetInt(_prefsKey, value);
            }

            public int WrappedIndex => Index % _levelCount;

            public LevelProgression(string prefsKey, int levelCount)
            {
                _prefsKey = prefsKey;
                _levelCount = levelCount;
            }

            public void Advance() => Index++;
        }
    }
}
