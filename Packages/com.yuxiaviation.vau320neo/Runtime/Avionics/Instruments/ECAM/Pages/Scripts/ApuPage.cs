using A320VAU.Common;
using Avionics.Systems.Common;
using EsnyaSFAddons.SFEXT;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace A320VAU.ECAM {
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ApuPage : ECAMPage {
        private DependenciesInjector _injector;
        private AircraftSystemData _aircraftSystemData;
        private SFEXT_AuxiliaryPowerUnit _apu;

        public GameObject apuAvailText;
        public GameObject flapOpenText;
        public Text apuNText;

        private bool _initialized;

        private void Start() {
            _injector = DependenciesInjector.GetInstance(this);
            _aircraftSystemData = _injector.equipmentData;
            _apu = _injector.apu;

            _initialized = true;
        }

        public override void OnPageUpdate() {
            if (!_initialized) return;

            flapOpenText.SetActive(_aircraftSystemData.isApuRunning);
            apuAvailText.SetActive(_aircraftSystemData.isApuStarted);

            apuNText.text = ((int)(_apu.apuAudioSource.volume * 100f)).ToString();
        }
    }
}