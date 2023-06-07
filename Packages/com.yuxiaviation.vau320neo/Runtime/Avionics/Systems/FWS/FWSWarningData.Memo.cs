using UdonSharp;

namespace A320VAU.FWS
{
    public partial class FWSWarningData : UdonSharpBehaviour
    {
        public FWSWarningMessageData APU_BLEED;
        public FWSWarningMessageData PARK_BRK;
        public FWSWarningMessageData APU_AVAIL;

        public void MonitorMemo()
        {
            setWarningMessageVisableValue(ref APU_BLEED.IsVisable, FWS.equipmentData.IsAPURunning);
            setWarningMessageVisableValue(ref APU_AVAIL.IsVisable, FWS.equipmentData.IsAPURunning);
            setWarningMessageVisableValue(ref PARK_BRK.IsVisable, FWS.equipmentData.Brake.ParkBreakSet);
        }
    }
}