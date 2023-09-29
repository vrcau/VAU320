using A320VAU.Common;
using UdonSharp;
using YuxiFlightInstruments.BasicFlightData;

namespace A320VAU.ADIRU {
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ADR : UdonSharpBehaviour {
        private DependenciesInjector _dependenciesInjector;
        private YFI_FlightDataInterface _flightDataInterface;

        public float pressureAltitude { get; private set; }

        public float trueAirSpeed { get; private set; }
        public float instrumentAirSpeed { get; private set; }
        public float mach { get; private set; }

        public float angleOfAttack { get; private set; } = 0;

        public float verticalSpeed { get; private set; } = 0f;

        // TODO: Temperature

        private void Start() {
            _dependenciesInjector = DependenciesInjector.GetInstance(this);
            _flightDataInterface = _dependenciesInjector.flightData;
        }

        private void LateUpdate() {
            pressureAltitude = _flightDataInterface.altitude;

            trueAirSpeed = _flightDataInterface.TAS;
            instrumentAirSpeed = _flightDataInterface.TAS;

            mach = _flightDataInterface.mach;

            angleOfAttack = _flightDataInterface.angleOfAttack;

            verticalSpeed = _flightDataInterface.verticalSpeed;
        }
    }
}