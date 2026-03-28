using System;
using DG.Tweening;
using UnityEngine;

namespace PackNFlow
{
    public class PixelBlock : MonoBehaviour
    {
        [SerializeField] private Renderer blockRenderer;

        public PixelBlockData Data { get; private set; }
        public bool IsMarkedForRemoval { get; private set; }
        public bool IsBeingPulled { get; private set; }

        private static readonly int BaseColorProp = Shader.PropertyToID("_BaseColor");
        private MaterialPropertyBlock _mpb;

        public event Action<PixelBlock> OnPulled;

        public void Initialize(PixelBlockData data, float cellScale)
        {
            Data = data;
            transform.localScale = cellScale * Vector3.one;
            ApplyColor();
        }

        private void OnDestroy()
        {
            DOTween.Kill(transform);
            OnPulled = null;
        }

        private void OnDisable()
        {
            DOTween.Kill(transform);
            OnPulled = null;
        }

        public void PullToward(Transform target, float duration, Action onArrived = null)
        {
            if (IsMarkedForRemoval || IsBeingPulled) return;

            IsBeingPulled = true;
            OnPulled?.Invoke(this);

            DOTween.Kill(transform);

            var startScale = transform.localScale;
            var startRot = transform.rotation;
            var progress = 0f;
            var startPos = transform.position;

            DOTween.To(() => progress, x => progress = x, 1f, duration)
                .SetEase(Ease.InQuad)
                .SetLink(gameObject)
                .OnUpdate(() =>
                {
                    var dest = target.position;
                    transform.position = Vector3.Lerp(startPos, dest, progress);
                    // transform.localScale = Vector3.Lerp(startScale, Vector3.zero, progress);
                    transform.rotation = Quaternion.Slerp(startRot, target.rotation, progress);
                })
                .OnComplete(() =>
                {
                    onArrived?.Invoke();
                    IsMarkedForRemoval = true;
                    IsBeingPulled = false;
                    gameObject.SetActive(false);
                });
        }

        public void MarkForRemoval()
        {
            if (IsMarkedForRemoval) return;

            IsMarkedForRemoval = true;
            PlayDisappearAnimation();
            OnPulled?.Invoke(this);
        }

        public void ApplyColorOverride(Color32 color)
        {
            if (_mpb == null) _mpb = new MaterialPropertyBlock();

            _mpb.SetColor(BaseColorProp, color);
            blockRenderer.SetPropertyBlock(_mpb);
        }

        private void ApplyColor()
        {
            var levelData = LevelDirector.Instance?.ActiveLevelData;
            if (levelData == null) return;

            if (_mpb == null) _mpb = new MaterialPropertyBlock();

            _mpb.SetColor(BaseColorProp, levelData.GetColorById(Data.ColorId));
            blockRenderer.SetPropertyBlock(_mpb);
        }

        private void PlayDisappearAnimation()
        {
            var baseScale = transform.localScale;
            DOTween.Kill(transform);

            transform.DOShakeScale(0.3f, baseScale * 0.2f, 20, 90f, true, ShakeRandomnessMode.Harmonic)
                .OnComplete(() =>
                {
                    transform.DOScale(0f, 0.1f)
                        .OnComplete(() => gameObject.SetActive(false));
                })
                .SetLink(gameObject);
        }
    }
}
