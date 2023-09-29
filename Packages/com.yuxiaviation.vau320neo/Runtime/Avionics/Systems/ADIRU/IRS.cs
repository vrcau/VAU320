using A320VAU.Common;
using SaccFlightAndVehicles;
using UdonSharp;
using UnityEngine;
using YuxiFlightInstruments.BasicFlightData;

namespace A320VAU.ADIRU {
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class IRS : UdonSharpBehaviour {
        private DependenciesInjector _injector;
        private YFI_FlightDataInterface _flightDataInterface;
        private SaccAirVehicle _saccAirVehicle;

        public float pitch { get; private set; } = 0;
        public float bank { get; private set; } = 0;

        public float trackPitchAngle { get; private set; } = 0;
        public float trackSlipAngle { get; private set; } = 0;

        public float heading { get; private set; } = 0;
        public float track { get; private set; } = 0;

        public float groundSpeed { get; private set; } = 0;

        public Vector2 position { get; private set; } = Vector2.zero;

        public Vector3 velocity { get; private set; } = Vector3.zero;

        private void Start() {
            _injector = DependenciesInjector.GetInstance(this);
            _flightDataInterface = _injector.flightData;
            _saccAirVehicle = _injector.saccAirVehicle;
        }

        private void LateUpdate() {

            // Implementation here
            pitch = _flightDataInterface.pitch;
            bank = _flightDataInterface.bank;

            trackPitchAngle = _flightDataInterface.trackPitchAngle;
            trackSlipAngle = _flightDataInterface.SlipAngle;

            heading = _flightDataInterface.heading;
            track = heading; // temp

            groundSpeed = _flightDataInterface.groundSpeed;

            position = _saccAirVehicle.CenterOfMass.position;

            velocity = (Vector3)_flightDataInterface.GetProgramVariable("currentVelocity");
        }
    }
}