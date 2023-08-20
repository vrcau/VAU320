using System;
using A320VAU.Common;
using A320VAU.Utils;
using SaccFlightAndVehicles;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace A320VAU.FCU {
    public class FCU : UdonSharpBehaviour {
        private DFUNC_AltHold _altHoldDFunc;

        private DFUNC_Cruise _cruiseDFunc;
        private DependenciesInjector _injector;
        
        private readonly float UPDATE_INTERVAL = UpdateIntervalUtil.GetUpdateIntervalFromFPS(5);
        private float _lastUpdate;

        private void Start() {
            _injector = DependenciesInjector.GetInstance(this);
            _cruiseDFunc = _injector.cruise;
            _altHoldDFunc = _injector.altHold;

            UpdateFCUMode();
        }

        private void LateUpdate() {
            if (!UpdateIntervalUtil.CanUpdate(ref _lastUpdate, UPDATE_INTERVAL)) return;
            
            autoPilot1Switch.SetActive(_altHoldDFunc.AltHold);
            autoThrustSwitch.SetActive(_cruiseDFunc.Cruise);
            TargetSpeed = Convert.ToInt32(_cruiseDFunc.SetSpeed * 1.9438445f);
        }

        private void UpdateFCUMode() {
            switch (FCUMode) {
                case FCUMode.HeadingVerticalSpeed:
                    TRKFPAModeIndicate1.SetActive(false);
                    TRKFPAModeIndicate2.SetActive(false);
                    HDGVSModeIndicate1.SetActive(true);
                    HDGVSModeIndicate2.SetActive(true);
                    break;
                case FCUMode.TrackFPA:
                    TRKFPAModeIndicate1.SetActive(true);
                    TRKFPAModeIndicate2.SetActive(true);
                    HDGVSModeIndicate1.SetActive(false);
                    HDGVSModeIndicate2.SetActive(false);
                    break;
            }

            UpdateSpeedWindow();
            UpdateHeadingWindow();
            UpdateAltitudeWindow();
            UpdateVerticalSpeedWindow();
        }

        private void UpdateSpeedWindow() {
            SpeedManagedIndicate.SetActive(IsSpeedManaged);
            if (IsSpeedManaged) SpeedText.text = "---";

            if (IsMachSpeed) {
                SpeedModeIndicate.SetActive(false);
                MachModeIndicate.SetActive(true);

                if (!IsSpeedManaged) SpeedText.text = TargetMach.ToString();
            }
            else {
                SpeedModeIndicate.SetActive(true);
                MachModeIndicate.SetActive(false);

                if (!IsSpeedManaged) SpeedText.text = TargetSpeed.ToString("D3");
            }
        }

        private void UpdateHeadingWindow() {
            HeadingManagedIndicate.SetActive(IsHeadingManaged);
            GPSModeIndicate.SetActive(IsHeadingManaged);
            if (IsHeadingManaged)
                HeadingText.text = "---";
            else
                HeadingText.text = TargetHeading.ToString("D3");

            switch (FCUMode) {
                case FCUMode.HeadingVerticalSpeed:
                    HeadingModeIndicate.SetActive(true);
                    TrackModeIndicate.SetActive(false);
                    break;
                case FCUMode.TrackFPA:
                    HeadingModeIndicate.SetActive(false);
                    TrackModeIndicate.SetActive(true);
                    break;
            }
        }

        private void UpdateAltitudeWindow() {
            AltitudeText.text = TargetAltitude.ToString("D5");
        }

        private void UpdateVerticalSpeedWindow() {
            VerticalSpeedManagedIndicate.SetActive(IsVerticalSpeedManaged);
            if (IsVerticalSpeedManaged) VerticalSpeedText.text = "-----";

            switch (FCUMode) {
                case FCUMode.HeadingVerticalSpeed:
                    VerticalSpeedModeIndicate.SetActive(true);
                    FPAModeIndicate.SetActive(false);
                    if (TargetVerticalSpeed >= 0)
                        VerticalSpeedText.text = "+" + TargetVerticalSpeed.ToString("D4");
                    else
                        VerticalSpeedText.text = TargetVerticalSpeed.ToString("D4");
                    break;
                case FCUMode.TrackFPA:
                    VerticalSpeedModeIndicate.SetActive(false);
                    FPAModeIndicate.SetActive(true);
                    if (TargetVerticalSpeed >= 0)
                        VerticalSpeedText.text = "+" + TargetFPA;
                    else
                        VerticalSpeedText.text = TargetFPA.ToString();
                    break;
            }
        }

    #region Property

        [FieldChangeCallback(nameof(FCUMode))] public FCUMode _fcuMode = FCUMode.HeadingVerticalSpeed;

        public FCUMode FCUMode {
            get => _fcuMode;
            set {
                _fcuMode = value;
                UpdateFCUMode();
            }
        }

        [FieldChangeCallback(nameof(IsSpeedManaged))]
        public bool _isSpeedManaged;

        public bool IsSpeedManaged {
            get => _isSpeedManaged;
            set {
                _isSpeedManaged = value;
                UpdateSpeedWindow();
            }
        }

        [FieldChangeCallback(nameof(TargetSpeed))]
        public int _targetSpeed = 100;

        public int TargetSpeed {
            get => _targetSpeed;
            set {
                _targetSpeed = value;
                UpdateSpeedWindow();
            }
        }

        [FieldChangeCallback(nameof(IsMachSpeed))]
        public bool _isMachSpeed;

        public bool IsMachSpeed {
            get => _isMachSpeed;
            set {
                _isMachSpeed = value;
                UpdateSpeedWindow();
            }
        }

        [FieldChangeCallback(nameof(TargetMach))]
        public double _targetMach = 0.6;

        public double TargetMach {
            get => _targetMach;
            set {
                _targetMach = value;
                UpdateSpeedWindow();
            }
        }

        [FieldChangeCallback(nameof(IsHeadingManaged))]
        public bool _isHeadingManaged;

        public bool IsHeadingManaged {
            get => _isHeadingManaged;
            set {
                _isHeadingManaged = value;
                UpdateHeadingWindow();
            }
        }

        [FieldChangeCallback(nameof(TargetHeading))]
        public int _targetHeading = 100;

        public int TargetHeading {
            get => _targetHeading;
            set {
                _targetHeading = value;
                UpdateHeadingWindow();
            }
        }

        [FieldChangeCallback(nameof(TargetAltitude))]
        public int _targetAltitude = 100;

        public int TargetAltitude {
            get => _targetAltitude;
            set {
                _targetAltitude = value;
                UpdateAltitudeWindow();
            }
        }

        [FieldChangeCallback(nameof(IsVerticalSpeedManaged))]
        public bool _isVerticalSpeedManaged;

        public bool IsVerticalSpeedManaged {
            get => _isVerticalSpeedManaged;
            set {
                _isVerticalSpeedManaged = value;
                UpdateVerticalSpeedWindow();
            }
        }

        [FieldChangeCallback(nameof(TargetVerticalSpeed))]
        public int _targetVerticalSpeed;

        public int TargetVerticalSpeed {
            get => _targetVerticalSpeed;
            set {
                _targetVerticalSpeed = value;
                UpdateVerticalSpeedWindow();
            }
        }

        [FieldChangeCallback(nameof(TargetFPA))]
        public double _targetFPA;

        public double TargetFPA {
            get => _targetFPA;
            set {
                _targetFPA = value;
                UpdateVerticalSpeedWindow();
            }
        }

    #endregion

    #region UI Elements

        [Header("UI Elements")]
        public Text SpeedText;

        public Text HeadingText;
        public Text AltitudeText;
        public Text VerticalSpeedText;

        public GameObject SpeedModeIndicate;
        public GameObject MachModeIndicate;
        public GameObject SpeedManagedIndicate;

        public GameObject HeadingModeIndicate;
        public GameObject TrackModeIndicate;
        public GameObject GPSModeIndicate;
        public GameObject HeadingManagedIndicate;

        public GameObject HDGVSModeIndicate1;
        public GameObject HDGVSModeIndicate2;

        public GameObject TRKFPAModeIndicate1;
        public GameObject TRKFPAModeIndicate2;

        public GameObject VerticalSpeedManagedIndicate;
        public GameObject VerticalSpeedModeIndicate;
        public GameObject FPAModeIndicate;

        public GameObject autoPilot1Switch;
        public GameObject autoPilot2Switch;

        public GameObject autoThrustSwitch;

    #endregion
    }

    public enum FCUMode {
        HeadingVerticalSpeed,
        TrackFPA
    }
}