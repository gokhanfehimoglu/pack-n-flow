using UnityEngine;

namespace PackNFlow
{
    public class TetherLine : MonoBehaviour
    {
        [SerializeField] private MeshRenderer meshRenderer;

        private Unit _endA;
        private Unit _endB;
        private bool _active;

        private static readonly int BaseColorProp = Shader.PropertyToID("_BaseColor");

        public void Connect(Unit a, Unit b)
        {
            _endA = a;
            _endB = b;

            var levelData = LevelDirector.Instance?.ActiveLevelData;
            if (levelData == null) return;

            var blockA = new MaterialPropertyBlock();
            // var blockB = new MaterialPropertyBlock();
            blockA.SetColor(BaseColorProp, levelData.GetColorById(a.Data.ColorId));
            // blockB.SetColor(BaseColorProp, levelData.GetColorById(b.Data.ColorId));
            meshRenderer.SetPropertyBlock(blockA, 0);
            // meshRenderer.SetPropertyBlock(blockB, 1);

            _active = true;
        }

        public void Disconnect()
        {
            _active = false;
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!_active) return;

            var mid = (_endB.transform.position + _endA.transform.position) * 0.5f + Vector3.up * 0.5f;
            transform.position = mid;

            float dist = Vector3.Distance(_endA.transform.position, _endB.transform.position);
            transform.localScale = new Vector3(0.5f, 0.5f, dist);
            transform.forward = (_endA.transform.position - _endB.transform.position).normalized;
        }
    }
}
