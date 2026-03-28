using System;
using System.Collections.Generic;
using UnityEngine;

namespace PackNFlow
{
    public class PixelBlockController : MonoBehaviour
    {
        public event Action OnAllBlocksCleared;

        public void Initialize(Bounds conveyorBounds) { }
        public void Prepare(Bounds conveyorBounds) { }

        public bool TryFindBlockForUnit(Unit unit, out PixelBlock block, out Edge edge)
        {
            block = null;
            edge = Edge.South;
            return false;
        }
    }
}
