using A320VAU.Common;
using A320VAU.ECAM;
using Avionics.Systems.Common;
using UdonSharp;
using UnityEngine;

namespace A320VAU.FWS {
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class FWS : UdonSharpBehaviour {
    #region Aircraft Systems

        [HideInInspector] public ADIRU.ADIRU adiru;
        [HideInInspector] public AircraftSystemData equipmentData;
        [HideInInspector] public RadioAltimeter.RadioAltimeter radioAltimeter;

    #endregion

    #region FWS Data (OEB) (Checklist and warnings)

        [HideInInspector] public FWSWarningMessageData[] fwsWarningMessageDatas;
        private FWSWarningData _fwsWarningData;

    #endregion

    #region ECAM and Warning Light/Audio

        public Animator cockpitAnimator;

        private readonly int MASTER_WARNING_HASH = Animator.StringToHash("IsMasterWarning");
        private readonly int MASTER_CAUTION_HASH = Animator.StringToHash("IsMasterCaution");

        [Header("ECAM and warning Light/Audio")]
        public ECAMDisplay ECAMController;

        [HideInInspector] public AudioClip Caution; // looking for master warning? check out the FWS GameObject

    #endregion

    #region FWS Warning

        [HideInInspector] public bool _hasWarningVisibleChange;
        [HideInInspector] public bool _hasWarningDataVisibleChange;

    #endregion

        public AudioSource audioSource;

        private const float FWS_UPDATE_INTERVAL = 0.1f;
        private float _lastFwsUpdate;

        private void Start() {
            var injector = DependenciesInjector.GetInstance(this);
            adiru = injector.adiru;
            radioAltimeter = injector.radioAltimeter;

            fwsWarningMessageDatas = GetComponentsInChildren<FWSWarningMessageData>();
            _fwsWarningData = GetComponentInChildren<FWSWarningData>();
        }

        private void LateUpdate() {
            var radioAltitude = radioAltimeter.radioAltitude;

            if (radioAltimeter.isAvailable) {
                UpdateMinimumCallout(radioAltitude);
                UpdateAltitudeCallout(radioAltitude);
            }

            UpdateFWS();
        }

        private void OnEnable() {
            _lastAltitudeCalloutIndex = -1;
            _lastMinimumCalloutIndex = -1;
        }

        private void UpdateFWS() {
            if (Time.time - _lastFwsUpdate < FWS_UPDATE_INTERVAL) return;
            _lastFwsUpdate = Time.time;

            _fwsWarningData.Monitor(this); // the core of the FWS

            if (!_hasWarningVisibleChange) return; // return if there is nothing need to update

            // Get Updated Warnings and Warning Level (e.g Master Caution/Warning)
            if (_hasWarningDataVisibleChange) {
                var hasMasterWarning = false;
                var hasMasterCaution = false;

                foreach (var memo in fwsWarningMessageDatas)
                    if (memo.isVisible && memo.Type == WarningType.Primary)
                        switch (memo.Level) {
                            case WarningLevel.Immediate:
                                hasMasterWarning = true;
                                break;
                            case WarningLevel.None:
                                // doing nothing
                                break;
                            default:
                                hasMasterCaution = true;
                                break;
                        }

                if (hasMasterWarning) {
                    audioSource.Play();

                    cockpitAnimator.SetBool(MASTER_WARNING_HASH, true);
                }

                if (hasMasterCaution) {
                    cockpitAnimator.SetBool(MASTER_CAUTION_HASH, true);

                    audioSource.PlayOneShot(Caution);
                }

                if (!hasMasterCaution && !hasMasterWarning) {
                    audioSource.Stop();

                    cockpitAnimator.SetBool(MASTER_WARNING_HASH, false);
                    cockpitAnimator.SetBool(MASTER_CAUTION_HASH, false);
                }
            }

            ECAMController.UpdateMemo();
        }

        // ReSharper disable once UnusedMember.Global
        public void ResetWarning() {
            audioSource.Stop();

            cockpitAnimator.SetBool(MASTER_WARNING_HASH, false);
            cockpitAnimator.SetBool(MASTER_CAUTION_HASH, false);
        }

        private static bool Contains(string[] array, string item) {
            foreach (var temp in array)
                if (temp == item)
                    return true;

            return false;
        }

    #region AltitudeCallout

        [Header("Altitude Callout")]
        public float[] altitudeCalloutIndexs = {
            2500f, 2000f, 1000f, 500f, 400f, 300f, 200f, 100f, 50f, 40f, 30f, 20f, 10f, 5f
        };

        public AudioClip[] altitudeCallouts = new AudioClip[14];

        public AudioClip retardCallout;
        public AudioClip hundredAboveCallout;
        public AudioClip minimumCallout;

        public float decisionHeight = 200f;
        // public float MinimumDescentAltitude = 200f;

        private int _lastAltitudeCalloutIndex = -1;
        private int _lastMinimumCalloutIndex = -1;

    #endregion

    #region Mininmum Callout

        private void UpdateMinimumCallout(float radioAltitude) {
            var minimumCalloutIndex = GetMinimumCalloutIndex(radioAltitude);

            if (_lastMinimumCalloutIndex != -1 && minimumCalloutIndex > _lastMinimumCalloutIndex) {
                switch (minimumCalloutIndex) {
                    // HUNDRED ABOVE
                    case 1:
                        SendCustomEventDelayedSeconds(nameof(CalloutHundredAbove), 1);
                        break;
                    // MINIMUM
                    case 2:
                        SendCustomEventDelayedSeconds(nameof(CalloutMinimum), 1);
                        break;
                }
            }

            _lastMinimumCalloutIndex = minimumCalloutIndex;
        }

        public void CalloutHundredAbove() {
            audioSource.PlayOneShot(hundredAboveCallout);
        }

        public void CalloutMinimum() {
            audioSource.PlayOneShot(minimumCallout);
        }

        private int GetMinimumCalloutIndex(float radioAltitude) {
            if (radioAltitude < decisionHeight) return 2;
            if (radioAltitude < decisionHeight + 100f) return 1;

            return 0;
        }

    #endregion

    #region Altitude Callout

        public void CalloutRetard() {
            _isRetardCalloutActive = true;

            var radioAltitude = radioAltimeter.radioAltitude;

            if (Time.time - _lastCalloutRetard < 0.5f)
                return;

            // RETARD
            if (radioAltitude < 20f &&
                (int)equipmentData.throttleLevelerSlot < (int)ThrottleLevelerSlot.IDLE) {
                audioSource.PlayOneShot(retardCallout);

                SendCustomEventDelayedSeconds(nameof(CalloutRetard), 1);
                _lastCalloutRetard = Time.time;
            }
            else {
                _isRetardCalloutActive = false;
            }
        }

        private float _lastCallout;

        private bool _isRetardCalloutActive;

        private float _lastCalloutRetard;

        private void UpdateAltitudeCallout(float radioAltitude) {
            var altitudeCalloutIndex = GetAltitudeCalloutIndex(radioAltitude);

            if (_lastAltitudeCalloutIndex != -1 && altitudeCalloutIndex > _lastAltitudeCalloutIndex) {
                audioSource.PlayOneShot(altitudeCallouts[altitudeCalloutIndex]);

                // RETARD
                if (!_isRetardCalloutActive && altitudeCalloutIndex >= 10) {
                    SendCustomEventDelayedSeconds(nameof(CalloutRetard), 1);
                }

                _lastCallout = Time.time;
            }
            else if (altitudeCalloutIndex != _lastAltitudeCalloutIndex) {
                _lastCallout = Time.time;
            }
            else {
                // Repeat when after 11s (>50ft) / 4s (<50ft)
                var diff = Time.time - _lastCallout;
                var lastCalloutLength = altitudeCallouts[_lastAltitudeCalloutIndex].length;
                if (!equipmentData.isAircraftGrounded) {
                    if (altitudeCalloutIndex != -1 && (
                            (radioAltitude > 50f && diff > 11f + lastCalloutLength)
                            ||
                            (radioAltitude < 50f && diff > 4f + lastCalloutLength)
                        ) && Mathf.Abs(radioAltitude - altitudeCalloutIndexs[altitudeCalloutIndex]) < 10) {
                        audioSource.PlayOneShot(altitudeCallouts[altitudeCalloutIndex]);
                        _lastCallout = Time.time;
                    }
                }
                else {
                    _lastCallout = Time.time;
                }
            }

            _lastAltitudeCalloutIndex = altitudeCalloutIndex;
        }

        private int GetAltitudeCalloutIndex(float radioAltitude) {
            for (var index = altitudeCalloutIndexs.Length - 1; index != -1; index--)
                if (radioAltitude < altitudeCalloutIndexs[index])
                    return index;

            return -1;
        }

    #endregion
    }
}