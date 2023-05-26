
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
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class FWS : UdonSharpBehaviour
    {
        #region FWS Data (OEB) (Checklist and warnings)
        [HideInInspector]
        public FWSWarningMessageData[] FWSWarningMessageDatas;
        [HideInInspector]
        public FWSWarningData FWSWarningData;
        #endregion

        #region ECAM and Warning Light/Audio
        [Header("ECAM and warning Light/Audio")]
        public ECAMDisplay ECAMController;

        [HideInInspector]
        //private AudioSource AudioSource;

        public AudioClip Caution; // looking for master warning? check out the FWS gameobject

        public GameObject MasterWarningLightCAPT;
        public GameObject MasterWarningLightFO;
        public GameObject MasterCautionLightCAPT;
        public GameObject MasterCautionLightFO;
        #endregion

        #region Aircraft Systems
        [Header("Aircraft Systems")]
        public SaccAirVehicle SaccAirVehicle;
        public SaccEntity SaccEntity;
        public YFI_FlightDataInterface FlightData;
        public ECAMDataInterface EquipmentData;
        public GPWS_OWML GPWS; //as sound source

        
        #endregion

        #region FWS Warning
        [HideInInspector]
        public bool _hasWarningVisableChange = false;
        [HideInInspector]
        public bool _hasWarningDataVisableChange = false;
        [HideInInspector]
        private string[] _activeWarnings = new string[0];
        #endregion

        #region AltitudeCallout
        [Header("Altitude Callout")]
        public float[] AltitudeCalloutIndexs = new float[] {
            2500f, 2000f, 1000f, 500f, 400f, 300f, 200f, 100f, 50f, 40f, 30f, 20f, 10f, 5f
        };

        public AudioClip[] AltitudeCallouts = new AudioClip[14];

        public AudioClip RetardCallout;
        public AudioClip HundredAboveCallout;
        public AudioClip MininmumCallout;

        public float DecisionHeight = 200f;
        // public float MinimumDescentAltitude = 200f;

        private int _lastAltitdueCalloutIndex = -1;
        private int _lastMininmumCalloutIndex = -1;
        private DateTime _lastCallout = DateTime.Now;
        #endregion

        private void Start()
        {
            FWSWarningMessageDatas = GetComponentsInChildren<FWSWarningMessageData>();
            FWSWarningData = GetComponentInChildren<FWSWarningData>();
            //AudioSource = GetComponent<AudioSource>();
        }

        private void LateUpdate()
        {
            var radioAltitude = (float)GPWS.GetProgramVariable("radioAltitude");

            UpdateMininmumCallout(radioAltitude);
            UpdateAltitudeCallout(radioAltitude);
            UpdateFWS();
        }

        private void OnEnable()
        {
            _lastAltitdueCalloutIndex = -1;
            _lastMininmumCalloutIndex = -1;
        }

        #region Mininmum Callout
        private void UpdateMininmumCallout(float radioAltitude)
        {
            var mininmumCalloutIndex = GetMinunmumCalloutIndex(radioAltitude);

            if (_lastMininmumCalloutIndex != -1 && mininmumCalloutIndex > _lastMininmumCalloutIndex)
            {
                // HUNDRED ABOVE
                if (mininmumCalloutIndex == 0)
                {
                    GPWS.PlayOneShot(HundredAboveCallout);
                }

                // MINIMUM
                if (mininmumCalloutIndex == 1)
                {
                    GPWS.PlayOneShot(MininmumCallout);
                }
            }

            _lastMininmumCalloutIndex = mininmumCalloutIndex;
        }

        private int GetMinunmumCalloutIndex(float radioAltitude)
        {
            if (radioAltitude < DecisionHeight) return 1;
            if (radioAltitude < (DecisionHeight + 100f)) return 0;

            return -1;
        }
        #endregion

        #region Altitude Callout
        private void UpdateAltitudeCallout(float radioAltitude)
        {
            var altitudeCalloutIndex = GetAltitudeCalloutIndex(radioAltitude);

            if (_lastAltitdueCalloutIndex != -1 && altitudeCalloutIndex > _lastAltitdueCalloutIndex)
            {
                // RETARD
                if (altitudeCalloutIndex == 12)
                {
                    GPWS.PlayOneShot(RetardCallout);
                }
                else
                {
                    GPWS.PlayOneShot(AltitudeCallouts[altitudeCalloutIndex]);
                }

                _lastCallout = DateTime.Now;
            }
            else if (altitudeCalloutIndex != _lastAltitdueCalloutIndex)
            {
                _lastCallout = DateTime.Now;
            }
            else
            {
                // Repeat when after 11s (>50ft) / 4s (<50ft)
                var diff = DateTime.Now - _lastCallout;
                if (altitudeCalloutIndex != -1 &&
                (radioAltitude > 50f && diff.TotalSeconds > 11) | (radioAltitude < 50f && diff.TotalMilliseconds < 4) &&
                Mathf.Abs(radioAltitude - AltitudeCalloutIndexs[altitudeCalloutIndex]) < 10)
                {
                    GPWS.PlayOneShot(AltitudeCallouts[altitudeCalloutIndex]);
                    _lastCallout = DateTime.Now;
                }
            }

            _lastAltitdueCalloutIndex = altitudeCalloutIndex;
        }

        private int GetAltitudeCalloutIndex(float radioAltitude)
        {
            for (int index = AltitudeCalloutIndexs.Length - 1; index != -1; index--)
            {
                if (radioAltitude < AltitudeCalloutIndexs[index])
                    return index;
            }

            return -1;
        }
        #endregion

        private void UpdateFWS()
        {
            var _hasMatserWarning = false;
            var _hasMatserCaution = false;
            FWSWarningData.Monitor(this); // the core of the FWS

            if (!_hasWarningVisableChange) return; // return if there is nothing need to update

            #region Get Updated Warnings and Wanring Level (e.g Master Caution/Warning)
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
            #endregion

            #region Warning Light & Sound
            if (_hasMatserWarning)
            {
                GPWS.audioSource.Play();
                
                MasterWarningLightCAPT.SetActive(true);
                MasterWarningLightFO.SetActive(true);
                MasterCautionLightCAPT.SetActive(true);
                MasterCautionLightFO.SetActive(true);
            }
            else
            {
                GPWS.audioSource.Stop();
                MasterWarningLightCAPT.SetActive(false);
                MasterWarningLightFO.SetActive(false);
                if (_hasMatserCaution)
                {
                    MasterCautionLightCAPT.SetActive(true);
                    MasterCautionLightFO.SetActive(true);
                    GPWS.PlayOneShot(Caution);
                }
                else
                {
                    MasterCautionLightCAPT.SetActive(false);
                    MasterCautionLightFO.SetActive(false);
                }
            }
            #endregion

            _activeWarnings = new string[0];
            ECAMController.UpdateMemo();
            //ECAMController.SendCustomEvent("UpdateMemo");
        }

        public void CancleWarning()
        {
            GPWS.audioSource.Stop();
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