using UdonSharp;
using UnityEngine;

namespace A320VAU.FWS
{
    public partial class FWSWarningData : UdonSharpBehaviour
    {
        private FWSWarningMessageData TAKEOFF_MEMO;
        private FWSWarningMessageData LANDING_MEMO;

        private void SetupConfigMemo()
        {
            TAKEOFF_MEMO = GetWarningMessageData(nameof(TAKEOFF_MEMO));
            LANDING_MEMO = GetWarningMessageData(nameof(LANDING_MEMO));
        }

        private void MonitorConfigMemo()
        {
            var isEngine1Running = FWS.equipmentData.IsEngineLRunning;
            var isEngine2Running = FWS.equipmentData.IsEngineRRunning;

            #region Takeoff Memo

            SetWarnVisible(ref TAKEOFF_MEMO.IsVisable,
                FWS.saccAirVehicle.Taxiing && !Mathf.Approximately(FWS.saccAirVehicle.ThrottleInput, 1) &&
                (isEngine1Running || isEngine2Running), true);
            if (TAKEOFF_MEMO.IsVisable)
            {
                // AUTO BRK MAX
                TAKEOFF_MEMO.MessageLine[0].IsMessageVisable = false;
                TAKEOFF_MEMO.MessageLine[1].IsMessageVisable = false;
                // SIGN ON
                TAKEOFF_MEMO.MessageLine[3].IsMessageVisable = false;
                TAKEOFF_MEMO.MessageLine[4].IsMessageVisable = false;
                // CABIN READY
                if (!FWS.equipmentData.Canopy.CanopyOpen)
                {
                    SetWarnVisible(ref TAKEOFF_MEMO.MessageLine[6].IsMessageVisable, false);
                    SetWarnVisible(ref TAKEOFF_MEMO.MessageLine[7].IsMessageVisable, false);
                    SetWarnVisible(ref TAKEOFF_MEMO.MessageLine[8].IsMessageVisable, true);
                }
                else
                {
                    SetWarnVisible(ref TAKEOFF_MEMO.MessageLine[6].IsMessageVisable, true);
                    SetWarnVisible(ref TAKEOFF_MEMO.MessageLine[7].IsMessageVisable, true);
                    SetWarnVisible(ref TAKEOFF_MEMO.MessageLine[8].IsMessageVisable, false);
                }

                // SPLRS ARM
                SetWarnVisible(ref TAKEOFF_MEMO.MessageLine[9].IsMessageVisable, false);
                SetWarnVisible(ref TAKEOFF_MEMO.MessageLine[10].IsMessageVisable, false);
                // FLAP T.O & T.O CONFIG TEST
                if ((FWS.equipmentData.Flap.detentIndex == 1 && FWS.equipmentData.Flap.targetDetentIndex == 1) ||
                    (FWS.equipmentData.Flap.detentIndex == 2 && FWS.equipmentData.Flap.targetDetentIndex == 2))
                {
                    // FLAP T.O
                    SetWarnVisible(ref TAKEOFF_MEMO.MessageLine[12].IsMessageVisable, false);
                    SetWarnVisible(ref TAKEOFF_MEMO.MessageLine[13].IsMessageVisable, false);
                    SetWarnVisible(ref TAKEOFF_MEMO.MessageLine[14].IsMessageVisable, true);
                    // T.O CONFIG
                    SetWarnVisible(ref TAKEOFF_MEMO.MessageLine[15].IsMessageVisable, false);
                    SetWarnVisible(ref TAKEOFF_MEMO.MessageLine[16].IsMessageVisable, false);
                    SetWarnVisible(ref TAKEOFF_MEMO.MessageLine[17].IsMessageVisable, true);
                }
                else
                {
                    // FLAP T.O
                    SetWarnVisible(ref TAKEOFF_MEMO.MessageLine[12].IsMessageVisable, true);
                    SetWarnVisible(ref TAKEOFF_MEMO.MessageLine[13].IsMessageVisable, true);
                    SetWarnVisible(ref TAKEOFF_MEMO.MessageLine[14].IsMessageVisable, false);
                    // T.O CONFIG
                    SetWarnVisible(ref TAKEOFF_MEMO.MessageLine[15].IsMessageVisable, true);
                    SetWarnVisible(ref TAKEOFF_MEMO.MessageLine[16].IsMessageVisable, true);
                    SetWarnVisible(ref TAKEOFF_MEMO.MessageLine[17].IsMessageVisable, false);
                }
            }

            #endregion

            #region Landing Memo

            SetWarnVisible(ref LANDING_MEMO.IsVisable,
                !FWS.saccAirVehicle.Taxiing & FWS.flightData.TAS > 80f & (isEngine1Running | isEngine2Running) &
                (float)FWS.gpws.GetProgramVariable("radioAltitude") < 2000f);

            if (LANDING_MEMO.IsVisable)
            {
                // GEAR DN
                if (FWS.equipmentData.GearLeft.targetPosition == 1)
                {
                    SetWarnVisible(ref LANDING_MEMO.MessageLine[0].IsMessageVisable, false);
                    SetWarnVisible(ref LANDING_MEMO.MessageLine[1].IsMessageVisable, false);
                    SetWarnVisible(ref LANDING_MEMO.MessageLine[2].IsMessageVisable, true);
                }
                else
                {
                    SetWarnVisible(ref LANDING_MEMO.MessageLine[0].IsMessageVisable, true);
                    SetWarnVisible(ref LANDING_MEMO.MessageLine[1].IsMessageVisable, true);
                    SetWarnVisible(ref LANDING_MEMO.MessageLine[2].IsMessageVisable, false);
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
                if (FWS.equipmentData.Flap.targetDetentIndex == 4)
                {
                    SetWarnVisible(ref LANDING_MEMO.MessageLine[12].IsMessageVisable, false);
                    SetWarnVisible(ref LANDING_MEMO.MessageLine[13].IsMessageVisable, false);
                    SetWarnVisible(ref LANDING_MEMO.MessageLine[14].IsMessageVisable, true);
                }
                else
                {
                    SetWarnVisible(ref LANDING_MEMO.MessageLine[12].IsMessageVisable, true);
                    SetWarnVisible(ref LANDING_MEMO.MessageLine[13].IsMessageVisable, true);
                    SetWarnVisible(ref LANDING_MEMO.MessageLine[14].IsMessageVisable, false);
                }

                // FLAPS CONF3
                LANDING_MEMO.MessageLine[15].IsMessageVisable = false;
                LANDING_MEMO.MessageLine[16].IsMessageVisable = false;
            }

            #endregion
        }
    }
}