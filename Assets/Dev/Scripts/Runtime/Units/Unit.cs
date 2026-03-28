using System;
using System.Collections.Generic;
using UnityEngine;

namespace PackNFlow
{
    public class Unit : MonoBehaviour
    {
        public bool IsScanReady { get; set; }
        public bool IsCapacityDepleted { get; set; }
        public bool IsDeployable { get; set; }

        public event Action<Unit, ConveyorCarriage> OnBoardingCompleted;

        public void BoardTheCarriage(ConveyorCarriage carriage) { }
        public void LeaveConveyor() { }
    }
}
