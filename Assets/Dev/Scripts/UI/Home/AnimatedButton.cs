using DG.Tweening;
using PackNFlow.AudioSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PackNFlow.UI.Home
{
	public class AnimatedButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
	{
		private Tween _downTween;
		private Tween _upTween;
		private Button _button;

		private void Awake()
		{
			_button = GetComponent<Button>();
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (!_button.interactable) return;
			
			_upTween?.Kill();
			_downTween = transform.DOScale(Vector3.one * 0.8f, 0.2f).SetUpdate(true).SetLink(gameObject);
		}
		
		public void OnPointerUp(PointerEventData eventData)
		{
			if (!_button.interactable) return;
			
			_downTween?.Kill();
			_upTween = transform.DOScale(Vector3.one, 0.2f).SetUpdate(true).SetLink(gameObject);
		}
		
		public void OnPointerClick(PointerEventData eventData)
		{
			if(!_button.interactable) return;

			AudioManager.Instance.PlayAudio(AudioName.ButtonClick);
		}
	}
}