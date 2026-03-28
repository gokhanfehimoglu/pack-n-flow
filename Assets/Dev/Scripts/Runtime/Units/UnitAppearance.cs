using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace PackNFlow
{
    public class UnitAppearance : MonoBehaviour
    {
        [SerializeField] private TextMeshPro capacityLabel;
        [SerializeField] private List<Renderer> unitRenderers;
        [SerializeField] private List<Renderer> ropeRenderers;
        [SerializeField] private Transform muzzle;

        public Renderer MeshRenderer => unitRenderers is { Count: > 0 } ? unitRenderers[0] : null;

        private static readonly int BaseColorProp = Shader.PropertyToID("_BaseColor");
        private static readonly int ShadowColorProp = Shader.PropertyToID("_SColor");

        private MaterialPropertyBlock _bodyMpb;
        private MaterialPropertyBlock _ropeMpb;
        private UnitData _data;

        public void ApplyConfig(UnitData data)
        {
            _data = data;
            ApplyColor();
            UpdateCapacityText(data.PullCapacity);
            if (!data.IsConcealed)
                capacityLabel.alpha = 0.5f;
        }

        public void ApplyConcealed()
        {
            _bodyMpb = new MaterialPropertyBlock();
            _bodyMpb.SetColor(BaseColorProp, Color.white);
            _bodyMpb.SetColor(ShadowColorProp, Color.gray);

            foreach (var r in unitRenderers)
            {
                r.material = UnitAppearanceConfig.Instance.concealed;
                r.SetPropertyBlock(_bodyMpb);
            }

            if (ropeRenderers is { Count: > 0 })
            {
                _ropeMpb = new MaterialPropertyBlock();
                _ropeMpb.SetColor(BaseColorProp, Color.white);
                _ropeMpb.SetColor(ShadowColorProp, Color.gray);

                var ropeMat = UnitAppearanceConfig.Instance.concealedRope != null
                    ? UnitAppearanceConfig.Instance.concealedRope
                    : UnitAppearanceConfig.Instance.concealed;

                foreach (var r in ropeRenderers)
                {
                    r.material = ropeMat;
                    r.SetPropertyBlock(_ropeMpb);
                }
            }

            capacityLabel.SetText("?");
        }

        public void ApplyColor(Color32 color)
        {
            ApplyBodyColor(color);
            ApplyRopeColor(color);
        }

        public void UpdateCapacityText(int remaining) => capacityLabel.SetText(remaining.ToString());

        public void Reveal(Color32 color)
        {
            UpdateCapacityText(_data.PullCapacity);
            ApplyColor(color);
        }

        public Transform Muzzle => muzzle;

        public void ApplyDeployableVisual(bool canDeploy)
        {
            var color = LevelDirector.Instance.ActiveLevelData.GetColorById(_data.ColorId);
            Reveal(color);
            capacityLabel.alpha = 1f;
        }

        public void ApplyEditorVisuals(LevelData levelData, UnitData data)
        {
            _data = data;
            ApplyColor(levelData.GetColorById(data.ColorId));
            UpdateCapacityText(data.PullCapacity);
            capacityLabel.alpha = 1f;
        }

        private void ApplyColor()
        {
            if (LevelDirector.Instance != null)
                ApplyColor(LevelDirector.Instance.ActiveLevelData.GetColorById(_data.ColorId));
        }

        private void ApplyBodyColor(Color32 color)
        {
            if (_bodyMpb == null) _bodyMpb = new MaterialPropertyBlock();

            var defaultMat = UnitAppearanceConfig.Instance.defaultMaterial;
            var concealedMat = UnitAppearanceConfig.Instance.concealed;

            _bodyMpb.SetColor(BaseColorProp, color);
            _bodyMpb.SetColor(ShadowColorProp, color);

            foreach (var r in unitRenderers)
            {
                r.sharedMaterial = defaultMat;
                r.SetPropertyBlock(_bodyMpb);
            }
        }

        private void ApplyRopeColor(Color32 color)
        {
            if (ropeRenderers is not { Count: > 0 }) return;

            if (_ropeMpb == null) _ropeMpb = new MaterialPropertyBlock();

            var defaultRopeMat = UnitAppearanceConfig.Instance.defaultRopeMaterial;
            var concealedMat = UnitAppearanceConfig.Instance.concealed;
            var concealedRopeMat = UnitAppearanceConfig.Instance.concealedRope;

            _ropeMpb.SetColor(BaseColorProp, color);
            _ropeMpb.SetColor(ShadowColorProp, color);

            foreach (var r in ropeRenderers)
            {
                r.sharedMaterial = defaultRopeMat;
                r.SetPropertyBlock(_ropeMpb);
            }
        }

        private void LateUpdate()
        {
            if (capacityLabel != null)
                capacityLabel.transform.rotation = Camera.main.transform.rotation;
        }
    }
}
