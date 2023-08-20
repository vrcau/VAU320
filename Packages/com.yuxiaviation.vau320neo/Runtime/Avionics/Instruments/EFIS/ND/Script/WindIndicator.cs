using System;
using A320VAU.Common;
using A320VAU.Utils;
using SaccFlightAndVehicles;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VirtualAviationJapan;

namespace A320VAU.ND {
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WindIndicator : UdonSharpBehaviour {
        private DependenciesInjector _dependenciesInjector;

        private SaccAirVehicle _saccAirVehicle;
        private NavaidDatabase _navaidDatabase;

        private Transform _originTransform;

        public Transform windDirectionIndicator;
        public Text windDirectionText;
        public Text windSpeedText;
        
        private readonly float UPDATE_INTERVAL = UpdateIntervalUtil.GetUpdateIntervalFromFPS(10);
        private float _lastUpdate;

        private void Start() {
            _dependenciesInjector = DependenciesInjector.GetInstance(this);

            _saccAirVehicle = _dependenciesInjector.saccAirVehicle;
            _navaidDatabase = _dependenciesInjector.navaidDatabase;

            _originTransform = transform;
        }

        private void LateUpdate() {
            if (!UpdateIntervalUtil.CanUpdate(ref _lastUpdate, UPDATE_INTERVAL)) return;
            
            // https://github.com/VirtualAviationJapan/Virtual-CNS/blob/master/Packages/jp.virtualaviation.virtual-cns/Instruments/Scripts/WindIndicator.cs
            var wind = _saccAirVehicle.Wind;
            var windSpeed = wind.magnitude;
            var xzWindDirection = Vector3.ProjectOnPlane(wind, Vector3.up).normalized;
            var windAbsoluteAngle = (Vector3.SignedAngle(Vector3.forward, xzWindDirection, Vector3.up) +
                                     _navaidDatabase.magneticDeclination + 540) % 360;
            var windRelativeAngle = Vector3.SignedAngle(_originTransform.forward, xzWindDirection, Vector3.up);

            windDirectionIndicator.transform.localRotation = Quaternion.AngleAxis(-windRelativeAngle, Vector3.forward);
            windDirectionText.text = windAbsoluteAngle.ToString("000");
            windSpeedText.text = ((int)(windSpeed * 1.94384f)).ToString();
        }
    }
}