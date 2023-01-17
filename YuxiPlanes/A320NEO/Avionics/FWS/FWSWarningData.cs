
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace A320VAU.FWS
{
    public partial class FWSWarningData : UdonSharpBehaviour
    {
        private FWS FWS;
        private bool _hasWarningVisableChange = false;

        public bool Monitor(FWS fws)
        {
            _hasWarningVisableChange = false;
            FWS = fws;

            MonitorEngine();
            MonitorConfigMemo();
            MonitorGear();
            MonitorMemo();
            MonitorConfig();
            return _hasWarningVisableChange;
        }

        private void setWarningMessageVisableValue(ref bool isVisable, bool newValue)
        {
            if (isVisable == newValue) return;
            isVisable = newValue;
            _hasWarningVisableChange = true;
        }
    }
}