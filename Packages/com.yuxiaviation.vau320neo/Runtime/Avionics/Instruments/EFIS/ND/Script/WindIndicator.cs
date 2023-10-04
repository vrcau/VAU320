using System;
using A320VAU.Common;
using A320VAU.Utils;
using SaccFlightAndVehicles;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VirtualCNS;

namespace A320VAU.ND {
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WindIndicator : UdonSharpBehaviour {
        private DependenciesInjector _dependenciesInjector;

        private ADIRU.ADIRU _adiru;

        public Transform windDirectionIndicator;
        public Text windDirectionText;
        public Text windSpeedText;
        
        private readonly float UPDATE_INTERVAL = UpdateIntervalUtil.GetUpdateIntervalFromFPS(10);
        private float _lastUpdate;

        private void Start() {
            _dependenciesInjector = DependenciesInjector.GetInstance(this);

            _adiru = _dependenciesInjector.adiru;
        }

        private void LateUpdate() {
            if (!UpdateIntervalUtil.CanUpdate(ref _lastUpdate, UPDATE_INTERVAL)) return;

            var windSpeed = _adiru.windSpeed;
            var windDirection = _adiru.windDirection;
            var windRelativeDirection = _adiru.irs.heading - windDirection;

            windDirectionIndicator.transform.localRotation = Quaternion.AngleAxis(-windRelativeDirection, Vector3.forward);
            windDirectionText.text = windDirection.ToString("000");
            windSpeedText.text = ((int)(windSpeed * 1.94384f)).ToString();
        }
    }
}