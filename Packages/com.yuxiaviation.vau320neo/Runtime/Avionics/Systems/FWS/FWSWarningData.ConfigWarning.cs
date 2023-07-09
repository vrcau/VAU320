using UnityEngine;

namespace A320VAU.FWS {
    public partial class FWSWarningData {
        private FWSWarningMessageData FLAPS_NOT_IN_TAKEOFF_CONFIG;
        private FWSWarningMessageData PARK_BRAKE_ON;

        private void SetupConfig() {
            FLAPS_NOT_IN_TAKEOFF_CONFIG = GetWarningMessageData(nameof(FLAPS_NOT_IN_TAKEOFF_CONFIG));
            PARK_BRAKE_ON = GetWarningMessageData(nameof(PARK_BRAKE_ON));
        }

        private void MonitorConfig() {
            if ((Mathf.Approximately(FWS.equipmentData.engine1ThrottleLeveler, 1f) ||
                 Mathf.Approximately(FWS.equipmentData.engine2ThrottleLeveler, 1f)) &&
                FWS.saccAirVehicle.Taxiing) {
                SetWarnVisible(ref FLAPS_NOT_IN_TAKEOFF_CONFIG.isVisable,
                    !(FWS.equipmentData.flapCurrentIndex == 1 || FWS.equipmentData.flapCurrentIndex == 2), true);
                SetWarnVisible(ref PARK_BRAKE_ON.isVisable, FWS.equipmentData.isParkBreakSet, true);
            }
            else {
                SetWarnVisible(ref FLAPS_NOT_IN_TAKEOFF_CONFIG.isVisable, false, true);
                SetWarnVisible(ref PARK_BRAKE_ON.isVisable, false, true);
            }
        }
    }
}