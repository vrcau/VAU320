
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

using YuxiFlightInstruments.Navigation;
using VirtualAviationJapan;
using JetBrains.Annotations;

namespace A320VAU.PFD
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(90)] // After Nav Selector, Before Instruments
    public class NavigationDisplay : UdonSharpBehaviour
    {
        public NavSelector navSelector;
        [Tooltip("信息显示")]
        public Text NaviInfo;
        public int updateIntervalFrames = 10;

        private const int UPDATE_STEP_COUNT = 4;
        private float magneticDeclination;
        private int updateIntervalOffsetFrames;

        private Vector3 GetNavaidPosition()
        {
            var t = navSelector.NavaidTransform;
            return (t ? t : transform).position;
        }


        private int GetBearing()
        {
            var direction = Vector3.ProjectOnPlane(GetNavaidPosition() - transform.position, Vector3.up).normalized;
            var bearing = (Mathf.RoundToInt(Vector3.SignedAngle(Vector3.forward, direction, Vector3.up)) + 360) % 360;
            return bearing == 0 ? 360 : bearing;
        }

        private void Start()
        {
            if (!navSelector) navSelector = GetComponentInParent<NavSelector>();
            var navaidDatabaseObj = GameObject.Find(nameof(NavaidDatabase));
            if (navaidDatabaseObj) magneticDeclination = navaidDatabaseObj.GetComponent<NavaidDatabase>().magneticDeclination;
            NaviInfo.text = "---\n--- < size = 14 > .-- </ size >\n- < size = 14 > .- </ size >;";
        }

        private void OnEnable()
        {
            updateIntervalOffsetFrames = Random.Range(0, UPDATE_STEP_COUNT);
        }

        private void Update()
        {
            var updateCount = (Time.renderedFrameCount + updateIntervalOffsetFrames) % (UPDATE_STEP_COUNT + updateIntervalFrames);
            if (updateCount == 0) UpdateText();

        }

        private void UpdateText()
        {
            var distance = navSelector.HasDME ? (Vector3.Distance(transform.position, GetNavaidPosition()) / 1852.0f):0;
            var frequency = navSelector.Index >= 0 ? navSelector.database.frequencies[navSelector.Index] : 999.9; //TODO 错误频率怎么显示
            NaviInfo.text = string.Format("{0}\n{1:f0}<size=14>.{2:f0}</size>\n{3:f1}",
                navSelector.Identity ?? "---",
                (int)frequency,
                (frequency - (int)frequency) * 10,
                distance
                );
        }

        
    }
    /*
    public class NavigationDisplay : UdonSharpBehaviour
    {
        //先只实现一下LOC GS动画
        [Tooltip("Navi Data Interface两部")]
        public YFI_NavigationReceiver naviData1;
        //public YFI_NavigationReceiver naviData2;
        [Tooltip("仪表的动画控制器")]
        public Animator IndicatorAnimator;

        [Tooltip("水平偏离最大角度")]
        public float MAXLOCDEVIATION = 5f;
        [Tooltip("垂直偏离最大角度")]
        public float MAXGSDEVIATION = 5f;

        [Tooltip("信息显示")]
        public Text NaviInfo;
        [Tooltip("导航台选择时启用")]
        public GameObject BeaconSelectedOnly;
        public GameObject LOCIndicator;
        public GameObject GSIndicator;

        private int LOC_HASH = Animator.StringToHash("LOCDeviation");
        private int GlideSlope_HASH = Animator.StringToHash("GSDeviation");

        void Start()
        {
            NaviInfo.text = "VAU\n114 < size = 14 > .50 </ size >\n7 < size = 14 > .4 </ size >;";
            BeaconSelectedOnly.SetActive(false);
        }

        private void LateUpdate()
        {
            //如果当前台有效，更新显示
            if (naviData1.hasBeaconSelected)
            {
                BeaconSelectedOnly.SetActive(true);
                UpdateILS();
            }
            else
            {
                BeaconSelectedOnly.SetActive(false);
            }
        }

        private void UpdateILS()
        {
            //动画更新
            //LOC
            
            var azimuth = naviData1.VORazimuth;
            float LOCDeviationNormal = Remap01(azimuth, -MAXLOCDEVIATION, MAXLOCDEVIATION);
            IndicatorAnimator.SetFloat(LOC_HASH, LOCDeviationNormal);

            //GS
            GSIndicator.SetActive(naviData1.GSCapture);
            var GSAngle = naviData1.GSAngle;
            float GSDeviationNormal = Remap01(GSAngle, -MAXGSDEVIATION, MAXGSDEVIATION);
            IndicatorAnimator.SetFloat(GlideSlope_HASH, GSDeviationNormal);

            //文字更新
            var distance = naviData1.distance * 0.00054f;
            NaviInfo.text = string.Format("{0}\n{1:f0}<size=14>.{2:f0}</size>\n{3:f1}",
                naviData1.SelectedBeacon.beaconName,
                (int)naviData1.frequency,
                (naviData1.frequency- (int)naviData1.frequency)*10,
                distance
                );

        }


        private float Remap01(float value, float valueMin, float valueMax)
        {
            value = Mathf.Clamp01((value - valueMin) / (valueMax - valueMin));
            return value;
        }
    
    }
    */
}
