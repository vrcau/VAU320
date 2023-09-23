using System;
using A320VAU.Common;
using UdonSharp;
using UnityEngine.Serialization;

namespace A320VAU.FMGC {
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class FMGCPerformance : UdonSharpBehaviour {
        public FMGC fmgc;

        private DependenciesInjector _injector;

        public int v1 = 143;
        public int vr = 152;
        public int v2 = 162;

        public int reduceThrustAltitude = 1500;
        public int accelerateAltitude = 1500;
        public int engineOutAccelerateAltitude = 1500;

        public int flexTemperature;
        public int takeoffFlapSetting = 2;

        public int vapp = 132;

        public int minimumDescentDecisionAltitude;
        public int decisionHeight = 200;

        public FMGCPerformanceLandingConfig landingConfig = FMGCPerformanceLandingConfig.Full;

        private void Start() {
            _injector = DependenciesInjector.GetInstance(this);

            _injector.systemEventBus.RegisterSaccEvent(this);
            _injector.systemEventBus.Register(this);
        }

        public void EventBus_FlightPhaseChanged() {
            if (fmgc.flightPhase.CurrentFlightPhase == FlightPhase.Done)
                ReInit();
        }

        private void ReInit() {

        }
    }

    public enum FMGCPerformanceLandingConfig {
        Full,
        Conf3
    }
}