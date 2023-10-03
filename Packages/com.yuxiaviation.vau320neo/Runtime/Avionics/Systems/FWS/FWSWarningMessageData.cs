using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;

namespace A320VAU.FWS {
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class FWSWarningMessageData : UdonSharpBehaviour {
        public string Id;
        public string WarningGroup; // example: HYD F/CTL
        public WarningStyle WarningGroupStyle;
        public string WarningTitle; // example: ENGINE DUAL FAILURE
        public WarningStyle WarningTitleStyle;
        public WarningColor TitleColor;
        [HideInInspector] public bool isVisible;
        public DisplayZone Zone; // on the left or right of the ecam
        public WarningType Type;
        public WarningLevel Level;
        public SystemPage SystemPage;

        [HideInInspector]
        public WarningMessageLine[] MessageLine;

        private void Start() {
            MessageLine = GetComponentsInChildren<WarningMessageLine>();
        }
    }
}