using System;
using A320VAU.Common;
using UdonSharp;
using UnityEngine;
using Random = UnityEngine.Random;

namespace A320VAU.PFD {
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DU : UdonSharpBehaviour {
        private DependenciesInjector _injector;
        private SystemEventBus _eventBus;
        
        public GameObject SelfTestPage;
        public GameObject InvaildDataPage;
        public GameObject PowerPage;
        public GameObject PowerFlashCover;

        private bool _inSelfTest;
        private bool _isSelfTestCompleted;

        private void Start() {
            _injector = DependenciesInjector.GetInstance(this);
            _eventBus = _injector.systemEventBus;
            
            _eventBus.RegisterSaccEvent(this);
        }

        // Sacc Event
        public void SFEXT_O_RespawnButton() => InitDU();

        private void OnEnable() {
            if (_isSelfTestCompleted | _inSelfTest) return;
            InitDU();

            PowerPage.SetActive(!_isSelfTestCompleted);

            _inSelfTest = true;
            var flashStartDelay = Random.Range(2f, 5f);
            SendCustomEventDelayedSeconds(nameof(StartFlash), flashStartDelay);
        }

        public void StartFlash() {
            if (_isSelfTestCompleted | !_inSelfTest) return;

            Debug.Log("DU Start Flash");
            PowerFlashCover.SetActive(true);
            SendCustomEventDelayedSeconds(nameof(EndFlash), 0.2f);
        }

        public void EndFlash() {
            if (_isSelfTestCompleted | !_inSelfTest) return;

            PowerFlashCover.SetActive(false);
            var selfTestStartDelay = Random.Range(1f, 2f);
            SendCustomEventDelayedSeconds(nameof(StartSelftest), selfTestStartDelay);
        }

        public void StartSelftest() {
            if (_isSelfTestCompleted | !_inSelfTest) return;

            Debug.Log("DU Start Selftest");
            PowerPage.SetActive(false);
            SelfTestPage.SetActive(true);

            var selfTestDelay = Random.Range(25f, 40f);
            SendCustomEventDelayedSeconds(nameof(EndSelftest), selfTestDelay);
        }

        public void EndSelftest() {
            Debug.Log("DU boot complete");
            SelfTestPage.SetActive(false);
            _inSelfTest = false;
            _isSelfTestCompleted = true;
        }

        public void BypassSelftest() {
            InitDU();
            Debug.Log("DU Bypass self-test");

            _inSelfTest = false;
            _isSelfTestCompleted = true;
            PowerPage.SetActive(false);
        }

        public void InitDU() {
            Debug.Log("DU Init");

            PowerPage.SetActive(true);
            PowerFlashCover.SetActive(false);
            InvaildDataPage.SetActive(false);
            SelfTestPage.SetActive(false);

            _inSelfTest = false;
            _isSelfTestCompleted = false;
        }
    }
}