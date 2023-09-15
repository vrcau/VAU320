using UdonSharp;
using UnityEngine;

namespace VirtualFlightBag {
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class EnabledOnly : UdonSharpBehaviour {
        public GameObject[] enableObjects = { };

        private void OnEnable() {
            foreach (var enableObject in enableObjects) {
                if (enableObject)
                    enableObject.SetActive(true);
            }
        }

        private void OnDisable() {
            foreach (var enableObject in enableObjects) {
                if (enableObject)
                    enableObject.SetActive(false);
            }
        }
    }
}