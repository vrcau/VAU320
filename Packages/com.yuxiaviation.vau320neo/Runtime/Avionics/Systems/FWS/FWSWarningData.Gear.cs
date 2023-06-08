using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UdonSharp;

namespace A320VAU.FWS
{
    public partial class FWSWarningData : UdonSharpBehaviour
    {
        private FWSWarningMessageData BRAKES_HOT;

        private void SetupGear()
        {
            BRAKES_HOT = GetWarningMessageData(nameof(BRAKES_HOT));
        }

        private void MonitorGear()
        {
            // We don't have brake hot simulate, so......
            // if (BRAKES_HOT.IsVisable)
            // {
            //     if (FWS.saccAirVehicle.Taxiing)
            //     {
            //         // Ground
            //         BRAKES_HOT.MessageLine[0].IsMessageVisable = true;
            //         BRAKES_HOT.MessageLine[1].IsMessageVisable = true;
            //         BRAKES_HOT.MessageLine[2].IsMessageVisable = true;
            //         // Air
            //         BRAKES_HOT.MessageLine[3].IsMessageVisable = false;
            //         BRAKES_HOT.MessageLine[4].IsMessageVisable = false;
            //         BRAKES_HOT.MessageLine[5].IsMessageVisable = false;
            //     }
            //     else
            //     {
            //         // Ground
            //         BRAKES_HOT.MessageLine[0].IsMessageVisable = false;
            //         BRAKES_HOT.MessageLine[1].IsMessageVisable = false;
            //         BRAKES_HOT.MessageLine[2].IsMessageVisable = false;
            //         // Air
            //         BRAKES_HOT.MessageLine[3].IsMessageVisable = true;
            //         BRAKES_HOT.MessageLine[4].IsMessageVisable = FWS.equipmentData.GearNose.targetPosition != 1;
            //         BRAKES_HOT.MessageLine[5].IsMessageVisable = true;
            //     }
            // }
        }
    }
}