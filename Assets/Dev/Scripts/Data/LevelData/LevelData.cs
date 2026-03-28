using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PackNFlow
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "Create Level Data")]
    public class LevelData : ScriptableObject
    {
        [Title("Color Palette")]
        public Texture2D referenceTexture;
        public bool enableColorGrouping;
        public List<LevelColor> palette = new();

        [Title("Block Zone")]
        public int blockZoneWidth = 20;
        public int blockZoneHeight = 20;
        public float blockZoneCellSize = 0.5f;
        public List<PixelBlockData> blockEntries;

        [Title("Unit Area")]
        public int unitColumnCount = 3;
        public int unitColumnDepth = 40;
        public float unitGridCellSize = 1f;
        public int rackSlotCount = 5;
        public List<UnitColumnData> columnDataList;

        [Title("Conveyor")]
        public int conveyorCarriageCount = 5;

        private Dictionary<int, LevelColor> _colorLookup;
        private bool _lookupDirty = true;

        private void EnsureLookupBuilt()
        {
            if (!_lookupDirty && _colorLookup != null) return;

            _colorLookup = new Dictionary<int, LevelColor>(palette.Count);
            foreach (var entry in palette)
            {
                if (!_colorLookup.ContainsKey(entry.Id))
                    _colorLookup.Add(entry.Id, entry);
            }

            _lookupDirty = false;
        }

        public Color32 GetColorById(int colorId)
        {
            EnsureLookupBuilt();
            return _colorLookup.TryGetValue(colorId, out var entry) ? entry.Color : new Color32(255, 255, 255, 255);
        }

        public int RegisterColor(Color32 color, float tolerance = 0f)
        {
            for (int i = 0; i < palette.Count; i++)
            {
                bool match = tolerance <= 0f
                    ? ColorMath.ExactMatch(palette[i].Color, color)
                    : ColorMath.WithinTolerance(palette[i].Color, color, tolerance);

                if (match)
                    return palette[i].Id;
            }

            int newId = palette.Count > 0 ? palette[palette.Count - 1].Id + 1 : 0;
            palette.Add(new LevelColor(newId, color, newId));
            _lookupDirty = true;
            return newId;
        }

        public int RegisterColorGrouped(Color32 color, float tolerance)
        {
            for (int i = 0; i < palette.Count; i++)
            {
                if (ColorMath.ExactMatch(palette[i].Color, color))
                    return palette[i].Id;
            }

            int groupToken = -1;
            if (tolerance > 0f)
            {
                for (int i = 0; i < palette.Count; i++)
                {
                    if (ColorMath.WithinTolerance(palette[i].Color, color, tolerance))
                    {
                        groupToken = palette[i].UnitColorToken;
                        break;
                    }
                }
            }

            int newId = palette.Count > 0 ? palette[palette.Count - 1].Id + 1 : 0;
            palette.Add(new LevelColor(newId, color, groupToken >= 0 ? groupToken : newId));
            _lookupDirty = true;
            return newId;
        }

        public int ResolveUnitColorToken(int colorId)
        {
            if (!enableColorGrouping)
                return colorId;

            EnsureLookupBuilt();
            return _colorLookup.TryGetValue(colorId, out var entry) ? entry.UnitColorToken : colorId;
        }

        public void MarkPaletteDirty() => _lookupDirty = true;
    }
}
