using A320VAU.Common;
using UdonSharp;
using YuxiFlightInstruments.BasicFlightData;
using A320VAU.AtmosphereModel;
using UnityEngine;

namespace A320VAU.ADIRU {
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ADR : UdonSharpBehaviour {
        public DependenciesInjector _dependenciesInjector;
        public YFI_FlightDataInterface _flightDataInterface;
        public EarthAtmosphereModel airDataModule;

        //一次参数从_flightDataInterface里面取
        //二次参数从airDataModule里面取
        public float pressureAltitude => _flightDataInterface.altitude;
        public float trueAirSpeed => _flightDataInterface.TAS;
        public float instrumentAirSpeed => _flightDataInterface.TAS;
        public float mach => airDataModule.MachNumber;
        public float angleOfAttack => _flightDataInterface.angleOfAttack;
        public float AOAPitch => _flightDataInterface.AOAPitch;
        public float verticalSpeed => _flightDataInterface.verticalSpeed;
        public float TemperatureTotal => airDataModule.TemperatureTotal;
        public float Vstall_1g => _flightDataInterface.velocityStall1G;
        public float Vstall => _flightDataInterface.velocityStall;
        private void Start() {
            _dependenciesInjector = DependenciesInjector.GetInstance(this);
            if(_flightDataInterface == null)
                _flightDataInterface = _dependenciesInjector.flightData;
        }

    }
}