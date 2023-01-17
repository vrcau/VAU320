using UdonSharp;

namespace A320VAU.FWS
{
    public partial class FWSWarningData : UdonSharpBehaviour
    {
        public FWSWarningMessageData APU_BLEED;
        public FWSWarningMessageData PARK_BRK;

        public void MonitorMemo() {
            APU_BLEED.IsVisable = FWS.APU.run;
            PARK_BRK.IsVisable = FWS.Brake.ParkBreakSet;

            _hasWarningVisableChange = true;
        }
    }
}