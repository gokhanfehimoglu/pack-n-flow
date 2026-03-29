using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PackNFlow.UI
{
    public class GamePanel : PanelBase
    {
        [SerializeField] private Button retryButton;
        [SerializeField] private TextMeshProUGUI levelLabel;

        public override void Initialize()
        {
            base.Initialize();
            retryButton.onClick.AddListener(OnRetryClicked);
        }

        protected override void OnPrepareToShow()
        {
            levelLabel.SetText($"Level {LevelDirector.Instance.DisplayNumber}");
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
