
using System;
using A320VAU.Brake;
using A320VAU.ECAM;
using EsnyaSFAddons.DFUNC;
using EsnyaSFAddons.SFEXT;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using YuxiFlightInstruments.BasicFlightData;
using YuxiFlightInstruments.Navigation;

namespace A320VAU.FWS
{
    public class FWS : UdonSharpBehaviour
    {
        public FWSWarningMessageData[] FWSWarningMessageDatas;
        public FWSWarningData FWSWarningData;
        public ECAMController ECAMController;

        #region Aircraft Systems
        [Header("Aircraft Systems")]
        public YFI_FlightDataInterface FlightData;
        public SFEXT_AdvancedEngine Engine1;
        public SFEXT_AdvancedEngine Engine2;
        public SFEXT_AuxiliaryPowerUnit APU;
        public DFUNC_AdvancedFlaps Flaps;

        public SFEXT_AdvancedGear LeftLadingGear;
        public SFEXT_AdvancedGear RightLadingGear;
        public SFEXT_AdvancedGear FrontLadingGear;
        public DFUNC_a320_Brake Brake;

        public DFUNC_LandingLight LandingLight;

        public YFI_NavigationReceiver NavigationReceiver1;
        public YFI_NavigationReceiver NavigationReceiver2;

        public DFUNC_ElevatorTrim ElevatorTrim;
        #endregion

        private bool _hasWarningVisableChange = false;
        private void LateUpdate()
        {
            _hasWarningVisableChange = FWSWarningData.Monitor(this);

            if (_hasWarningVisableChange) ECAMController.SendCustomEvent("UpdateMemo");
        }
    }
}