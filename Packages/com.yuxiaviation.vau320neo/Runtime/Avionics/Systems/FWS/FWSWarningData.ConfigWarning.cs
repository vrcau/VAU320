using UdonSharp;
using UnityEngine;

namespace A320VAU.FWS
{
    public partial class FWSWarningData : UdonSharpBehaviour
    {
        private FWSWarningMessageData FLAPS_NOT_IN_TAKEOFF_CONFIG;
        private FWSWarningMessageData PARK_BRAKE_ON;

        private void SetupConfig()
        {
            FLAPS_NOT_IN_TAKEOFF_CONFIG = GetWarningMessageData(nameof(FLAPS_NOT_IN_TAKEOFF_CONFIG));
            PARK_BRAKE_ON = GetWarningMessageData(nameof(PARK_BRAKE_ON));
        }

        private void MonitorConfig()
        {
            if (Mathf.Approximately(FWS.equipmentData.ThrottleLevelerL, 1) &&
                Mathf.Approximately(FWS.equipmentData.ThrottleLevelerR, 1)&& 
                FWS.saccAirVehicle.Taxiing)
            {
                SetWarnVisible(ref FLAPS_NOT_IN_TAKEOFF_CONFIG.isVisable,
                    !(FWS.equipmentData.Flap.detentIndex == 1 || FWS.equipmentData.Flap.detentIndex == 2), true);
                SetWarnVisible(ref PARK_BRAKE_ON.isVisable, FWS.equipmentData.Brake.ParkBreakSet, true);
            }
            else
            {
                SetWarnVisible(ref FLAPS_NOT_IN_TAKEOFF_CONFIG.isVisable, false, true);
                SetWarnVisible(ref PARK_BRAKE_ON.isVisable, false, true);
            }
        }
    }
}