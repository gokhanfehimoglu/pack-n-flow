using UnityEngine;

namespace PackNFlow.UI
{
    public abstract class PanelBase : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;

        public bool IsInitialized { get; private set; }

        public virtual void Initialize()
        {
            IsInitialized = true;
        }

        public void Reveal()
        {
            OnPrepareToShow();
            gameObject.SetActive(true);
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            OnRevealed();
        }

        public void Conceal()
        {
            OnPrepareToHide();
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
            OnConcealed();
        }

        protected virtual void OnPrepareToShow() { }
        protected virtual void OnRevealed() { }
        protected virtual void OnPrepareToHide() { }
        protected virtual void OnConcealed() { }
    }
}
