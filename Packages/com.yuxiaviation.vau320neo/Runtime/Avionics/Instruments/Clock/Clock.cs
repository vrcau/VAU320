using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace A320VAU.Clock
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Clock : UdonSharpBehaviour
    {
        [Header("Clock")]
        public Text hhmmText;
        public Text ssText;
        
        private void Update()
        {
            hhmmText.text = DateTime.UtcNow.ToShortTimeString();
            ssText.text = DateTime.UtcNow.Second.ToString("D2");
        }
    }   
}