using DG.Tweening;
using PackNFlow.AudioSystem;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace PackNFlow.UI.Home
{
    public class TabBarController : Singleton<TabBarController>
    {
        [SerializeField] private HorizontalScrollSnap snapper;
        [SerializeField] private TabButton[] tabs;
        [SerializeField] private Image activeTabIndicator;
        [SerializeField] private AnimationCurve indicatorEase;
        [SerializeField] private float indicatorDuration;
        [SerializeField] private float scaleDuration;
        [SerializeField] private float activeTabIconHorizontalPadding;
        [SerializeField] private float activeTabIconTopPadding;
        [SerializeField] private float activeTabIconBottomPadding;
        [SerializeField] private float passiveTabIconHorizontalPadding;
        [SerializeField] private float passiveTabIconTopPadding;
        [SerializeField] private float passiveTabIconBottomPadding;

        private int _activeTabIndex = 2;

        private void OnEnable()
        {
            snapper.OnSelectionPageChangedEvent.AddListener(OnSelectionPageChanged);
        }

        public void OnTabClick(int tabIndex)
        {
            if (tabIndex == _activeTabIndex) return;
            AudioManager.Instance.PlayAudio(AudioName.ButtonClick);
            snapper.ChangePage(tabIndex);
        }

        private void OnSelectionPageChanged(int tabIndex)
        {
            var previousTab = tabs[_activeTabIndex];
            var activeTab = tabs[tabIndex];

            activeTabIndicator.transform.SetParent(activeTab.transform);
            activeTabIndicator.transform.SetSiblingIndex(tabIndex == tabs.Length - 1 ? 0 : 1);

            var active = new Vector3(activeTabIconHorizontalPadding, activeTabIconTopPadding,
                activeTabIconBottomPadding);
            var passive = new Vector3(passiveTabIconHorizontalPadding, passiveTabIconTopPadding,
                passiveTabIconBottomPadding);

            var seq = DOTween.Sequence();

            seq.Insert(0f, activeTabIndicator.transform.DOLocalMoveX(0f, indicatorDuration).SetEase(indicatorEase));
            seq.Insert(0f, previousTab.titleText.transform.DOScale(Vector3.zero, scaleDuration).SetEase(indicatorEase));
            seq.Insert(0f, DOVirtual.Vector3(
                active,
                passive,
                scaleDuration,
                value =>
                {
                    previousTab.icon.rectTransform.offsetMin = new Vector2(value.x, value.z);
                    previousTab.icon.rectTransform.offsetMax = new Vector2(-value.x, value.y);

                    var reverseValue = active - (passive - value);

                    activeTab.icon.rectTransform.offsetMin = new Vector2(reverseValue.x, reverseValue.z);
                    activeTab.icon.rectTransform.offsetMax = new Vector2(-reverseValue.x, reverseValue.y);
                }).SetEase(indicatorEase));
            seq.Insert(0f, activeTab.titleText.transform.DOScale(Vector3.one, scaleDuration).SetEase(indicatorEase));

            _activeTabIndex = tabIndex;
        }

        private void OnDisable()
        {
            snapper.OnSelectionPageChangedEvent.RemoveListener(OnSelectionPageChanged);
        }
    }
}