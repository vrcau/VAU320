using A320VAU.Common;
using A320VAU.Utils;
using Avionics.Systems.Common;
using UdonSharp;
using UnityEngine;
using YuxiFlightInstruments.BasicFlightData;

namespace A320VAU.FMGC {
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class FMGCFlightPhase : UdonSharpBehaviour {
        public FMGC fmgc;

        private DependenciesInjector _injector;
        private AircraftSystemData _aircraftSystemData;
        private ADIRU.ADIRU _adirud;
        private SystemEventBus _eventBus;

        public KeyCode activeApproachKey = KeyCode.Alpha9;
        private bool _isKeyPass;

        public int accelerateAltitude = 1500;

        private bool _isLastFrameTouchDown;
        private float _touchDownAt = -1f;

        [FieldChangeCallback(nameof(CurrentFlightPhase))]
        private FlightPhase _currentFlightPhase = FlightPhase.PreFlight;
        public FlightPhase CurrentFlightPhase {
            get => _currentFlightPhase;
            set {
                _currentFlightPhase = value;

                LogCurrentFlightPhase();
                _eventBus.SendEvent("FlightPhaseChanged");
            }
        }

        private readonly float UPDATE_INTERVAL = UpdateIntervalUtil.GetUpdateIntervalFromSeconds(1);
        private float _lastUpdate = -1f;

        private void Start() {
            _injector = DependenciesInjector.GetInstance(this);
            _aircraftSystemData = _injector.equipmentData;
            _adirud = _injector.adiru;
            _eventBus = _injector.systemEventBus;
        }

        private void LateUpdate() {
            if (Input.GetKey(activeApproachKey)) {
                if (!_isKeyPass) {
                    _isKeyPass = true;
                    if (!(CurrentFlightPhase == FlightPhase.PreFlight || CurrentFlightPhase == FlightPhase.Done)) {
                        CurrentFlightPhase = FlightPhase.Approach;
                    }
                }
            }
            else {
                _isKeyPass = false;
            }

            if (!UpdateIntervalUtil.CanUpdate(ref _lastUpdate, UPDATE_INTERVAL))
                return;

            ShouldGoToNextPhase();
        }

        private void LogCurrentFlightPhase() {
            switch (CurrentFlightPhase) {
                case FlightPhase.PreFlight:
                    Debug.Log("FlightPhase: PreFlight");
                    break;
                case FlightPhase.Takeoff:
                    Debug.Log("FlightPhase: Takeoff");
                    break;
                case FlightPhase.Climb:
                    Debug.Log("FlightPhase: Climb");
                    break;
                case FlightPhase.Cruise:
                    Debug.Log("FlightPhase: Cruise");
                    break;
                case FlightPhase.Descent:
                    Debug.Log("FlightPhase: Descent");
                    break;
                case FlightPhase.Approach:
                    Debug.Log("FlightPhase: Approach");
                    break;
                case FlightPhase.GoAround:
                    Debug.Log("FlightPhase: GoAround");
                    break;
                case FlightPhase.Done:
                    Debug.Log("FlightPhase: Done");
                    break;
            }
        }

        private void ShouldGoToNextPhase() {
            switch (CurrentFlightPhase) {
                case FlightPhase.PreFlight:
                    if (Mathf.Approximately(_aircraftSystemData.engine1ThrottleLeveler, 1f) ||
                        Mathf.Approximately(_aircraftSystemData.engine2ThrottleLeveler, 1f)) {
                        CurrentFlightPhase = FlightPhase.Takeoff;
                    }
                    break;
                case FlightPhase.Takeoff:
                    if (_adirud.adr.pressureAltitude >= accelerateAltitude) {
                        CurrentFlightPhase = FlightPhase.Climb;
                    }

                    break;
                case FlightPhase.Climb:
                    if (Mathf.Approximately(_adirud.adr.pressureAltitude, fmgc.flightPlan.cruiseAltitude) ||
                        _adirud.adr.pressureAltitude >= fmgc.flightPlan.cruiseAltitude) {
                        CurrentFlightPhase = FlightPhase.Cruise;
                    }

                    break;
                case FlightPhase.Cruise:
                    // We don't have managed descent or selected descent for now
                    if (_adirud.adr.pressureAltitude < fmgc.flightPlan.cruiseAltitude - 100f &&
                        _adirud.adr.verticalSpeed < -500f) {
                        CurrentFlightPhase = FlightPhase.Descent;
                    }
                    break;
                case FlightPhase.Descent:
                    // if (fmgc.flightPlan.arrivalAirportIndex != -1) {
                    //     var arrivalAirportTransform = fmgc.navaidDatabase.waypointTransforms[fmgc.flightPlan.arrivalAirportIndex];
                    //     var arrivalAirportPosition = arrivalAirportTransform.position;
                    //     arrivalAirportPosition.y = 0;
                    //
                    //     var aircraftPosition = transform.position;
                    //     aircraftPosition.y = 0;
                    // }
                    break;
                case FlightPhase.Approach:
                    if (_injector.saccAirVehicle.Taxiing) {
                        if (!_isLastFrameTouchDown) {
                            _isLastFrameTouchDown = true;
                            _touchDownAt = Time.time;
                        }

                        if (Time.time - _touchDownAt > 30f) {
                            CurrentFlightPhase = FlightPhase.Done;
                            return;
                        }
                    }
                    else {
                        _isLastFrameTouchDown = false;
                        _touchDownAt = -1f;
                    }

                    if (Mathf.Approximately(_aircraftSystemData.engine2ThrottleLeveler, 1f) ||
                        Mathf.Approximately(_aircraftSystemData.engine1ThrottleLeveler, 1f)) {
                        CurrentFlightPhase = FlightPhase.GoAround;
                    }
                    break;
                case FlightPhase.GoAround:
                    break;
                case FlightPhase.Done:
                    CurrentFlightPhase = FlightPhase.PreFlight;
                    break;
            }
        }
    }

    public enum FlightPhase {
        PreFlight,
        Takeoff,
        Climb,
        Cruise,
        Descent,
        Approach,
        GoAround,
        Done
    }
}