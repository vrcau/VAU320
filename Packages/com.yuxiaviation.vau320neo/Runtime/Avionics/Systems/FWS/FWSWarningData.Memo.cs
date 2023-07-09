using UdonSharp;

namespace A320VAU.FWS {
    public partial class FWSWarningData {
        private FWSWarningMessageData APU_AVAIL;
        private FWSWarningMessageData APU_BLEED;
        private FWSWarningMessageData PARK_BRK;

        private void SetupMemo() {
            APU_BLEED = GetWarningMessageData(nameof(APU_BLEED));
            PARK_BRK = GetWarningMessageData(nameof(PARK_BRK));
            APU_AVAIL = GetWarningMessageData(nameof(APU_AVAIL));
        }

        private void MonitorMemo() {
            SetWarnVisible(ref APU_BLEED.isVisable, FWS.equipmentData.isAPURunning);
            // APU BLEED will replace APU AVAIL if APU BLEED is on, but we don't have "APU BLEED" simulate.
            // SetWarnVisible(ref APU_AVAIL.IsVisable, FWS.equipmentData.IsAPURunning);
            SetWarnVisible(ref PARK_BRK.isVisable, FWS.equipmentData.isParkBreakSet);
        }
    }
}