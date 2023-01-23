using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using System;
using VRC.Udon;

namespace A320VAU.PFD
{
    public class DU : UdonSharpBehaviour
    {
        public GameObject SelfTestPage;
        public GameObject InvaildDataPage;
        public GameObject PowerPage;
        public GameObject PowerFlashCover;

        public bool BypassSlefTest = false;

        private DateTimeOffset powerUpTime;
        private DateTimeOffset flashStartTime;
        private DateTimeOffset flashEndTime;
        private DateTimeOffset selfTestStartTime;
        private DateTimeOffset selfTestCompleteTime;
        private bool inSelfTest = false;
        private bool isSelfTestComplete = false;
        private bool inFlash = false;
        private bool isFlashComplete = false;

        void OnEnable()
        {
            // power up!
            // We need Delay function!!!!!
            InitDU();
            powerUpTime = DateTimeOffset.Now;

            if (BypassSlefTest)
            {
                inSelfTest = false;
                isSelfTestComplete = true;
                inFlash = false;
                isFlashComplete = true;
                return;
            }

            PowerPage.SetActive(true);
            var flashStartDelay = UnityEngine.Random.Range(2f, 5f);
            flashStartTime = DateTimeOffset.Now.AddSeconds(flashStartDelay);
        }

        void LateUpdate()
        {
            if (isSelfTestComplete) gameObject.SetActive(false);

            if (inSelfTest & DateTimeOffset.Now > selfTestCompleteTime)
            {
                Debug.Log("DU boot complete");
                SelfTestPage.SetActive(false);

                isSelfTestComplete = true;
                inSelfTest = false;
                return;
            }

            if (!inSelfTest & selfTestStartTime > powerUpTime & DateTimeOffset.Now > selfTestStartTime)
            {
                Debug.Log("DU Start Selftest");
                PowerPage.SetActive(false);
                SelfTestPage.SetActive(true);

                var selfTestDelay = UnityEngine.Random.Range(25f, 40f);
                selfTestCompleteTime = DateTimeOffset.Now.AddSeconds(selfTestDelay);

                inSelfTest = true;

                SelfTestPage.SetActive(true);
                return;
            }

            if (isFlashComplete) return;

            if (inFlash & flashEndTime > powerUpTime & DateTimeOffset.Now > flashEndTime)
            {
                Debug.Log("DU Flash End");
                inFlash = false;
                isFlashComplete = true;
                PowerFlashCover.SetActive(false);

                var selfTestStartDelay = UnityEngine.Random.Range(1f, 2f);
                selfTestStartTime = DateTimeOffset.Now.AddSeconds(selfTestStartDelay);
            }

            if (!isFlashComplete & !inFlash & DateTimeOffset.Now > flashStartTime)
            {
                Debug.Log("DU Start Flash");
                inFlash = true;

                flashEndTime = DateTimeOffset.Now.AddMilliseconds(200);
                PowerFlashCover.SetActive(true);
                return;
            }
        }

        private void InitDU()
        {
            Debug.Log("DU Init");

            PowerPage.SetActive(true);
            PowerFlashCover.SetActive(false);
            InvaildDataPage.SetActive(false);
            SelfTestPage.SetActive(false);

            inSelfTest = false;
            isSelfTestComplete = false;
            inFlash = false;
            isFlashComplete = false;
        }
    }
}