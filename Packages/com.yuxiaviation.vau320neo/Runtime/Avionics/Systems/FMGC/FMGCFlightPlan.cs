using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace A320VAU.FMGC {
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class FMGCFlightPlan : UdonSharpBehaviour {
        public FMGC fmgc;

        public int cruiseAltitude = 2000;

        public int takeoffAirportIndex = -1;
        public int arrivalAirportIndex = -1;
    }
}
