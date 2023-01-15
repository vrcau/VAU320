using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UdonSharp;

namespace A320VAU.FWS
{
    public partial class FWSWarningData : UdonSharpBehaviour
    {
        public FWSWarningMessageData DUAL_ENGINE_FAULT;

        public void MonitorEngine()
        {
            if (!DUAL_ENGINE_FAULT.IsVisable) {
                DUAL_ENGINE_FAULT.IsVisable = true;
                _hasWarningVisableChange = true;
            }
        }
    }
}