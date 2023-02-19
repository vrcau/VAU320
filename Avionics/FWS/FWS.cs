
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
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
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

        public GameObject MasterWarningLightCAPT;
        public GameObject MasterWarningLightFO;
        public GameObject MasterCautionLightCAPT;
        public GameObject MasterCautionLightFO;

        public bool _hasWarningVisableChange = false;
        public bool _hasWarningDataVisableChange = false;
        private string[] _activeWarnings = new string[0];

        private void Start()
        {
            FWSWarningMessageDatas = GetComponentsInChildren<FWSWarningMessageData>();
            FWSWarningData = GetComponentInChildren<FWSWarningData>();
            AudioSource = GetComponent<AudioSource>();
        }

        private void LateUpdate()
        {
            var _hasMatserWarning = false;
            var _hasMatserCaution = false;
            FWSWarningData.Monitor(this);

            if (!_hasWarningVisableChange) return;

            var newActiveWarnings = new string[_activeWarnings.Length];
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
                                _hasMatserWarning = true;
                                break;
                            case WarningLevel.None:
                                // doing nothing
                                break;
                            default:
                                _hasMatserCaution = true;
                                break;
                        }
                    }
                }
            }

            if (_hasMatserWarning)
            {
                AudioSource.Play();
                MasterWarningLightCAPT.SetActive(true);
                MasterWarningLightFO.SetActive(true);
                MasterCautionLightCAPT.SetActive(true);
                MasterCautionLightFO.SetActive(true);
            }
            else
            {
                AudioSource.Stop();
                MasterWarningLightCAPT.SetActive(false);
                MasterWarningLightFO.SetActive(false);
                if (_hasMatserCaution)
                {
                    MasterCautionLightCAPT.SetActive(true);
                    MasterCautionLightFO.SetActive(true);
                    AudioSource.PlayOneShot(Caution);
                }
                else
                {
                    MasterCautionLightCAPT.SetActive(false);
                    MasterCautionLightFO.SetActive(false);
                }
            }

            _activeWarnings = new string[0];
            ECAMController.SendCustomEvent("UpdateMemo");
        }

        public void CancleWarning()
        {
            AudioSource.Stop();
            MasterWarningLightCAPT.SetActive(false);
            MasterWarningLightFO.SetActive(false);
            MasterCautionLightCAPT.SetActive(false);
            MasterCautionLightFO.SetActive(false);
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