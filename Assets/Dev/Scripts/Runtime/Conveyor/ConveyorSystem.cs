using System;
using UnityEngine;

namespace PackNFlow
{
    public class ConveyorSystem : MonoBehaviour
    {
        public Bounds? Bounds { get; private set; }

        public void Prepare(int carriageCount) { }

        public bool TryGetReadyCarriage(out ConveyorCarriage carriage)
        {
            carriage = null;
            return false;
        }

        public void DispatchCarriage(ConveyorCarriage carriage) { }
        public void ReleaseCarriageForUnit(Unit unit) { }
        public void PlayCapacityWarning() { }
    }

    public class ConveyorCarriage : MonoBehaviour
    {
        public void BeginMovement() { }
    }
}
