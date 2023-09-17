using System;
using A320VAU.Common;
using A320VAU.FMGC;
using UdonSharp;

namespace A320VAU.MCDU {
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public partial class PerformancePage : MCDUPage {
        private DependenciesInjector _injector;

        private FMGC.FMGC _fmgc;

        private MCDU _mcdu;

        private void Start() {
            _injector = DependenciesInjector.GetInstance(this);
            _fmgc = _injector.fmgc;
        }

        public override void OnPageInit(MCDU mcdu) {
            _mcdu = mcdu;

            TakeoffUI();
        }
    }
}