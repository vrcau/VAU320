using JetBrains.Annotations;
using SaccFlightAndVehicles;
using UdonSharp;
using UnityEngine;
using Varneon.VUdon.ArrayExtensions;
using YuxiFlightInstruments.BasicFlightData;

namespace A320VAU.Common {
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(151)] //after YFI electrical bus
    public class SystemEventBus : UdonSharpBehaviour {
        public YFI_FlightDataInterface basicFlightData;
        public SaccEntity saccEntity;
        public UdonSharpBehaviour[] receivers = { };
        
        private void Start() {
            saccEntity = basicFlightData.SAVControl.EntityControl;
        }

        [PublicAPI]
        public void RegisterSaccEvent(UdonSharpBehaviour behaviour) {
            saccEntity.ExtensionUdonBehaviours = saccEntity.ExtensionUdonBehaviours.Add(behaviour);
        }

        [PublicAPI]
        public void Register(UdonSharpBehaviour behaviour) {
            receivers = receivers.Add(behaviour);
        }

        [PublicAPI]
        public void SendEvent(string eventName) {
            foreach (var receiver in receivers) receiver.SendCustomEvent("EventBus_" + eventName);
        }

        [PublicAPI]
        public void SendEventWithOutPrefix(string eventName) {
            foreach (var receiver in receivers) receiver.SendCustomEvent(eventName);
        }

        [PublicAPI]
        public void SendEventToSacc(string eventName) {
            saccEntity.SendEventToExtensions(eventName);
        }
    }
}