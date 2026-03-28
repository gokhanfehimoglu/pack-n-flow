using System;
using UnityEngine;

namespace PackNFlow
{
    [Serializable]
    public struct UnitData
    {
        public int Id;
        public int PullCapacity;
        public int ColorId;
        public int TetheredUnitId;
        public Vector2Int Coordinates;
        public bool IsConcealed;

        public UnitData(int id, int pullCapacity, int colorId, int tetheredUnitId,
            Vector2Int coordinates, bool isConcealed)
        {
            Id = id;
            PullCapacity = pullCapacity;
            ColorId = colorId;
            TetheredUnitId = tetheredUnitId;
            Coordinates = coordinates;
            IsConcealed = isConcealed;
        }
    }
}
