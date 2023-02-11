using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UdonSharp;

namespace A320VAU.FWS
{
    public partial class FWSWarningData : UdonSharpBehaviour
    {
        public FWSWarningMessageData DUAL_ENGINE_FAULT;
        public FWSWarningMessageData ENGINE1_SHUT_DOWN;
        public FWSWarningMessageData ENGINE1_FAIL;
        public FWSWarningMessageData ENGINE2_FAIL;

        public void MonitorEngine()
        {
            setWarningMessageVisableValue(ref DUAL_ENGINE_FAULT.IsVisable, !FWS.SaccAirVehicle.Taxiing && (FWS.Engine1.n1 < FWS.Engine1.idleN1) && (FWS.Engine2.n1 < FWS.Engine2.idleN1), true);
            if (DUAL_ENGINE_FAULT.IsVisable) {
                setWarningMessageVisableValue(ref DUAL_ENGINE_FAULT.MessageLine[0].IsMessageVisable, true);
                setWarningMessageVisableValue(ref DUAL_ENGINE_FAULT.MessageLine[1].IsMessageVisable, true);
                setWarningMessageVisableValue(ref DUAL_ENGINE_FAULT.MessageLine[2].IsMessageVisable, true);
                setWarningMessageVisableValue(ref DUAL_ENGINE_FAULT.MessageLine[3].IsMessageVisable, true);
                setWarningMessageVisableValue(ref DUAL_ENGINE_FAULT.MessageLine[4].IsMessageVisable, true);
                setWarningMessageVisableValue(ref DUAL_ENGINE_FAULT.MessageLine[5].IsMessageVisable, true);
                setWarningMessageVisableValue(ref DUAL_ENGINE_FAULT.MessageLine[6].IsMessageVisable, true);
                setWarningMessageVisableValue(ref DUAL_ENGINE_FAULT.MessageLine[7].IsMessageVisable, true);
                setWarningMessageVisableValue(ref DUAL_ENGINE_FAULT.MessageLine[8].IsMessageVisable, true);
                setWarningMessageVisableValue(ref DUAL_ENGINE_FAULT.MessageLine[9].IsMessageVisable, true);
                setWarningMessageVisableValue(ref DUAL_ENGINE_FAULT.MessageLine[10].IsMessageVisable, true);
                setWarningMessageVisableValue(ref DUAL_ENGINE_FAULT.MessageLine[11].IsMessageVisable, true);
                setWarningMessageVisableValue(ref DUAL_ENGINE_FAULT.MessageLine[12].IsMessageVisable, true);
                setWarningMessageVisableValue(ref DUAL_ENGINE_FAULT.MessageLine[13].IsMessageVisable, true);
                setWarningMessageVisableValue(ref DUAL_ENGINE_FAULT.MessageLine[14].IsMessageVisable, true);
                setWarningMessageVisableValue(ref DUAL_ENGINE_FAULT.MessageLine[15].IsMessageVisable, true);
                setWarningMessageVisableValue(ref DUAL_ENGINE_FAULT.MessageLine[16].IsMessageVisable, true);
                setWarningMessageVisableValue(ref DUAL_ENGINE_FAULT.MessageLine[17].IsMessageVisable, true);
                setWarningMessageVisableValue(ref DUAL_ENGINE_FAULT.MessageLine[18].IsMessageVisable, true);
                setWarningMessageVisableValue(ref DUAL_ENGINE_FAULT.MessageLine[19].IsMessageVisable, true);
                setWarningMessageVisableValue(ref DUAL_ENGINE_FAULT.MessageLine[20].IsMessageVisable, true);
                setWarningMessageVisableValue(ref DUAL_ENGINE_FAULT.MessageLine[21].IsMessageVisable, true);
                setWarningMessageVisableValue(ref DUAL_ENGINE_FAULT.MessageLine[22].IsMessageVisable, true);
                setWarningMessageVisableValue(ref DUAL_ENGINE_FAULT.MessageLine[23].IsMessageVisable, true);
                setWarningMessageVisableValue(ref DUAL_ENGINE_FAULT.MessageLine[24].IsMessageVisable, true);
            }

            setWarningMessageVisableValue(ref ENGINE1_FAIL.IsVisable, !FWS.SaccAirVehicle.Taxiing && (FWS.Engine1.n1 < FWS.Engine1.idleN1), true);
            if (ENGINE1_FAIL.IsVisable)
            {
                setWarningMessageVisableValue(ref ENGINE1_FAIL.MessageLine[0].IsMessageVisable, true);
                setWarningMessageVisableValue(ref ENGINE1_FAIL.MessageLine[1].IsMessageVisable, true);
                setWarningMessageVisableValue(ref ENGINE1_FAIL.MessageLine[2].IsMessageVisable, true);
                setWarningMessageVisableValue(ref ENGINE1_FAIL.MessageLine[3].IsMessageVisable, true);
                setWarningMessageVisableValue(ref ENGINE1_FAIL.MessageLine[4].IsMessageVisable, true);
                setWarningMessageVisableValue(ref ENGINE1_FAIL.MessageLine[5].IsMessageVisable, true);
                setWarningMessageVisableValue(ref ENGINE1_FAIL.MessageLine[6].IsMessageVisable, true);
                setWarningMessageVisableValue(ref ENGINE1_FAIL.MessageLine[7].IsMessageVisable, true);
            }

            setWarningMessageVisableValue(ref ENGINE2_FAIL.IsVisable, !FWS.SaccAirVehicle.Taxiing && (FWS.Engine2.n1 < FWS.Engine1.idleN1), true);
            if (ENGINE2_FAIL.IsVisable)
            {
                setWarningMessageVisableValue(ref ENGINE2_FAIL.MessageLine[0].IsMessageVisable, true);
                setWarningMessageVisableValue(ref ENGINE2_FAIL.MessageLine[1].IsMessageVisable, true);
                setWarningMessageVisableValue(ref ENGINE2_FAIL.MessageLine[2].IsMessageVisable, true);
                setWarningMessageVisableValue(ref ENGINE2_FAIL.MessageLine[3].IsMessageVisable, true);
                setWarningMessageVisableValue(ref ENGINE2_FAIL.MessageLine[4].IsMessageVisable, true);
                setWarningMessageVisableValue(ref ENGINE2_FAIL.MessageLine[5].IsMessageVisable, true);
                setWarningMessageVisableValue(ref ENGINE2_FAIL.MessageLine[6].IsMessageVisable, true);
                setWarningMessageVisableValue(ref ENGINE2_FAIL.MessageLine[7].IsMessageVisable, true);
            }

            // setWarningMessageVisableValue(ref ENGINE1_SHUT_DOWN.IsVisable, !FWS.Engine1.starter);
            // if (ENGINE1_SHUT_DOWN.IsVisable)
            // {
            //     if (!FWS.Engine1.starter)
            //     {
            //         setWarningMessageVisableValue(ref ENGINE1_SHUT_DOWN.MessageLine[0].IsMessageVisable, false);
            //         setWarningMessageVisableValue(ref ENGINE1_SHUT_DOWN.MessageLine[1].IsMessageVisable, false);
            //         setWarningMessageVisableValue(ref ENGINE1_SHUT_DOWN.MessageLine[2].IsMessageVisable, false);
            //         setWarningMessageVisableValue(ref ENGINE1_SHUT_DOWN.MessageLine[3].IsMessageVisable, true);
            //         setWarningMessageVisableValue(ref ENGINE1_SHUT_DOWN.MessageLine[4].IsMessageVisable, true);
            //     }
            // }
        }
    }
}