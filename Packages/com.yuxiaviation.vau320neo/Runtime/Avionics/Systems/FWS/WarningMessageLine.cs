using UdonSharp;
using UnityEngine;

namespace A320VAU.FWS {
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class WarningMessageLine : UdonSharpBehaviour {
        public WarningColor MessageColor;
        public string MessageText;
        [HideInInspector] public bool isMessageVisible;
    }
}