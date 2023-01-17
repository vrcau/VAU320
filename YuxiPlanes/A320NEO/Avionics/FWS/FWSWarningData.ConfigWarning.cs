using UdonSharp;

namespace A320VAU.FWS
{
    public partial class FWSWarningData : UdonSharpBehaviour
    {
        public FWSWarningMessageData FLAPS_NOT_IN_TAKEOFF_CONFIG;
        public FWSWarningMessageData PARK_BRAKE_ON;

        public void MonitorConfig()
        {
            if (FWS.SaccAirVehicle.ThrottleInput == 1 && FWS.SaccAirVehicle.Taxiing)
            {
                FLAPS_NOT_IN_TAKEOFF_CONFIG.IsVisable = FWS.Flaps.detentIndex != 1;
                PARK_BRAKE_ON.IsVisable = FWS.Brake.ParkBreakSet;
            }
            else
            {
            FLAPS_NOT_IN_TAKEOFF_CONFIG.IsVisable = false;
            PARK_BRAKE_ON.IsVisable = false;
            }
        }
    }
}