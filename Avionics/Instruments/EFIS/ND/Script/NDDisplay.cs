
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using YuxiFlightInstruments.BasicFlightData;
using YuxiFlightInstruments.Navigation;

namespace A320VAU.ND
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class NDDisplay : UdonSharpBehaviour
    {
        [Tooltip("Flight Data Interface")]
        public YFI_FlightDataInterface FlightData;
        [Tooltip("Navi Data1")]
        public YFI_NavigationReceiver NaviData1;
        [Tooltip("Navi Data2")]
        public YFI_NavigationReceiver NaviData2;
        [Tooltip("仪表的动画控制器")]
        public Animator IndicatorAnimator;

        public float MAXSLIPANGLE = 50;
        [Tooltip("一些罗盘动画起始角度并不为0")]
        public float HDGoffset = 0;

        [Header("UI element")]
        public Text TASText;
        public Text GSText;
        [Header("WayPoint & Navigation Indicatior")]
        public Text waypointText; //NI下 ILS or VOR1
        public Text azimuthText;//NI下  频率
        public Text distanceText; //NI下 "CRS"
        public Text distanceUnitText; //NI下 CRS
        public Text ETAText; //NI下 NAME
        [Header("Navaid indication")]
        public Text VOR1Name;
        public Text VOR1Dist;

        public Text VOR2Name;
        public Text VOR2Dist;

        public GameObject VOR1SelectOnly;
        public GameObject VOR2SelectOnly;
        public GameObject WayPointNavigationIndicatior;

        private readonly int HEADING_HASH = Animator.StringToHash("HeadingNormalize");
        private readonly int SLIPANGLE_HASH = Animator.StringToHash("SlipAngleNormalize");

        void Start()
        {

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
            VOR1SelectOnly.SetActive(NaviData1.hasBeaconSelected);
            VOR2SelectOnly.SetActive(NaviData2.hasBeaconSelected);
            WayPointNavigationIndicatior.SetActive(NaviData2.hasBeaconSelected);
            //功能：waypoint 更新 右下角距离更新 向台背台（TODO）
            //Waypoint & Navaid indication 先只实现一下Navaid indication模式
            //种类
            if (NaviData1.hasBeaconSelected)
            {
                waypointText.text = (NaviData1.SelectedBeacon.beaconType == BeaconType.VOR || NaviData1.SelectedBeacon.beaconType == BeaconType.VORDME) ?
                    "VOR1" :
                    (NaviData1.SelectedBeacon.beaconType == BeaconType.ILS ? "ILS" : "---");
                //频率
                azimuthText.text = NaviData1.SelectedBeacon.beaconFrequency.ToString("f2");
                //
                distanceText.text = "CRS";
                //distanceUnitText.text = NaviData1.omnibearing.ToString("f0");
                distanceUnitText.text = string.Format("{0:000}", NaviData1.omnibearing);

                ETAText.text = NaviData1.SelectedBeacon.beaconName;

                //Navaid indication
                VOR1Name.text = NaviData1.SelectedBeacon.beaconName;
                VOR1Dist.text = (NaviData1.distance * 0.00054f).ToString("f1");
            }

            if (NaviData2.hasBeaconSelected)
            {
                //Navaid indication
                VOR2Name.text = NaviData2.SelectedBeacon.beaconName;
                VOR2Dist.text = (NaviData2.distance * 0.00054f).ToString("f1");
            }

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
    }
}
