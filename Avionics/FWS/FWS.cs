
using System;
using A320VAU.Brake;
using A320VAU.ECAM;
using A320VAU.Avionics;
using EsnyaSFAddons.DFUNC;
using EsnyaSFAddons.SFEXT;
using SaccFlightAndVehicles;
using UdonSharp;
using UnityEngine;
using A320VAU.DFUNC;
using VRC.SDKBase;
using VRC.Udon;
using YuxiFlightInstruments.BasicFlightData;
using YuxiFlightInstruments.Navigation;
using A320VAU.SFEXT;

namespace A320VAU.FWS
{
    public class FWS : UdonSharpBehaviour
    {
        [HideInInspector]
        public FWSWarningMessageData[] FWSWarningMessageDatas;
        [HideInInspector]
        public FWSWarningData FWSWarningData;
        public ECAMController ECAMController;
        [HideInInspector]
        public AudioSource AudioSource;

        public AudioClip Caution;

        public SaccAirVehicle SaccAirVehicle;
        public SaccEntity SaccEntity;

        #region Aircraft Systems
        [Header("Aircraft Systems")]
        public YFI_FlightDataInterface FlightData;
        public SFEXT_a320_AdvancedEngine Engine1;
        public SFEXT_a320_AdvancedEngine Engine2;
        public SFEXT_AuxiliaryPowerUnit APU;
        public DFUNC_AdvancedFlaps Flaps;

        public SFEXT_a320_AdvancedGear LeftLadingGear;
        public SFEXT_a320_AdvancedGear RightLadingGear;
        public SFEXT_a320_AdvancedGear FrontLadingGear;
        public DFUNC_a320_Brake Brake;

        public DFUNC_a320_LandingLight LandingLight;
        public DFUNC_Canopy Canopy;

        public YFI_NavigationReceiver NavigationReceiver1;
        public YFI_NavigationReceiver NavigationReceiver2;
        public GPWS_OWML GPWS;

        public DFUNC_ElevatorTrim ElevatorTrim;
        #endregion

        private bool _hasWarningVisableChange = false;
        private string[] _activeWarnings = new string[0];

        private void Start()
        {
            FWSWarningMessageDatas = GetComponentsInChildren<FWSWarningMessageData>();
            FWSWarningData = GetComponentInChildren<FWSWarningData>();
            AudioSource = GetComponent<AudioSource>();
        }

        private void LateUpdate()
        {
            _hasWarningVisableChange = FWSWarningData.Monitor(this);

            if (!_hasWarningVisableChange) return;

            var newActiveWarnings = new string[_activeWarnings.Length];
            var hasMatserWarning = false;
            var hasCaution = false;
            foreach (var memo in FWSWarningMessageDatas)
            {
                if (memo.IsVisable)
                {
                    addItem(newActiveWarnings, memo.Id);
                    if (memo.Type == WarningType.Primary && !contains(_activeWarnings, memo.Id))
                    {
                        switch (memo.Level)
                        {
                            case WarningLevel.Immediate:
                                hasMatserWarning = true;
                                break;
                            case WarningLevel.None:
                                // doing nothing
                                break;
                            default:
                                hasCaution = true;
                                break;
                        }
                    }
                }
            }

            if (hasMatserWarning)
            {
                AudioSource.Play();
            }
            else
            {
                AudioSource.Stop();
                if (hasCaution)
                {
                    AudioSource.PlayOneShot(Caution);
                }
            }

            _activeWarnings = new string[0];
            ECAMController.SendCustomEvent("UpdateMemo");
        }

        private string[] addItem(string[] array, string item)
        {
            var newArray = new string[array.Length + 1];
            newArray[array.Length] = item;
            return newArray;
        }

        private bool contains(string[] array, string item)
        {
            foreach (var temp in array)
            {
                if (temp == item) return true;
            }

            return false;
        }
    }
}