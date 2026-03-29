using UnityEngine;
using UnityEngine.UI;

namespace PackNFlow.UI
{
    public class LevelFailedPanel : PanelBase
    {
        [SerializeField] private Button retryButton;

        public override void Initialize()
        {
            base.Initialize();
            retryButton.onClick.AddListener(OnRetryClicked);
        }

        private void OnRetryClicked()
        {
            GameBootstrap.Instance.Playflow.TransitionTo(PlayflowState.Active);
        }

        private void OnDestroy()
        {
            retryButton.onClick.RemoveAllListeners();
        }
    }
}
