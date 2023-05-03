using System;
using EsnyaSFAddons.SFEXT;
using A320VAU.SFEXT;
using A320VAU.PFD;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using SaccFlightAndVehicles;
using YuxiFlightInstruments.ElectricalBus;

//note:this code is original from https://github.com/esnya/EsnyaSFAddons
//to satisfy vau320's demand, add eletrical start
namespace A320VAU.DEBUGGER
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class a320_Debugger : UdonSharpBehaviour
    {
        public KeyCode startKey = KeyCode.LeftShift;
        public KeyCode stopKey = KeyCode.RightControl;

        private byte prevState;
        private bool initialized, selected, isPilot, isPassenger, isOwner, prevTrigger;
        private string triggerAxis;
        private float stateChangedTime;

        public SaccAirVehicle airVehicle;
        public SFEXT_AuxiliaryPowerUnit apu;
        public YFI_ElectricalBus avionics;
        public SFEXT_a320_AdvancedEngine[] engines;
        public DU[] DUs;
        #region public events
        public void Start()
        {
            
        }

        public void InstantStart()
        {
            foreach (var each in DUs)
            {
                each.BypassSlefTest = true;
            }
            avionics.ToggleBatteryLocal();
            engines[0]._InstantStart();
            engines[1]._InstantStart();
        }

        public void BoomEngines()
        {
            engines[0].fire = true;
            engines[1].fire = true;
        }
        #endregion


        #region SFEXT Utilities
        private UdonSharpBehaviour GetExtention(SaccEntity entity, string udonTypeName)
        {
            foreach (var extention in entity.ExtensionUdonBehaviours)
            {
                if (extention && extention.GetUdonTypeName() == udonTypeName) return extention;
            }
            foreach (var extention in entity.Dial_Functions_L)
            {
                if (extention && extention.GetUdonTypeName() == udonTypeName) return extention;
            }
            foreach (var extention in entity.Dial_Functions_R)
            {
                if (extention && extention.GetUdonTypeName() == udonTypeName) return extention;
            }
            return null;
        }

        private UdonSharpBehaviour[] GetExtentions(SaccEntity entity, string udonTypeName)
        {
            var result = new UdonSharpBehaviour[entity.ExtensionUdonBehaviours.Length + entity.Dial_Functions_L.Length + entity.Dial_Functions_R.Length];
            var count = 0;
            foreach (var extention in entity.ExtensionUdonBehaviours)
            {
                if (extention && extention.GetUdonTypeName() == udonTypeName)
                {
                    result[count++] = extention;
                }
            }
            foreach (var extention in entity.Dial_Functions_L)
            {
                if (extention && extention.GetUdonTypeName() == udonTypeName)
                {
                    result[count++] = extention;
                }
            }
            foreach (var extention in entity.Dial_Functions_R)
            {
                if (extention && extention.GetUdonTypeName() == udonTypeName)
                {
                    result[count++] = extention;
                }
            }

            var finalResult = new UdonSharpBehaviour[count];
            Array.Copy(result, finalResult, count);

            return finalResult;
        }
        #endregion
    }
}
