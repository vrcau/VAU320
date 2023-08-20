using A320VAU.Common;
using A320VAU.ND.Pages;
using A320VAU.Utils;
using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VirtualAviationJapan;
using YuxiFlightInstruments.BasicFlightData;

namespace A320VAU.ND {
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class NDDisplay : UdonSharpBehaviour {
        private const float MAX_SLIP_ANGLE = 50;

        public int MainDataSource = 1;

        [Tooltip("仪表的动画控制器")]
        public Animator IndicatorAnimator;

        [FieldChangeCallback(nameof(NDMode))] public NDMode _ndMode;
        private FMGC.FMGC _fmgc;

        private DependenciesInjector _injector;

        private MapDisplay[] _mapDisplays;

        private NavSelector _vor1;
        private NavSelector _vor2;
        private YFI_FlightDataInterface FlightData;

        public NDMode NDMode {
            get => _ndMode;
            set {
                _ndMode = (NDMode)((int)value < 0 ? 0 : (int)value % 5);
                NDModeChanged();
            }
        }

        private void Start() {
            _injector = DependenciesInjector.GetInstance(this);
            FlightData = _injector.flightData;
            _fmgc = _injector.fmgc;

            _vor1 = _fmgc.radNav.VOR1;
            _vor2 = _fmgc.radNav.VOR2;

            NDModeChanged();
            _mapDisplays = GetComponentsInChildren<MapDisplay>(true);
        }

        private void OnEnable() {
            NDModeChanged();
        }

    #region Animation Hashs

        private readonly int HEADING_HASH = Animator.StringToHash("HeadingNormalize");
        private readonly int SLIP_ANGLE_HASH = Animator.StringToHash("SlipAngleNormalize");

    #endregion

    #region UI Elements

        [Header("UI element")]
        public Text TASText;

        public Text GSText;

        [Header("WayPoint & Navigation Indicatior")]
        public Text line1Text; //NI下 ILS or VOR1

        public Text line2Text; //NI下  频率
        public Text line3Text; //NI下 "CRS"
        public Text line4Text; //NI下 NAME

        [Header("Navaid indication")]
        public Text VOR1Name;

        public Text VOR1Dist;

        public Text VOR2Name;
        public Text VOR2Dist;

        public GameObject VOR1SelectOnly;
        public GameObject VOR2SelectOnly;
        public GameObject NavInfoIndicatior;

        public GameObject GSIndicator;

    #endregion

    #region EFIS Indicator Elements

        [Header("EFIS Status Display")]
        public GameObject cstr;

        public GameObject waypoint;
        public GameObject vordme;
        public GameObject ndb;
        public GameObject airport;

        [Header("Pages")]
        public GameObject ARCPage;

        public GameObject VORPage;
        public GameObject ILSPage;

        private EFISVisibilityType _efisVisibilityType;

    #endregion

    #region Update

        private readonly float UPDATE_INTERVAL = UpdateIntervalUtil.GetUpdateIntervalFromFPS(30);
        private float _lastUpdate;
        
        private void LateUpdate() {
            if (!UpdateIntervalUtil.CanUpdate(ref _lastUpdate, UPDATE_INTERVAL)) return;
            
            UpdateHeading();
            UpdateSlip();
            TASText.text = FlightData.TAS.ToString("f0");
            GSText.text = FlightData.groundSpeed.ToString("f0");

            UpdateNavigation();
        }

        private void UpdateHeading() {
            var HeadingAngle = FlightData.magneticHeading;
            IndicatorAnimator.SetFloat(HEADING_HASH, HeadingAngle / 360f);
        }

        private void UpdateSlip() {
            IndicatorAnimator.SetFloat(SLIP_ANGLE_HASH,
                Mathf.Clamp01((FlightData.SlipAngle + MAX_SLIP_ANGLE) / (MAX_SLIP_ANGLE + MAX_SLIP_ANGLE)));
        }

    #region Navaid

        private void UpdateNavigation() {
            VOR1SelectOnly.SetActive(_vor1.Index >= 0);
            VOR2SelectOnly.SetActive(_vor2.Index >= 0);
            NavInfoIndicatior.SetActive(_vor2.Index >= 0);

            //功能：waypoint 更新 右下角距离更新 向台背台（TODO）
            //Waypoint & Navaid indication 先只实现一下Navaid indication模式
            //种类
            switch (MainDataSource) {
                case 1:
                    UpdateNavigationInfo(_vor1);
                    break;
                case 2:
                    UpdateNavigationInfo(_vor2);
                    break;
                default:
                    UpdateNavigationInfo(_vor1);
                    break;
            }
        }

        private void UpdateNavigationInfo(NavSelector navigationReceiver) {
            if (NDMode != NDMode.PLAN) {
                if (_vor1.Index >= 0) {
                    VOR1Name.text = _vor1.Identity;
                    VOR1Dist.text = _vor1.HasDME
                        ? (Vector3.Distance(transform.position, GetNavaidPosition(_vor1)) / 1852.0f).ToString("f2")
                        : "--.-";
                }

                if (_vor2.Index >= 0) {
                    VOR2Name.text = _vor2.Identity;
                    VOR2Dist.text = _vor2.HasDME
                        ? (Vector3.Distance(transform.position, GetNavaidPosition(_vor2)) / 1852.0f).ToString("f2")
                        : "--.-";
                }
            }

            switch (NDMode) {
                case NDMode.LS:
                    line1Text.text = $"ILS{MainDataSource}";
                    if (navigationReceiver.Index >= 0) {
                        //频率
                        line2Text.text =
                            $"<color={AirbusAvionicsTheme.Carmine}>{(navigationReceiver.Index >= 0 ? navigationReceiver.database.frequencies[navigationReceiver.Index].ToString("f2") : "---.--")}</color>";
                        line3Text.text =
                            $"CRS <color={AirbusAvionicsTheme.Carmine}>{navigationReceiver.Course:f0}</color> <color={AirbusAvionicsTheme.Blue}>°</color>";


                        line4Text.text =
                            $"<color={AirbusAvionicsTheme.Carmine}>NaviData1.SelectedBeacon.beaconName</color>";
                    }

                    break;
                case NDMode.VOR:
                    if (navigationReceiver.Index >= 0) {
                        line1Text.text = $"VOR{MainDataSource}";
                        //频率
                        line2Text.text = navigationReceiver.Index >= 0
                            ? navigationReceiver.database.frequencies[navigationReceiver.Index].ToString("f2")
                            : "---.--";
                        line3Text.text = "CRS";

                        line4Text.text = _vor1.Identity;
                    }

                    break;
            }
        }

        private Vector3 GetNavaidPosition(NavSelector navSelector) {
            var t = navSelector.NavaidTransform;
            return (t ? t : transform).position;
        }

    #endregion

    #endregion

    #region Navigation Display Pages

        private void NDModeChanged() {
            ARCPage.SetActive(false);
            VORPage.SetActive(false);
            ILSPage.SetActive(false);

            switch (NDMode) {
                case NDMode.LS:
                    ILSPage.SetActive(true);
                    break;
                case NDMode.VOR:
                    VORPage.SetActive(true);
                    break;
                case NDMode.ARC:
                    ARCPage.SetActive(true);
                    break;
                default:
                    ARCPage.SetActive(true);
                    break;
            }
        }

        [PublicAPI]
        public void NDPageNextLocal() {
            NDMode = (NDMode)((int)NDMode + 1);
        }

        [PublicAPI]
        public void NDPagePrevLocal() {
            NDMode = (NDMode)((int)NDMode - 1);
        }

        [PublicAPI]
        public void NDPageChangeLocal() {
            Debug.Log("OnNDPageChange");
            // for one direction
            NDMode = (NDMode)(((int)NDMode + 1) % 5);
        }

    #endregion

    #region EFIS

        private void SetVisibilityType(EFISVisibilityType visibilityType) {
            _efisVisibilityType = visibilityType;
            foreach (var mapDisplay in _mapDisplays)
                mapDisplay.SetVisibilityType(visibilityType);

            ResetEFIS();
            switch (visibilityType) {
                case EFISVisibilityType.CSTR:
                    cstr.SetActive(true);
                    break;
                case EFISVisibilityType.WPT:
                    waypoint.SetActive(true);
                    break;
                case EFISVisibilityType.VORDME:
                    vordme.SetActive(true);
                    break;
                case EFISVisibilityType.NDB:
                    ndb.SetActive(true);
                    break;
                case EFISVisibilityType.APPT:
                    airport.SetActive(true);
                    break;
            }
        }

        // For TouchSwitch Event
        [PublicAPI]
        public void ToggleVisibilityTypeCSTR() {
            ToggleVisibilityType(EFISVisibilityType.CSTR);
        }

        [PublicAPI]
        public void ToggleVisibilityTypeWPT() {
            ToggleVisibilityType(EFISVisibilityType.WPT);
        }

        [PublicAPI]
        public void ToggleVisibilityTypeVORD() {
            ToggleVisibilityType(EFISVisibilityType.VORDME);
        }

        [PublicAPI]
        public void ToggleVisibilityTypeNDB() {
            ToggleVisibilityType(EFISVisibilityType.NDB);
        }

        [PublicAPI]
        public void ToggleVisibilityTypeAPPT() {
            ToggleVisibilityType(EFISVisibilityType.APPT);
        }

        [PublicAPI]
        private void ToggleVisibilityType(EFISVisibilityType type) {
            SetVisibilityType(_efisVisibilityType == type ? EFISVisibilityType.NONE : type);
        }

        private void ResetEFIS() {
            cstr.SetActive(false);
            waypoint.SetActive(false);
            vordme.SetActive(false);
            ndb.SetActive(false);
            airport.SetActive(false);
        }

    #endregion
    }

    public enum NDMode {
        LS,
        VOR,
        NAV,
        ARC,
        PLAN
    }
}