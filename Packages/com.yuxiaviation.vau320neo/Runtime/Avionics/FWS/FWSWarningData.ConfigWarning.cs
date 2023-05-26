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
            if (Mathf.Approximately(FWS.EquipmentData.ThrottleLevelerL, 1) &&
                Mathf.Approximately(FWS.EquipmentData.ThrottleLevelerR, 1)&& 
                FWS.SaccAirVehicle.Taxiing)
            {
                setWarningMessageVisableValue(ref FLAPS_NOT_IN_TAKEOFF_CONFIG.IsVisable, !(FWS.EquipmentData.Flap.detentIndex == 1 | FWS.EquipmentData.Flap.detentIndex == 2), true);
                setWarningMessageVisableValue(ref PARK_BRAKE_ON.IsVisable, FWS.EquipmentData.Brake.ParkBreakSet, true);
            }
            else
            {
                setWarningMessageVisableValue(ref FLAPS_NOT_IN_TAKEOFF_CONFIG.IsVisable, false, true);
                setWarningMessageVisableValue(ref PARK_BRAKE_ON.IsVisable, false, true);
            }
        }
    }
}