
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
            return _hasWarningVisableChange;
        }
    }
}