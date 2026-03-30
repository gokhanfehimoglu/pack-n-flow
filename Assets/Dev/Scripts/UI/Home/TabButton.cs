using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PackNFlow.UI.Home
{
    public class TabButton : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] public Image icon;
        [SerializeField] public TMP_Text titleText;
        [SerializeField] public RectTransform rt;
        [SerializeField] public int index;
        
        public void OnPointerClick(PointerEventData eventData)
        {
            TabBarController.Instance.OnTabClick(index);
        }
    }
}