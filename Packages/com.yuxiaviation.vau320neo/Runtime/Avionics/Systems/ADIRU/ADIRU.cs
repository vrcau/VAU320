using System;
using A320VAU.Common;
using SaccFlightAndVehicles;
using UdonSharp;
using UnityEngine;

namespace A320VAU.ADIRU {
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ADIRU : UdonSharpBehaviour {
        public ADR adr;
        public IRS irs;

        public InertialReferenceMode inertialReferenceMode { get; private set; } = InertialReferenceMode.Navigation;
        public AlignState alignState { get; private set; } = AlignState.Aligned;

        public float windDirection { get; private set; } = 0f;
        public float windSpeed { get; private set; } = 0f;

        private DependenciesInjector _injector;
        private SaccAirVehicle _saccAirVehicle;

        private void Start() {
            _injector = DependenciesInjector.GetInstance(this);
            _saccAirVehicle = _injector.saccAirVehicle;

            adr = GetComponentInChildren<ADR>(true);
            irs = GetComponentInChildren<IRS>(true);
        }

        private void LateUpdate() {
            var wind = _saccAirVehicle.Wind;

            windSpeed = wind.magnitude;

            var xzWindDirection = Vector3.ProjectOnPlane(wind, Vector3.up).normalized;
            windDirection = (Vector3.SignedAngle(Vector3.forward, xzWindDirection, Vector3.up) + 540) % 360;
        }
    }

    public enum InertialReferenceMode  {
        Off,
        Navigation,
        Attitude
    }

    public enum AlignState {
        Off,
        Aligning,
        Aligned,
    }
}