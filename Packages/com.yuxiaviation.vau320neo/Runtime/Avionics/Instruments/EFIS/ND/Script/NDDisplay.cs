
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using YuxiFlightInstruments.BasicFlightData;
using YuxiFlightInstruments.Navigation;
using System;
using A320VAU.Common;
using VirtualAviationJapan;

namespace A320VAU.ND
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class NDDisplay : UdonSharpBehaviour
    {
        [Tooltip("Flight Data Interface")]
        public YFI_FlightDataInterface FlightData;
        [Tooltip("Navi Data1")]
        //public YFI_NavigationReceiver NaviData1;
        public NavSelector NaviData1;
        [Tooltip("Navi Data2")]
        //public YFI_NavigationReceiver NaviData2;
        public NavSelector NaviData2;
        public int MainDataSource = 1;
        [Tooltip("仪表的动画控制器")]
        public Animator IndicatorAnimator;

        public float MAXSLIPANGLE = 50;
        [Tooltip("一些罗盘动画起始角度并不为0")]
        public float HDGoffset = 0;

        [Header("UI element")]
        public Text TASText;
        public Text GSText;
        [Header("WayPoint & Navigation Indicatior")]
        public Text line1Text; //NI下 ILS or VOR1
        public Text line2Text;//NI下  频率
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

        [Header("Pages")]
        public GameObject ARCPage;
        public GameObject VORPage;
        public GameObject ILSPage;

        [FieldChangeCallback(nameof(NDMode))] public NDMode _ndMode;
        public NDMode NDMode
        {
            get
            {
                return _ndMode;
            }
            set
            {
                _ndMode = (NDMode)(((int)value<0) ? 0 : (int)value % 5);
                NDModeChanged();
            }
        }

        private readonly int HEADING_HASH = Animator.StringToHash("HeadingNormalize");
        private readonly int SLIPANGLE_HASH = Animator.StringToHash("SlipAngleNormalize");
        private readonly int LOC_HASH = Animator.StringToHash("LOCDeviation");
        private readonly int GlideSlope_HASH = Animator.StringToHash("GSDeviation");
        private readonly int LOCHDG_HASH = Animator.StringToHash("LOCHDGNormalize");

        void Start()
        {
            NDModeChanged();
        }

        private void LateUpdate()
        {
            UpdateHeading();
            UpdateSlip();
            TASText.text = FlightData.TAS.ToString("f0");
            GSText.text = FlightData.groundSpeed.ToString("f0");

            //导航更新
            UpdateNavigation();

        }

        private void UpdateNavigation()
        {
            //VOR1SelectOnly.SetActive(NaviData1.hasBeaconSelected);
            //VOR2SelectOnly.SetActive(NaviData2.hasBeaconSelected);
            //NavInfoIndicatior.SetActive(NaviData2.hasBeaconSelected);

            VOR1SelectOnly.SetActive(NaviData1.Index >= 0);
            VOR2SelectOnly.SetActive(NaviData2.Index >= 0);
            NavInfoIndicatior.SetActive(NaviData2.Index >= 0);
            //功能：waypoint 更新 右下角距离更新 向台背台（TODO）
            //Waypoint & Navaid indication 先只实现一下Navaid indication模式
            //种类
            switch (MainDataSource)
            {
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
        private void UpdateNavigationInfo(NavSelector navigationReceiver)
        {
            if (NDMode != NDMode.PLAN)
            {
                if (NaviData1.Index >= 0)
                {
                    VOR1Name.text = NaviData1.Identity;   
                    VOR1Dist.text = NaviData1.HasDME ? (Vector3.Distance(transform.position, GetNavaidPosition(NaviData1)) / 1852.0f).ToString("f2") : "--.-";
                }

                if (NaviData1.Index >= 0)
                {
                    VOR2Name.text = NaviData2.Identity;
                    VOR2Dist.text = NaviData2.HasDME ? (Vector3.Distance(transform.position, GetNavaidPosition(NaviData2)) / 1852.0f).ToString("f2") : "--.-";
                }
            }

            switch (NDMode)
            {
                case NDMode.LS:
                    line1Text.text = $"ILS{MainDataSource}";
                    if (navigationReceiver.Index >= 0)
                    {
                        //频率
                        line2Text.text =
                            $"<color={AirbusAvionicsTheme.Carmine}>{((navigationReceiver.Index >= 0) ? (navigationReceiver.database.frequencies[navigationReceiver.Index]).ToString("f2") : "---.--")}</color>";
                        line3Text.text =
                            $"CRS <color={AirbusAvionicsTheme.Carmine}>{navigationReceiver.Course:f0}</color> <color={AirbusAvionicsTheme.Blue}>°</color>";


                        line4Text.text = $"<color={AirbusAvionicsTheme.Carmine}>NaviData1.SelectedBeacon.beaconName</color>";
                        //UpdateILS(navigationReceiver); 由CDI Driver 完成
                    }
                    break;
                case NDMode.VOR:
                    if (navigationReceiver.Index >= 0)
                    {
                        line1Text.text = $"VOR{MainDataSource}";
                        //频率
                        line2Text.text = ((navigationReceiver.Index >= 0) ? (navigationReceiver.database.frequencies[navigationReceiver.Index]).ToString("f2") : "---.--");
                        line3Text.text = "CRS";

                        line4Text.text = NaviData1.Identity;
                    }
                    break;
                default:
                    // Navpoint
                    break;
            }
        }
        /*private void UpdateNavigationInfo(YFI_NavigationReceiver navigationReceiver)
        {
            if (NDMode != NDMode.PLAN)
            {
                if (NaviData1.hasBeaconSelected)
                {
                    VOR1Name.text = NaviData1.SelectedBeacon.beaconName;
                    VOR1Dist.text = (NaviData1.distance * 0.00054f).ToString("f1");
                }

                if (NaviData2.hasBeaconSelected)
                {
                    VOR2Name.text = NaviData2.SelectedBeacon.beaconName;
                    VOR2Dist.text = (NaviData2.distance * 0.00054f).ToString("f1");
                }
            }

            switch (NDMode)
            {
                case NDMode.LS:
                    line1Text.text = $"ILS{MainDataSource}";
                    if (navigationReceiver.hasBeaconSelected)
                    {
                        //频率
                        line2Text.text =
                            $"<color={AirbusAvionicsTheme.Carmine}>{navigationReceiver.SelectedBeacon.beaconFrequency.ToString("f2")}</color>";
                        line3Text.text =
                            $"CRS <color={AirbusAvionicsTheme.Carmine}>{navigationReceiver.SelectedBeacon.runwayHeading}</color> <color={AirbusAvionicsTheme.Blue}>°</color>";


                        line4Text.text = $"<color={AirbusAvionicsTheme.Carmine}>NaviData1.SelectedBeacon.beaconName</color>";
                        UpdateILS(navigationReceiver);
                    }
                    break;
                case NDMode.VOR:
                    if (navigationReceiver.hasBeaconSelected)
                    {
                        line1Text.text = $"VOR{MainDataSource}";
                        //频率
                        line2Text.text = NaviData1.SelectedBeacon.beaconFrequency.ToString("f2");
                        line3Text.text = "CRS";

                        line4Text.text = NaviData1.SelectedBeacon.beaconName;
                    }
                    break;
                default:
                    // Navpoint
                    break;
            }
        }
        */

        private void NDModeChanged()
        {
            ARCPage.SetActive(false);
            VORPage.SetActive(false);
            ILSPage.SetActive(false);

            switch (NDMode)
            {
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

        public void NDPageNextLocal()
        {
            NDMode = (NDMode)((int)NDMode + 1);
        }

        public void NDPagePrevLocal()
        {
            NDMode = (NDMode)((int)NDMode - 1);
        }

        public void NDPageChangeLocal()
        {
            Debug.Log("OnNDPageChange");
            //for one deriction
            NDMode = (NDMode)(((int)NDMode + 1)%5);
        }

        private void UpdateHeading()
        {
            float HeadingAngle = FlightData.magneticHeading;
            IndicatorAnimator.SetFloat(HEADING_HASH, (HeadingAngle - HDGoffset) / 360f);
        }

        private void UpdateSlip()
        {
            IndicatorAnimator.SetFloat(SLIPANGLE_HASH, Mathf.Clamp01((FlightData.SlipAngle + MAXSLIPANGLE) / (MAXSLIPANGLE + MAXSLIPANGLE)));
        }

        private void UpdateILS(YFI_NavigationReceiver navigationReceiver)
        {
            var hdg = navigationReceiver.SelectedBeacon.runwayHeading;
            var LOCHDGNormal = hdg / 360f;
            IndicatorAnimator.SetFloat(LOCHDG_HASH, LOCHDGNormal);

            var azimuth = navigationReceiver.VORazimuth;
            float LOCDeviationNormal = Remap01(azimuth, -1.6f, 1.6f);
            IndicatorAnimator.SetFloat(LOC_HASH, LOCDeviationNormal);

            //GS
            GSIndicator.SetActive(navigationReceiver.GSCapture);
            var GSAngle = navigationReceiver.GSAngle;
            float GSDeviationNormal = Remap01(GSAngle, -0.8f, 0.8f);
            IndicatorAnimator.SetFloat(GlideSlope_HASH, GSDeviationNormal);

            //Debug.Log($"{LOCHDGNormal} | {LOCDeviationNormal} | {GSDeviationNormal}");
        }
        private Vector3 GetNavaidPosition(NavSelector navSelector)
        {
            var t = navSelector.NavaidTransform;
            return (t ? t : transform).position;
        }
        private float Remap01(float value, float valueMin, float valueMax)
        {
            value = Mathf.Clamp01((value - valueMin) / (valueMax - valueMin));
            return value;
        }
    }

    public enum NDMode
    {
        LS,
        VOR,
        NAV,
        ARC,
        PLAN
    }
}
