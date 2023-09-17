using JetBrains.Annotations;
using SaccFlightAndVehicles;
using UdonSharp;
using Varneon.VUdon.ArrayExtensions;

namespace A320VAU.Common {
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SystemEventBus : UdonSharpBehaviour {
        private DependenciesInjector _injector;
        private UdonSharpBehaviour[] _receivers = { };

        private SaccEntity _saccEntity;

        private void Start() {
            _injector = DependenciesInjector.GetInstance(this);
            _saccEntity = _injector.saccEntity;
        }

        [PublicAPI]
        public void RegisterSaccEvent(UdonSharpBehaviour behaviour) {
            _saccEntity.ExtensionUdonBehaviours = _saccEntity.ExtensionUdonBehaviours.Add(behaviour);
        }

        [PublicAPI]
        public void Register(UdonSharpBehaviour behaviour) {
            _receivers = _receivers.Add(behaviour);
        }

        [PublicAPI]
        public void SendEvent(string eventName) {
            foreach (var receiver in _receivers) receiver.SendCustomEvent("EventBus_" + eventName);
        }

        [PublicAPI]
        public void SendEventWithOutPrefix(string eventName) {
            foreach (var receiver in _receivers) receiver.SendCustomEvent(eventName);
        }

        [PublicAPI]
        public void SendEventToSacc(string eventName) {
            _saccEntity.SendEventToExtensions(eventName);
        }
    }
}