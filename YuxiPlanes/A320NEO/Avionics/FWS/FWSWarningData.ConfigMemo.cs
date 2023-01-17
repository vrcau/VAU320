using System.Collections;
using System.Collections.Generic;
using UdonSharp;
using UnityEngine;

namespace A320VAU.FWS
{
    public partial class FWSWarningData : UdonSharpBehaviour
    {
        public FWSWarningMessageData TAKEOFF_MEMO;
        public FWSWarningMessageData LANDING_MEMO;

        public void MonitorConfigMemo()
        {
            var isEngine1Running = FWS.Engine1.fuel && FWS.Engine1.n1 > 0.63f * FWS.Engine1.idleN1 && !FWS.Engine1.stall;
            var isEngine2Running = FWS.Engine2.fuel && FWS.Engine2.n1 > 0.63f * FWS.Engine2.idleN1 && !FWS.Engine2.stall;

            #region Takeoff Memo
            TAKEOFF_MEMO.IsVisable = FWS.SaccAirVehicle.Taxiing & isEngine1Running & isEngine2Running;

            if (TAKEOFF_MEMO.IsVisable)
            {
                // AUTO BRK MAX
                TAKEOFF_MEMO.MessageLine[0].IsMessageVisable = false;
                TAKEOFF_MEMO.MessageLine[1].IsMessageVisable = false;
                // SIGN ON
                TAKEOFF_MEMO.MessageLine[3].IsMessageVisable = false;
                TAKEOFF_MEMO.MessageLine[4].IsMessageVisable = false;
                // CABIN READY
                TAKEOFF_MEMO.MessageLine[6].IsMessageVisable = false;
                TAKEOFF_MEMO.MessageLine[7].IsMessageVisable = false;
                // SPLRS ARM
                TAKEOFF_MEMO.MessageLine[9].IsMessageVisable = false;
                TAKEOFF_MEMO.MessageLine[10].IsMessageVisable = false;
                // FLAP T.O & T.O CONFIG TEST
                if (FWS.Flaps.detentIndex == 1 && FWS.Flaps.targetDetentIndex == 1)
                {
                    TAKEOFF_MEMO.MessageLine[12].IsMessageVisable = false;
                    TAKEOFF_MEMO.MessageLine[13].IsMessageVisable = false;
                    TAKEOFF_MEMO.MessageLine[14].IsMessageVisable = true;
                    // T.O CONFIG
                    TAKEOFF_MEMO.MessageLine[15].IsMessageVisable = false;
                    TAKEOFF_MEMO.MessageLine[16].IsMessageVisable = false;
                    TAKEOFF_MEMO.MessageLine[17].IsMessageVisable = true;
                }
                else
                {
                    // FLAP T.O
                    TAKEOFF_MEMO.MessageLine[12].IsMessageVisable = true;
                    TAKEOFF_MEMO.MessageLine[13].IsMessageVisable = true;
                    TAKEOFF_MEMO.MessageLine[14].IsMessageVisable = false;
                    // T.O CONFIG
                    TAKEOFF_MEMO.MessageLine[15].IsMessageVisable = true;
                    TAKEOFF_MEMO.MessageLine[16].IsMessageVisable = true;
                    TAKEOFF_MEMO.MessageLine[17].IsMessageVisable = false;
                }
            }
            #endregion

            #region Landing Memo
            LANDING_MEMO.IsVisable = !FWS.SaccAirVehicle.Taxiing & isEngine1Running & isEngine2Running & (float)FWS.GPWS.GetProgramVariable("radioAltitude") < 1000f;

            // GEAR DN
            if (FWS.LeftLadingGear.targetPosition == 1)
            {
                LANDING_MEMO.MessageLine[0].IsMessageVisable = false;
                LANDING_MEMO.MessageLine[1].IsMessageVisable = false;
                LANDING_MEMO.MessageLine[2].IsMessageVisable = true;
            }
            else
            {
                LANDING_MEMO.MessageLine[0].IsMessageVisable = false;
                LANDING_MEMO.MessageLine[1].IsMessageVisable = false;
                LANDING_MEMO.MessageLine[2].IsMessageVisable = true;
            }

            // SINGS ON
            LANDING_MEMO.MessageLine[3].IsMessageVisable = false;
            LANDING_MEMO.MessageLine[4].IsMessageVisable = false;
            LANDING_MEMO.MessageLine[5].IsMessageVisable = true;

            // CABIN READY
            LANDING_MEMO.MessageLine[6].IsMessageVisable = false;
            LANDING_MEMO.MessageLine[7].IsMessageVisable = false;
            LANDING_MEMO.MessageLine[8].IsMessageVisable = true;

            // SPLRS ARM
            LANDING_MEMO.MessageLine[9].IsMessageVisable = false;
            LANDING_MEMO.MessageLine[10].IsMessageVisable = false;
            LANDING_MEMO.MessageLine[11].IsMessageVisable = true;

            // FLAPS FULL
            if (FWS.Flaps.targetDetentIndex == 4)
            {
                LANDING_MEMO.MessageLine[12].IsMessageVisable = false;
                LANDING_MEMO.MessageLine[13].IsMessageVisable = false;
                LANDING_MEMO.MessageLine[14].IsMessageVisable = true;
            }
            else
            {
                LANDING_MEMO.MessageLine[12].IsMessageVisable = true;
                LANDING_MEMO.MessageLine[13].IsMessageVisable = true;
                LANDING_MEMO.MessageLine[14].IsMessageVisable = false;
            }

            // FLAPS CONF3
            LANDING_MEMO.MessageLine[15].IsMessageVisable = false;
            LANDING_MEMO.MessageLine[16].IsMessageVisable = false;
            #endregion

            _hasWarningVisableChange = true;
        }
    }
}