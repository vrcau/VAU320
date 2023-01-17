
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace A320VAU.FWS
{
    public class WarningMessageLine : UdonSharpBehaviour
    {
        public WarningColor MessageColor;
        public string MessageText;
        public bool IsMessageVisable;
    }
}
