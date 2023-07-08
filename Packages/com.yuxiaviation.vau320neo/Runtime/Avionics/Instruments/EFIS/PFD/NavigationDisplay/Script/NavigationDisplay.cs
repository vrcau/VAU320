using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VirtualAviationJapan;

namespace A320VAU.PFD {
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(90)] // After Nav Selector, Before Instruments
    public class NavigationDisplay : UdonSharpBehaviour {
        private const int UPDATE_STEP_COUNT = 4;
        public NavSelector navSelector;

        [Tooltip("信息显示")]
        public Text NaviInfo;

        public int updateIntervalFrames = 10;
        private float magneticDeclination;
        private int updateIntervalOffsetFrames;

        private void Start() {
            if (!navSelector) navSelector = GetComponentInParent<NavSelector>();
            var navaidDatabaseObj = GameObject.Find(nameof(NavaidDatabase));
            if (navaidDatabaseObj)
                magneticDeclination = navaidDatabaseObj.GetComponent<NavaidDatabase>().magneticDeclination;
            NaviInfo.text = "---\n--- < size = 14 > .-- </ size >\n- < size = 14 > .- </ size >;";
        }

        private void Update() {
            var updateCount = (Time.renderedFrameCount + updateIntervalOffsetFrames) %
                              (UPDATE_STEP_COUNT + updateIntervalFrames);
            if (updateCount == 0) UpdateText();
        }

        private void OnEnable() {
            updateIntervalOffsetFrames = Random.Range(0, UPDATE_STEP_COUNT);
        }

        private Vector3 GetNavaidPosition() {
            var t = navSelector.NavaidTransform;
            return (t ? t : transform).position;
        }

        private int GetBearing() {
            var direction = Vector3.ProjectOnPlane(GetNavaidPosition() - transform.position, Vector3.up).normalized;
            var bearing = (Mathf.RoundToInt(Vector3.SignedAngle(Vector3.forward, direction, Vector3.up)) + 360) % 360;
            return bearing == 0 ? 360 : bearing;
        }

        private void UpdateText() {
            var distance = navSelector.HasDME ? Vector3.Distance(transform.position, GetNavaidPosition()) / 1852.0f : 0;
            var frequency =
                navSelector.Index >= 0 ? navSelector.database.frequencies[navSelector.Index] : 999.9; //TODO 错误频率怎么显示
            NaviInfo.text = string.Format("{0}\n{1:f0}<size=14>.{2:f0}</size>\n{3:f1}",
                navSelector.Identity ?? "---",
                (int)frequency,
                (frequency - (int)frequency) * 10,
                distance
            );
        }
    }
}