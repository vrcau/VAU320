using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace A320VAU.Clock {
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Clock : UdonSharpBehaviour {
        private const float UPDATE_INTERVAL = 0.5f;

        [Header("Clock")]
        public Text hhmmText;

        public Text ssText;
        private float _lastUpdate;

        private void Update() {
            if (Time.time - _lastUpdate < 0.5f) return;
            _lastUpdate = Time.time;

            hhmmText.text = DateTime.UtcNow.ToShortTimeString();
            ssText.text = DateTime.UtcNow.Second.ToString("D2");
        }
    }
}