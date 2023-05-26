﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UdonSharp;


namespace A320VAU.FWS
{
    public partial class FWSWarningData : UdonSharpBehaviour
    {
        public FWSWarningMessageData ENGINE1_FIRE;
        public FWSWarningMessageData ENGINE2_FIRE;

        public FWSWarningMessageData DUAL_ENGINE_FAULT;

        public FWSWarningMessageData ENGINE1_SHUT_DOWN;

        public FWSWarningMessageData ENGINE1_FAIL;
        public FWSWarningMessageData ENGINE2_FAIL;

        public FWSWarningMessageData ENGINE1_EGT_OVERLIMIT;
        public FWSWarningMessageData ENGINE2_EGT_OVERLIMIT;
        public FWSWarningMessageData ENGINE1_N1_OVERLIMIT;
        public FWSWarningMessageData ENGINE2_N1_OVERLIMIT;
        public FWSWarningMessageData ENGINE1_N2_OVERLIMIT;
        public FWSWarningMessageData ENGINE2_N2_OVERLIMIT;

        public void MonitorEngine()
        {
            var engine1N1 = FWS.EquipmentData.N1LRef * 100;
            var engine2N1 = FWS.EquipmentData.N2LRef * 100;

            var engine1N2 = FWS.EquipmentData.N2RRef * 100;
            var engine2N2 = FWS.EquipmentData.N2RRef * 100;

            #region ENGIEN FIRE
            setWarningMessageVisableValue(ref ENGINE1_FIRE.IsVisable, FWS.EquipmentData.EngineL.fire, true);
            if (ENGINE1_FIRE.IsVisable)
            {
                setWarningMessageVisableValue(ref ENGINE1_FIRE.MessageLine[0].IsMessageVisable, FWS.SaccAirVehicle.ThrottleInput != FWS.EquipmentData.EngineL.idlePoint);
                setWarningMessageVisableValue(ref ENGINE1_FIRE.MessageLine[1].IsMessageVisable, FWS.EquipmentData.EngineL.fuel);
                setWarningMessageVisableValue(ref ENGINE1_FIRE.MessageLine[2].IsMessageVisable, true);
                setWarningMessageVisableValue(ref ENGINE1_FIRE.MessageLine[3].IsMessageVisable, true);
            }

            setWarningMessageVisableValue(ref ENGINE2_FIRE.IsVisable, FWS.EquipmentData.EngineR.fire, true);
            if (ENGINE2_FIRE.IsVisable)
            {
                setWarningMessageVisableValue(ref ENGINE2_FIRE.MessageLine[0].IsMessageVisable, FWS.SaccAirVehicle.ThrottleInput != FWS.EquipmentData.EngineR.idlePoint);
                setWarningMessageVisableValue(ref ENGINE2_FIRE.MessageLine[1].IsMessageVisable, FWS.EquipmentData.EngineR.fuel);
                setWarningMessageVisableValue(ref ENGINE2_FIRE.MessageLine[2].IsMessageVisable, true);
                setWarningMessageVisableValue(ref ENGINE2_FIRE.MessageLine[3].IsMessageVisable, true);
            }
            #endregion

            #region DUAL ENGINE FAIL
            setWarningMessageVisableValue(ref DUAL_ENGINE_FAULT.IsVisable, !FWS.SaccAirVehicle.Taxiing && !FWS.EquipmentData.IsEngineLRunning && !FWS.EquipmentData.IsEngineRRunning, true);
            if (DUAL_ENGINE_FAULT.IsVisable)
            {
                setWarningMessageVisableValue(ref DUAL_ENGINE_FAULT.MessageLine[0].IsMessageVisable, true);
                setWarningMessageVisableValue(ref DUAL_ENGINE_FAULT.MessageLine[1].IsMessageVisable, !Mathf.Approximately(FWS.EquipmentData.ThrottleLevelerL, FWS.EquipmentData.EngineL.idlePoint));
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
            #endregion

            #region ENGINE FAIL
            setWarningMessageVisableValue(ref ENGINE1_FAIL.IsVisable, !FWS.SaccAirVehicle.Taxiing && !FWS.EquipmentData.IsEngineLRunning, true);
            if (ENGINE1_FAIL.IsVisable)
            {
                setWarningMessageVisableValue(ref ENGINE1_FAIL.MessageLine[0].IsMessageVisable, true);
                setWarningMessageVisableValue(ref ENGINE1_FAIL.MessageLine[1].IsMessageVisable, !Mathf.Approximately(FWS.EquipmentData.ThrottleLevelerL, FWS.EquipmentData.EngineL.idlePoint));
                setWarningMessageVisableValue(ref ENGINE1_FAIL.MessageLine[2].IsMessageVisable, true);
                setWarningMessageVisableValue(ref ENGINE1_FAIL.MessageLine[3].IsMessageVisable, true);
                setWarningMessageVisableValue(ref ENGINE1_FAIL.MessageLine[4].IsMessageVisable, true);
                setWarningMessageVisableValue(ref ENGINE1_FAIL.MessageLine[5].IsMessageVisable, true);
                setWarningMessageVisableValue(ref ENGINE1_FAIL.MessageLine[6].IsMessageVisable, true);
                setWarningMessageVisableValue(ref ENGINE1_FAIL.MessageLine[7].IsMessageVisable, true);
            }

            setWarningMessageVisableValue(ref ENGINE2_FAIL.IsVisable, !FWS.SaccAirVehicle.Taxiing && !FWS.EquipmentData.IsEngineRRunning, true);
            if (ENGINE2_FAIL.IsVisable)
            {
                setWarningMessageVisableValue(ref ENGINE2_FAIL.MessageLine[0].IsMessageVisable, true);
                setWarningMessageVisableValue(ref ENGINE2_FAIL.MessageLine[1].IsMessageVisable, !Mathf.Approximately(FWS.EquipmentData.ThrottleLevelerR, FWS.EquipmentData.EngineR.idlePoint));
                setWarningMessageVisableValue(ref ENGINE2_FAIL.MessageLine[2].IsMessageVisable, true);
                setWarningMessageVisableValue(ref ENGINE2_FAIL.MessageLine[3].IsMessageVisable, true);
                setWarningMessageVisableValue(ref ENGINE2_FAIL.MessageLine[4].IsMessageVisable, true);
                setWarningMessageVisableValue(ref ENGINE2_FAIL.MessageLine[5].IsMessageVisable, true);
                setWarningMessageVisableValue(ref ENGINE2_FAIL.MessageLine[6].IsMessageVisable, true);
                setWarningMessageVisableValue(ref ENGINE2_FAIL.MessageLine[7].IsMessageVisable, true);
            }
            #endregion

            #region EGT Overlimit
            setWarningMessageVisableValue(ref ENGINE1_EGT_OVERLIMIT.IsVisable, FWS.EquipmentData.EGTL > 1060f, true);
            if (ENGINE1_EGT_OVERLIMIT.IsVisable)
            {
                setWarningMessageVisableValue(ref ENGINE1_EGT_OVERLIMIT.MessageLine[0].IsMessageVisable, !Mathf.Approximately(FWS.EquipmentData.ThrottleLevelerL, FWS.EquipmentData.EngineL.idlePoint));
                setWarningMessageVisableValue(ref ENGINE1_EGT_OVERLIMIT.MessageLine[1].IsMessageVisable, FWS.EquipmentData.EngineL.fuel);
            }

            setWarningMessageVisableValue(ref ENGINE2_EGT_OVERLIMIT.IsVisable, FWS.EquipmentData.EGTR > 1060f, true);
            if (ENGINE2_EGT_OVERLIMIT.IsVisable)
            {
                setWarningMessageVisableValue(ref ENGINE2_EGT_OVERLIMIT.MessageLine[0].IsMessageVisable, !Mathf.Approximately(FWS.EquipmentData.ThrottleLevelerR, FWS.EquipmentData.EngineR.idlePoint));
                setWarningMessageVisableValue(ref ENGINE2_EGT_OVERLIMIT.MessageLine[1].IsMessageVisable, FWS.EquipmentData.EngineR.fuel);
            }
            #endregion

            #region N1 OVERLIMIT
            setWarningMessageVisableValue(ref ENGINE1_N1_OVERLIMIT.IsVisable, engine1N1 > 100f, true);
            if (ENGINE1_N1_OVERLIMIT.IsVisable)
            {
                setWarningMessageVisableValue(ref ENGINE1_N1_OVERLIMIT.MessageLine[0].IsMessageVisable, !Mathf.Approximately(FWS.EquipmentData.ThrottleLevelerL, FWS.EquipmentData.EngineL.idlePoint));
                setWarningMessageVisableValue(ref ENGINE1_N1_OVERLIMIT.MessageLine[1].IsMessageVisable, FWS.EquipmentData.EngineL.fuel);
            }

            setWarningMessageVisableValue(ref ENGINE2_N1_OVERLIMIT.IsVisable, engine2N1 > 100f, true);
            if (ENGINE2_N1_OVERLIMIT.IsVisable)
            {
                setWarningMessageVisableValue(ref ENGINE2_N1_OVERLIMIT.MessageLine[0].IsMessageVisable, !Mathf.Approximately(FWS.EquipmentData.ThrottleLevelerR, FWS.EquipmentData.EngineR.idlePoint));
                setWarningMessageVisableValue(ref ENGINE2_N1_OVERLIMIT.MessageLine[1].IsMessageVisable, FWS.EquipmentData.EngineR.fuel);
            }
            #endregion

            #region N2 OVERLIMIT
            setWarningMessageVisableValue(ref ENGINE1_N2_OVERLIMIT.IsVisable, engine2N2 > 100f, true);
            if (ENGINE1_N2_OVERLIMIT.IsVisable)
            {
                setWarningMessageVisableValue(ref ENGINE1_N2_OVERLIMIT.MessageLine[0].IsMessageVisable, !Mathf.Approximately(FWS.EquipmentData.ThrottleLevelerL, FWS.EquipmentData.EngineL.idlePoint));
                setWarningMessageVisableValue(ref ENGINE1_N2_OVERLIMIT.MessageLine[1].IsMessageVisable, FWS.EquipmentData.EngineL.fuel);
            }

            setWarningMessageVisableValue(ref ENGINE2_N2_OVERLIMIT.IsVisable, engine2N2 > 100f, true);
            if (ENGINE2_N2_OVERLIMIT.IsVisable)
            {
                setWarningMessageVisableValue(ref ENGINE2_N2_OVERLIMIT.MessageLine[0].IsMessageVisable, !Mathf.Approximately(FWS.EquipmentData.ThrottleLevelerR, FWS.EquipmentData.EngineR.idlePoint));
                setWarningMessageVisableValue(ref ENGINE2_N2_OVERLIMIT.MessageLine[1].IsMessageVisable, FWS.EquipmentData.EngineR.fuel);
            }
            #endregion

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