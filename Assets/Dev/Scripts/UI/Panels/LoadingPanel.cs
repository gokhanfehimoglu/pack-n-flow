using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace PackNFlow.UI
{
    public class LoadingPanel : MonoBehaviour
    {
        [SerializeField] private Image progressBar;

        private void Awake()
        {
            StartLoading();
        }

        private void StartLoading()
        {
            progressBar.DOFillAmount(1f, 0.6f).SetSpeedBased().SetEase(Ease.Linear).SetUpdate(true)
                .OnComplete(() => { gameObject.SetActive(false); });
        }
    }
}