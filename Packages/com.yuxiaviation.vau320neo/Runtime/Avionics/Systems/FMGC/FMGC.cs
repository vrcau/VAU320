using A320VAU.Common;
using UdonSharp;
using UnityEngine;
using VirtualAviationJapan;

namespace A320VAU.FMGC {
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class FMGC : UdonSharpBehaviour {
        public NavaidDatabase navaidDatabase;
        public FMGCRadNav radNav;
        private DependenciesInjector _injector;

        private void Start() {
            _injector = DependenciesInjector.GetInstance(this);

            navaidDatabase = _injector.navaidDatabase;
            if (!navaidDatabase)
                Debug.LogError("You don't have a NavaidDatabase in your scene, FMGC won't work.", this);

            radNav = GetComponentInChildren<FMGCRadNav>();
            radNav.fmgc = this;
        }
    }
}