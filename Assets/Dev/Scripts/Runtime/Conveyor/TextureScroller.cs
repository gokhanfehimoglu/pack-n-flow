using UnityEngine;

namespace PackNFlow
{
    public class TextureScroller : MonoBehaviour
    {
        [SerializeField] private Renderer scrollRenderer;
        [SerializeField] private float scrollX = 0.5f;
        [SerializeField] private float scrollY = 0.5f;
        [SerializeField] private bool scrolling;

        private Material _mat;
        private float _scrollTime;

        private void Start()
        {
            _mat = scrollRenderer.sharedMaterial;
        }

        private void Update()
        {
            if (!scrolling) return;

            _scrollTime += Time.deltaTime;
            var offsetX = _scrollTime * scrollX;
            var offsetY = _scrollTime * scrollY;
            _mat.mainTextureOffset = new Vector2(offsetX, offsetY);
        }
    }
}