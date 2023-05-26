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
            var isEngine1Running = FWS.EquipmentData.IsEngineLRunning;
            var isEngine2Running = FWS.EquipmentData.IsEngineRRunning;

            #region Takeoff Memo
            TAKEOFF_MEMO.IsVisable = FWS.SaccAirVehicle.Taxiing & FWS.SaccAirVehicle.ThrottleInput != 1 & (isEngine1Running | isEngine2Running);

            if (TAKEOFF_MEMO.IsVisable)
            {
                // AUTO BRK MAX
                TAKEOFF_MEMO.MessageLine[0].IsMessageVisable = false;
                TAKEOFF_MEMO.MessageLine[1].IsMessageVisable = false;
                // SIGN ON
                TAKEOFF_MEMO.MessageLine[3].IsMessageVisable = false;
                TAKEOFF_MEMO.MessageLine[4].IsMessageVisable = false;
                // CABIN READY
                if (!FWS.EquipmentData.Canopy.CanopyOpen)
                {
                    setWarningMessageVisableValue(ref TAKEOFF_MEMO.MessageLine[6].IsMessageVisable, false);
                    setWarningMessageVisableValue(ref TAKEOFF_MEMO.MessageLine[7].IsMessageVisable, false);
                    setWarningMessageVisableValue(ref TAKEOFF_MEMO.MessageLine[8].IsMessageVisable, true);
                }
                else
                {
                    setWarningMessageVisableValue(ref TAKEOFF_MEMO.MessageLine[6].IsMessageVisable, true);
                    setWarningMessageVisableValue(ref TAKEOFF_MEMO.MessageLine[7].IsMessageVisable, true);
                    setWarningMessageVisableValue(ref TAKEOFF_MEMO.MessageLine[8].IsMessageVisable, false);
                }

                // SPLRS ARM
                TAKEOFF_MEMO.MessageLine[9].IsMessageVisable = false;
                TAKEOFF_MEMO.MessageLine[10].IsMessageVisable = false;
                // FLAP T.O & T.O CONFIG TEST
                if ((FWS.EquipmentData.Flap.detentIndex == 1 && FWS.EquipmentData.Flap.targetDetentIndex == 1) | (FWS.EquipmentData.Flap.detentIndex == 2 && FWS.EquipmentData.Flap.targetDetentIndex == 2))
                {
                    // FLAP T.O
                    setWarningMessageVisableValue(ref TAKEOFF_MEMO.MessageLine[12].IsMessageVisable, false);
                    setWarningMessageVisableValue(ref TAKEOFF_MEMO.MessageLine[13].IsMessageVisable, false);
                    setWarningMessageVisableValue(ref TAKEOFF_MEMO.MessageLine[14].IsMessageVisable, true);
                    // T.O CONFIG
                    setWarningMessageVisableValue(ref TAKEOFF_MEMO.MessageLine[15].IsMessageVisable, false);
                    setWarningMessageVisableValue(ref TAKEOFF_MEMO.MessageLine[16].IsMessageVisable, false);
                    setWarningMessageVisableValue(ref TAKEOFF_MEMO.MessageLine[17].IsMessageVisable, true);
                }
                else
                {
                    // FLAP T.O
                    setWarningMessageVisableValue(ref TAKEOFF_MEMO.MessageLine[12].IsMessageVisable, true);
                    setWarningMessageVisableValue(ref TAKEOFF_MEMO.MessageLine[13].IsMessageVisable, true);
                    setWarningMessageVisableValue(ref TAKEOFF_MEMO.MessageLine[14].IsMessageVisable, false);
                    // T.O CONFIG
                    setWarningMessageVisableValue(ref TAKEOFF_MEMO.MessageLine[15].IsMessageVisable, true);
                    setWarningMessageVisableValue(ref TAKEOFF_MEMO.MessageLine[16].IsMessageVisable, true);
                    setWarningMessageVisableValue(ref TAKEOFF_MEMO.MessageLine[17].IsMessageVisable, false);
                }
            }
            #endregion

            #region Landing Memo
            setWarningMessageVisableValue(ref LANDING_MEMO.IsVisable, !FWS.SaccAirVehicle.Taxiing & FWS.FlightData.TAS > 80f & (isEngine1Running | isEngine2Running) & (float)FWS.GPWS.GetProgramVariable("radioAltitude") < 2000f);

            // GEAR DN
            if (FWS.EquipmentData.GearLeft.targetPosition == 1)
            {
                setWarningMessageVisableValue(ref LANDING_MEMO.MessageLine[0].IsMessageVisable, false);
                setWarningMessageVisableValue(ref LANDING_MEMO.MessageLine[1].IsMessageVisable, false);
                setWarningMessageVisableValue(ref LANDING_MEMO.MessageLine[2].IsMessageVisable, true);
            }
            else
            {
                setWarningMessageVisableValue(ref LANDING_MEMO.MessageLine[0].IsMessageVisable, true);
                setWarningMessageVisableValue(ref LANDING_MEMO.MessageLine[1].IsMessageVisable, true);
                setWarningMessageVisableValue(ref LANDING_MEMO.MessageLine[2].IsMessageVisable, false);
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
            if (FWS.EquipmentData.Flap.targetDetentIndex == 4)
            {
                setWarningMessageVisableValue(ref LANDING_MEMO.MessageLine[12].IsMessageVisable, false);
                setWarningMessageVisableValue(ref LANDING_MEMO.MessageLine[13].IsMessageVisable, false);
                setWarningMessageVisableValue(ref LANDING_MEMO.MessageLine[14].IsMessageVisable, true);
            }
            else
            {
                setWarningMessageVisableValue(ref LANDING_MEMO.MessageLine[12].IsMessageVisable, true);
                setWarningMessageVisableValue(ref LANDING_MEMO.MessageLine[13].IsMessageVisable, true);
                setWarningMessageVisableValue(ref LANDING_MEMO.MessageLine[14].IsMessageVisable, false);
            }

            // FLAPS CONF3
            LANDING_MEMO.MessageLine[15].IsMessageVisable = false;
            LANDING_MEMO.MessageLine[16].IsMessageVisable = false;
            #endregion
        }
    }
}