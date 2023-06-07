using UdonSharp;
using UnityEngine;

namespace A320VAU.FWS
{
    public partial class FWSWarningData : UdonSharpBehaviour
    {
        public FWSWarningMessageData FLAPS_NOT_IN_TAKEOFF_CONFIG;
        public FWSWarningMessageData PARK_BRAKE_ON;

        public void MonitorConfig()
        {
            if (Mathf.Approximately(FWS.equipmentData.ThrottleLevelerL, 1) &&
                Mathf.Approximately(FWS.equipmentData.ThrottleLevelerR, 1)&& 
                FWS.saccAirVehicle.Taxiing)
            {
                setWarningMessageVisableValue(ref FLAPS_NOT_IN_TAKEOFF_CONFIG.IsVisable, !(FWS.equipmentData.Flap.detentIndex == 1 | FWS.equipmentData.Flap.detentIndex == 2), true);
                setWarningMessageVisableValue(ref PARK_BRAKE_ON.IsVisable, FWS.equipmentData.Brake.ParkBreakSet, true);
            }
            else
            {
                setWarningMessageVisableValue(ref FLAPS_NOT_IN_TAKEOFF_CONFIG.IsVisable, false, true);
                setWarningMessageVisableValue(ref PARK_BRAKE_ON.IsVisable, false, true);
            }
        }
    }
}