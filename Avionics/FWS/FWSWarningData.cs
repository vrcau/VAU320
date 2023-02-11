
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
        private bool _hasWarningDataVisableChange = false;

        public void Monitor(FWS fws)
        {
            _hasWarningVisableChange = false;
            _hasWarningDataVisableChange = false;
            FWS = fws;

            MonitorEngine();
            MonitorConfigMemo();
            MonitorGear();
            MonitorMemo();
            MonitorConfig();

            fws._hasWarningDataVisableChange = _hasWarningDataVisableChange;
            fws._hasWarningVisableChange = _hasWarningVisableChange;
        }

        private void setWarningMessageVisableValue(ref bool isVisable, bool newValue, bool isWarningData = false)
        {
            if (isVisable == newValue) return;
            isVisable = newValue;
            _hasWarningVisableChange = true;
        }
    }
}