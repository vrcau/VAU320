﻿using A320VAU.Common;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VirtualCNS;

namespace A320VAU.PFD {
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(90)] // After Nav Selector, Before Instruments
    public class NavigationDisplay : UdonSharpBehaviour {
        private const int UPDATE_STEP_COUNT = 4;

        [Tooltip("信息显示")]
        public Text NaviInfo;

        public int updateIntervalFrames = 10;
        private FMGC.FMGC _fmgc;
        private NavSelector _ils;
        private DependenciesInjector _injector;
        private int updateIntervalOffsetFrames;

        private void Start() {
            _injector = DependenciesInjector.GetInstance(this);
            _fmgc = _injector.fmgc;
            _ils = _fmgc.radNav.ILS;

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
            var t = _ils.NavaidTransform;
            return (t ? t : transform).position;
        }

        private void UpdateText() {
            var frequency =
                _ils.Index >= 0 ? _ils.database.frequencies[_ils.Index] : 999.9; //TODO 错误频率怎么显示

            if (_ils.HasDME) {
                NaviInfo.text = string.Format("{0}\n{1:f0}<size=14>.{2:f0}</size>\n{3:f1}",
                _ils.Identity ?? "---",
                (int)frequency,
                (frequency - (int)frequency) * 10,
                Vector3.Distance(transform.position, GetNavaidPosition()) / 1852.0f);
                }
            else {
                NaviInfo.text = string.Format("{0}\n{1:f0}<size=14>.{2:f0}</size>\n-.-",
                _ils.Identity ?? "---",
                (int)frequency,
                (frequency - (int)frequency) * 10);
                }
        }
    }
}