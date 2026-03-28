using System;
using UnityEngine;

namespace PackNFlow
{
    [Serializable]
    public struct PixelBlockData
    {
        public Vector2Int Coordinates;
        public int ColorId;

        public PixelBlockData(Vector2Int coordinates, int colorId)
        {
            Coordinates = coordinates;
            ColorId = colorId;
        }
    }
}
