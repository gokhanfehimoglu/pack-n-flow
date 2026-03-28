using System;
using UnityEngine;

namespace PackNFlow
{
    [Serializable]
    public struct LevelColor
    {
        public int Id;
        public Color32 Color;
        public int UnitColorToken;

        public LevelColor(int id, Color32 color, int unitColorToken)
        {
            Id = id;
            Color = color;
            UnitColorToken = unitColorToken;
        }
    }
}
