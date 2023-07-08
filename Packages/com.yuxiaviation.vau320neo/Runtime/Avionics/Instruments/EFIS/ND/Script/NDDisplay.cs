using A320VAU.Common;
using A320VAU.ND.Pages;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VirtualAviationJapan;
using YuxiFlightInstruments.BasicFlightData;

// ReSharper disable UnusedMember.Global

namespace A320VAU.ND {
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class NDDisplay : UdonSharpBehaviour {
        private const float UPDATE_INTERVAL = 0.5f;

        [Tooltip("Flight Data Interface")]
        public YFI_FlightDataInterface FlightData;

        [Tooltip("Navi Data1")]
        public NavSelector NaviData1;

        [Tooltip("Navi Data2")]
        public NavSelector NaviData2;

        public int MainDataSource = 1;

        [Tooltip("仪表的动画控制器")]
        public Animator IndicatorAnimator;

        public float MAXSLIPANGLE = 50;

        [Tooltip("一些罗盘动画起始角度并不为0")]
        public float HDGoffset;

        [FieldChangeCallback(nameof(NDMode))] public NDMode _ndMode;
        private float _lastUpdate;

        private MapDisplay[] _mapDisplays;

        public NDMode NDMode {
            get => _ndMode;
            set {
                _ndMode = (NDMode)((int)value < 0 ? 0 : (int)value % 5);
                NDModeChanged();
            }
        }

        private void Start() {
            NDModeChanged();

            _mapDisplays = GetComponentsInChildren<MapDisplay>(true);
        }

        private void OnEnable() {
            NDModeChanged();
        }

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

    #region EFIS

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

    #region AnimationHash

        private readonly int HEADING_HASH = Animator.StringToHash("HeadingNormalize");
        private readonly int SLIPANGLE_HASH = Animator.StringToHash("SlipAngleNormalize");

    #endregion

    #region Update

        private void LateUpdate() {
            UpdateHeading();
            UpdateSlip();
            TASText.text = FlightData.TAS.ToString("f0");
            GSText.text = FlightData.groundSpeed.ToString("f0");

            // 导航更新
            if (Time.time - _lastUpdate < UPDATE_INTERVAL) return;
            _lastUpdate = Time.time;
            UpdateNavigation();
        }

        private void UpdateHeading() {
            var HeadingAngle = FlightData.magneticHeading;
            IndicatorAnimator.SetFloat(HEADING_HASH, (HeadingAngle - HDGoffset) / 360f);
        }

        private void UpdateSlip() {
            IndicatorAnimator.SetFloat(SLIPANGLE_HASH,
                Mathf.Clamp01((FlightData.SlipAngle + MAXSLIPANGLE) / (MAXSLIPANGLE + MAXSLIPANGLE)));
        }

    #region Navaid

        private void UpdateNavigation() {
            VOR1SelectOnly.SetActive(NaviData1.Index >= 0);
            VOR2SelectOnly.SetActive(NaviData2.Index >= 0);
            NavInfoIndicatior.SetActive(NaviData2.Index >= 0);

            //功能：waypoint 更新 右下角距离更新 向台背台（TODO）
            //Waypoint & Navaid indication 先只实现一下Navaid indication模式
            //种类
            switch (MainDataSource) {
                case 1:
                    UpdateNavigationInfo(NaviData1);
                    break;
                case 2:
                    UpdateNavigationInfo(NaviData2);
                    break;
                default:
                    UpdateNavigationInfo(NaviData1);
                    break;
            }
        }

        private void UpdateNavigationInfo(NavSelector navigationReceiver) {
            if (NDMode != NDMode.PLAN) {
                if (NaviData1.Index >= 0) {
                    VOR1Name.text = NaviData1.Identity;
                    VOR1Dist.text = NaviData1.HasDME
                        ? (Vector3.Distance(transform.position, GetNavaidPosition(NaviData1)) / 1852.0f).ToString("f2")
                        : "--.-";
                }

                if (NaviData2.Index >= 0) {
                    VOR2Name.text = NaviData2.Identity;
                    VOR2Dist.text = NaviData2.HasDME
                        ? (Vector3.Distance(transform.position, GetNavaidPosition(NaviData2)) / 1852.0f).ToString("f2")
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

                        line4Text.text = NaviData1.Identity;
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

        public void NDPageNextLocal() {
            NDMode = (NDMode)((int)NDMode + 1);
        }

        public void NDPagePrevLocal() {
            NDMode = (NDMode)((int)NDMode - 1);
        }

        public void NDPageChangeLocal() {
            Debug.Log("OnNDPageChange");
            // for one deriction
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
        public void ToggleVisibilityTypeCSTR() {
            ToggleVisibilityType(EFISVisibilityType.CSTR);
        }

        public void ToggleVisibilityTypeWPT() {
            ToggleVisibilityType(EFISVisibilityType.WPT);
        }

        public void ToggleVisibilityTypeVORD() {
            ToggleVisibilityType(EFISVisibilityType.VORDME);
        }

        public void ToggleVisibilityTypeNDB() {
            ToggleVisibilityType(EFISVisibilityType.NDB);
        }

        public void ToggleVisibilityTypeAPPT() {
            ToggleVisibilityType(EFISVisibilityType.APPT);
        }

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