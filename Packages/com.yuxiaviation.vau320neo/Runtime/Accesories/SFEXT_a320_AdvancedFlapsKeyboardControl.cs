using System;
using EsnyaSFAddons.DFUNC;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace A320VAU.SFEXT {
    public class SFEXT_a320_AdvancedFlapsKeyboardControl : UdonSharpBehaviour {
        public DFUNC_AdvancedFlaps advancedFlaps;

        public KeyCode flapsUpKey = KeyCode.Alpha1;
        public KeyCode flapsDownKey = KeyCode.Alpha2;

        private bool _isKeyPress;

        private void LateUpdate() {
            if (Input.GetKey(flapsUpKey)) {
                if (_isKeyPress) return;

                var targetFlapDetentIndex = advancedFlaps.targetDetentIndex - 1;
                if (targetFlapDetentIndex < advancedFlaps.detents.Length && targetFlapDetentIndex >= 0)
                    advancedFlaps.targetAngle = advancedFlaps.detents[targetFlapDetentIndex];

                _isKeyPress = true;
                return;
            }

            if (Input.GetKey(flapsDownKey)) {
                if (_isKeyPress) return;

                var targetFlapDetentIndex = advancedFlaps.targetDetentIndex + 1;
                if (targetFlapDetentIndex < advancedFlaps.detents.Length && targetFlapDetentIndex >= 0)
                    advancedFlaps.targetAngle = advancedFlaps.detents[targetFlapDetentIndex];

                _isKeyPress = true;
                return;
            }

            _isKeyPress = false;
        }
    }
}