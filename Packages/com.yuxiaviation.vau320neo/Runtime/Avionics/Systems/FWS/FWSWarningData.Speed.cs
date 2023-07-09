﻿namespace A320VAU.FWS {
    public partial class FWSWarningData {
        private FWSWarningMessageData OVERSPEED;
        private int VLE = 280;

        private int VMO = 350;

        private void SetupSpeed() {
            OVERSPEED = GetWarningMessageData(nameof(OVERSPEED));
        }

        private void MonitorSpeed() {
            var VFE = (int)FWS.equipmentData.flapTargetSpeedLimit;

            if (VFE < (int)FWS.equipmentData.flapCurrentSpeedLimit)
                VFE = (int)FWS.equipmentData.flapCurrentSpeedLimit;

            var VMAX = VMO;
            if (VFE < VMAX)
                VMAX = (int)FWS.equipmentData.flapTargetSpeedLimit;

            if (FWS.equipmentData.IsGearsDown && VLE < VMAX)
                VMAX = VLE;

            SetWarnVisible(ref OVERSPEED.isVisable, FWS.flightData.groundSpeed > VMAX + 4f, true);
            if (OVERSPEED.isVisable) {
                SetWarnVisible(ref OVERSPEED.MessageLine[0].isMessageVisible, FWS.flightData.groundSpeed > VMO);
                SetWarnVisible(ref OVERSPEED.MessageLine[1].isMessageVisible, FWS.flightData.groundSpeed > VLE);
                SetWarnVisible(ref OVERSPEED.MessageLine[2].isMessageVisible, FWS.flightData.groundSpeed > VFE);

                OVERSPEED.MessageLine[2].MessageText = $" -VFE................{VFE}";
            }
        }
    }
}