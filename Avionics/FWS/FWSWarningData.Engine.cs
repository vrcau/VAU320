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
        }
    }
}