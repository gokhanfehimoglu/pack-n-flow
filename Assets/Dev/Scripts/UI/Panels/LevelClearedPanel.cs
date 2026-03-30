using UnityEngine;
using UnityEngine.UI;

namespace PackNFlow.UI
{
    public class LevelClearedPanel : PanelBase
    {
        [SerializeField] private Button continueButton;

        public override void Initialize()
        {
            base.Initialize();
            continueButton.onClick.AddListener(OnContinueClicked);
        }

        private static void OnContinueClicked()
        {
            GameBootstrap.Instance.Playflow.TransitionTo(PlayflowState.Home);
        }

        private void OnDestroy()
        {
            continueButton.onClick.RemoveAllListeners();
        }
    }
}
