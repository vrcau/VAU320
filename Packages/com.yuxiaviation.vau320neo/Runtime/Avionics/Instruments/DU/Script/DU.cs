using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using System;
using VRC.Udon;

namespace A320VAU.PFD
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DU : UdonSharpBehaviour
    {
        public GameObject SelfTestPage;
        public GameObject InvaildDataPage;
        public GameObject PowerPage;
        public GameObject PowerFlashCover;
        public GameObject PowerSource;

        public bool BypassSlefTest = false;

        private bool inSelftest = false;
        private bool isSelftestCompleted = false;

        private void OnEnable()
        {
            if (isSelftestCompleted | inSelftest | BypassSlefTest) return;
            InitDU();

            PowerPage.SetActive(!isSelftestCompleted);
            if (!PowerSource.activeSelf) return;
            
            inSelftest = true;
            var flashStartDelay = UnityEngine.Random.Range(2f, 5f);
            SendCustomEventDelayedSeconds(nameof(StartFlash), flashStartDelay);
        }

        public void StartFlash()
        {
            if (isSelftestCompleted | !inSelftest | BypassSlefTest) return;
            
            Debug.Log("DU Start Flash");
            PowerFlashCover.SetActive(true);
            SendCustomEventDelayedSeconds(nameof(EndFlash), 0.2f);
        }

        public void EndFlash()
        {
            if (isSelftestCompleted | !inSelftest | BypassSlefTest) return;
            
            PowerFlashCover.SetActive(false);
            var selfTestStartDelay = UnityEngine.Random.Range(1f, 2f);
            SendCustomEventDelayedSeconds(nameof(StartSelftest), selfTestStartDelay);
        }

        public void StartSelftest()
        {
            if (isSelftestCompleted | !inSelftest | BypassSlefTest) return;
            
            Debug.Log("DU Start Selftest");
            PowerPage.SetActive(false);
            SelfTestPage.SetActive(true);

            var selfTestDelay = UnityEngine.Random.Range(25f, 40f);
            SendCustomEventDelayedSeconds(nameof(EndSelftest), selfTestDelay);
        }

        public void EndSelftest()
        {
            Debug.Log("DU boot complete");
            SelfTestPage.SetActive(false);
            inSelftest = false;
            isSelftestCompleted = true;
        }

        public void BypassSelftest()
        {
            InitDU();
            Debug.Log("DU Bypass selftest");
            
            inSelftest = false;
            isSelftestCompleted = true;
            PowerPage.SetActive(false);
        }

        public void InitDU()
        {
            Debug.Log("DU Init");

            PowerPage.SetActive(true);
            PowerFlashCover.SetActive(false);
            InvaildDataPage.SetActive(false);
            SelfTestPage.SetActive(false);

            inSelftest = false;
            isSelftestCompleted = false;
        }
    }
}