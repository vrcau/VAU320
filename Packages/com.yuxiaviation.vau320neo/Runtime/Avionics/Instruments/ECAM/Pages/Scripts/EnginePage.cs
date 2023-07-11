using A320VAU.Common;
using A320VAU.SFEXT;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace A320VAU.ECAM {
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class EnginePage : ECAMPage {
        private DependenciesInjector _injector;
        private SFEXT_a320_AdvancedEngine _engine1;
        private SFEXT_a320_AdvancedEngine _engine2;

        public Text engine1OilPressureText;
        public Text engine1OilTemperatureText;

        public Text engine2OilPressureText;
        public Text engine2OilTemperatureText;

        public Text engine1IgnText;
        public Text engine2IgnText;

        private bool _isInitialized;

        private void Start() {
            _injector = DependenciesInjector.GetInstance(this);

            _engine1 = _injector.engine1;
            _engine2 = _injector.engine2;

            _isInitialized = true;
        }

        public override void OnPageUpdate() {
            if (!_isInitialized) return;
            
            engine1OilPressureText.text = ((int)(_engine1.oilPressure * 0.0145)).ToString();
            engine2OilPressureText.text = ((int)(_engine2.oilPressure * 0.0145)).ToString();

            engine1OilTemperatureText.text = ((int)_engine1.oilTempurature).ToString();
            engine2OilTemperatureText.text = ((int)_engine2.oilTempurature).ToString();

            engine1IgnText.text = _engine1.starter ? "A" : "";
            engine2IgnText.text = _engine2.starter ? "B" : "";
        }
    }
}