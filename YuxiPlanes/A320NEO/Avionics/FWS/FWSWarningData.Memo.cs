using UdonSharp;

namespace A320VAU.FWS
{
    public partial class FWSWarningData : UdonSharpBehaviour
    {
        public FWSWarningMessageData APU_BLEED;
        public FWSWarningMessageData PARK_BRK;
        public FWSWarningMessageData APU_AVAIL;

        public void MonitorMemo() {
            APU_BLEED.IsVisable = FWS.APU.started;
            APU_AVAIL.IsVisable = FWS.APU.started;
            PARK_BRK.IsVisable = FWS.Brake.ParkBreakSet;

            _hasWarningVisableChange = true;
        }
    }
}