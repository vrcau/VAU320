using System;
using UdonSharp;
using UnityEngine.UI;

namespace VirtualFlightBag {
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class VirtualFlightBagMain : UdonSharpBehaviour {
        public Text timeText;

        private void Update() {
            timeText.text = $"{DateTimeOffset.UtcNow:t}z / {DateTimeOffset.Now:t}";
        }
    }
}
