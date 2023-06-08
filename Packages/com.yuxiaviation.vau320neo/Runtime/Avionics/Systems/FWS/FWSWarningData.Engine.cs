using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UdonSharp;


namespace A320VAU.FWS
{
    public partial class FWSWarningData : UdonSharpBehaviour
    {
        private FWSWarningMessageData ENGINE1_FIRE;
        private FWSWarningMessageData ENGINE2_FIRE;

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

        private void SetupEngine()
        {
            ENGINE1_FIRE = GetWarningMessageData(nameof(ENGINE1_FIRE));
            ENGINE2_FIRE = GetWarningMessageData(nameof(ENGINE2_FIRE));
            
            DUAL_ENGINE_FAULT = GetWarningMessageData(nameof(DUAL_ENGINE_FAULT));
            
            ENGINE1_SHUT_DOWN = GetWarningMessageData(nameof(ENGINE1_SHUT_DOWN));
            
            ENGINE1_FAIL = GetWarningMessageData(nameof(ENGINE1_FAIL));
            ENGINE2_FAIL = GetWarningMessageData(nameof(ENGINE2_FAIL));
            
            ENGINE1_EGT_OVERLIMIT = GetWarningMessageData(nameof(ENGINE1_EGT_OVERLIMIT));
            ENGINE2_EGT_OVERLIMIT = GetWarningMessageData(nameof(ENGINE2_EGT_OVERLIMIT));
            
            ENGINE1_N1_OVERLIMIT = GetWarningMessageData(nameof(ENGINE1_N1_OVERLIMIT));
            ENGINE2_N1_OVERLIMIT = GetWarningMessageData(nameof(ENGINE2_N1_OVERLIMIT));
            
            ENGINE1_N2_OVERLIMIT = GetWarningMessageData(nameof(ENGINE1_N2_OVERLIMIT));
            ENGINE2_N2_OVERLIMIT = GetWarningMessageData(nameof(ENGINE2_N2_OVERLIMIT));
        }

        private void MonitorEngine()
        {
            var engine1N1 = FWS.equipmentData.N1LRef * 100;
            var engine2N1 = FWS.equipmentData.N2LRef * 100;

            var engine1N2 = FWS.equipmentData.N2RRef * 100;
            var engine2N2 = FWS.equipmentData.N2RRef * 100;

            #region ENGIEN FIRE
            SetWarnVisible(ref ENGINE1_FIRE.IsVisable, FWS.equipmentData.EngineL.fire, true);
            if (ENGINE1_FIRE.IsVisable)
            {
                SetWarnVisible(ref ENGINE1_FIRE.MessageLine[0].IsMessageVisable, !Mathf.Approximately(FWS.saccAirVehicle.ThrottleInput, FWS.equipmentData.EngineL.idlePoint));
                SetWarnVisible(ref ENGINE1_FIRE.MessageLine[1].IsMessageVisable, FWS.equipmentData.EngineL.fuel);
                SetWarnVisible(ref ENGINE1_FIRE.MessageLine[2].IsMessageVisable, true);
                SetWarnVisible(ref ENGINE1_FIRE.MessageLine[3].IsMessageVisable, true);
            }

            SetWarnVisible(ref ENGINE2_FIRE.IsVisable, FWS.equipmentData.EngineR.fire, true);
            if (ENGINE2_FIRE.IsVisable)
            {
                SetWarnVisible(ref ENGINE2_FIRE.MessageLine[0].IsMessageVisable, !Mathf.Approximately(FWS.saccAirVehicle.ThrottleInput, FWS.equipmentData.EngineR.idlePoint));
                SetWarnVisible(ref ENGINE2_FIRE.MessageLine[1].IsMessageVisable, FWS.equipmentData.EngineR.fuel);
                SetWarnVisible(ref ENGINE2_FIRE.MessageLine[2].IsMessageVisable, true);
                SetWarnVisible(ref ENGINE2_FIRE.MessageLine[3].IsMessageVisable, true);
            }
            #endregion

            #region DUAL ENGINE FAIL
            SetWarnVisible(ref DUAL_ENGINE_FAULT.IsVisable,
                !FWS.saccAirVehicle.Taxiing && !FWS.equipmentData.IsEngineLRunning &&
                !FWS.equipmentData.IsEngineRRunning, true);
            if (DUAL_ENGINE_FAULT.IsVisable)
            {
                SetWarnVisible(ref DUAL_ENGINE_FAULT.MessageLine[0].IsMessageVisable, true);
                SetWarnVisible(ref DUAL_ENGINE_FAULT.MessageLine[1].IsMessageVisable, !Mathf.Approximately(FWS.equipmentData.ThrottleLevelerL, FWS.equipmentData.EngineL.idlePoint));
                SetWarnVisible(ref DUAL_ENGINE_FAULT.MessageLine[2].IsMessageVisable, true);
                SetWarnVisible(ref DUAL_ENGINE_FAULT.MessageLine[3].IsMessageVisable, true);
                SetWarnVisible(ref DUAL_ENGINE_FAULT.MessageLine[4].IsMessageVisable, true);
                SetWarnVisible(ref DUAL_ENGINE_FAULT.MessageLine[5].IsMessageVisable, true);
                SetWarnVisible(ref DUAL_ENGINE_FAULT.MessageLine[6].IsMessageVisable, true);
                SetWarnVisible(ref DUAL_ENGINE_FAULT.MessageLine[7].IsMessageVisable, true);
                SetWarnVisible(ref DUAL_ENGINE_FAULT.MessageLine[8].IsMessageVisable, true);
                SetWarnVisible(ref DUAL_ENGINE_FAULT.MessageLine[9].IsMessageVisable, true);
                SetWarnVisible(ref DUAL_ENGINE_FAULT.MessageLine[10].IsMessageVisable, true);
                SetWarnVisible(ref DUAL_ENGINE_FAULT.MessageLine[11].IsMessageVisable, true);
                SetWarnVisible(ref DUAL_ENGINE_FAULT.MessageLine[12].IsMessageVisable, true);
                SetWarnVisible(ref DUAL_ENGINE_FAULT.MessageLine[13].IsMessageVisable, true);
                SetWarnVisible(ref DUAL_ENGINE_FAULT.MessageLine[14].IsMessageVisable, true);
                SetWarnVisible(ref DUAL_ENGINE_FAULT.MessageLine[15].IsMessageVisable, true);
                SetWarnVisible(ref DUAL_ENGINE_FAULT.MessageLine[16].IsMessageVisable, true);
                SetWarnVisible(ref DUAL_ENGINE_FAULT.MessageLine[17].IsMessageVisable, true);
                SetWarnVisible(ref DUAL_ENGINE_FAULT.MessageLine[18].IsMessageVisable, true);
                SetWarnVisible(ref DUAL_ENGINE_FAULT.MessageLine[19].IsMessageVisable, true);
                SetWarnVisible(ref DUAL_ENGINE_FAULT.MessageLine[20].IsMessageVisable, true);
                SetWarnVisible(ref DUAL_ENGINE_FAULT.MessageLine[21].IsMessageVisable, true);
                SetWarnVisible(ref DUAL_ENGINE_FAULT.MessageLine[22].IsMessageVisable, true);
                SetWarnVisible(ref DUAL_ENGINE_FAULT.MessageLine[23].IsMessageVisable, true);
                SetWarnVisible(ref DUAL_ENGINE_FAULT.MessageLine[24].IsMessageVisable, true);
            }
            #endregion

            #region ENGINE FAIL
            SetWarnVisible(ref ENGINE1_FAIL.IsVisable, !FWS.saccAirVehicle.Taxiing && !FWS.equipmentData.IsEngineLRunning, true);
            if (ENGINE1_FAIL.IsVisable)
            {
                SetWarnVisible(ref ENGINE1_FAIL.MessageLine[0].IsMessageVisable, true);
                SetWarnVisible(ref ENGINE1_FAIL.MessageLine[1].IsMessageVisable, !Mathf.Approximately(FWS.equipmentData.ThrottleLevelerL, FWS.equipmentData.EngineL.idlePoint));
                SetWarnVisible(ref ENGINE1_FAIL.MessageLine[2].IsMessageVisable, true);
                SetWarnVisible(ref ENGINE1_FAIL.MessageLine[3].IsMessageVisable, true);
                SetWarnVisible(ref ENGINE1_FAIL.MessageLine[4].IsMessageVisable, true);
                SetWarnVisible(ref ENGINE1_FAIL.MessageLine[5].IsMessageVisable, true);
                SetWarnVisible(ref ENGINE1_FAIL.MessageLine[6].IsMessageVisable, true);
                SetWarnVisible(ref ENGINE1_FAIL.MessageLine[7].IsMessageVisable, true);
            }

            SetWarnVisible(ref ENGINE2_FAIL.IsVisable, !FWS.saccAirVehicle.Taxiing && !FWS.equipmentData.IsEngineRRunning, true);
            if (ENGINE2_FAIL.IsVisable)
            {
                SetWarnVisible(ref ENGINE2_FAIL.MessageLine[0].IsMessageVisable, true);
                SetWarnVisible(ref ENGINE2_FAIL.MessageLine[1].IsMessageVisable, !Mathf.Approximately(FWS.equipmentData.ThrottleLevelerR, FWS.equipmentData.EngineR.idlePoint));
                SetWarnVisible(ref ENGINE2_FAIL.MessageLine[2].IsMessageVisable, true);
                SetWarnVisible(ref ENGINE2_FAIL.MessageLine[3].IsMessageVisable, true);
                SetWarnVisible(ref ENGINE2_FAIL.MessageLine[4].IsMessageVisable, true);
                SetWarnVisible(ref ENGINE2_FAIL.MessageLine[5].IsMessageVisable, true);
                SetWarnVisible(ref ENGINE2_FAIL.MessageLine[6].IsMessageVisable, true);
                SetWarnVisible(ref ENGINE2_FAIL.MessageLine[7].IsMessageVisable, true);
            }
            #endregion

            #region EGT Overlimit
            SetWarnVisible(ref ENGINE1_EGT_OVERLIMIT.IsVisable, FWS.equipmentData.EGTL > 1060f, true);
            if (ENGINE1_EGT_OVERLIMIT.IsVisable)
            {
                SetWarnVisible(ref ENGINE1_EGT_OVERLIMIT.MessageLine[0].IsMessageVisable, !Mathf.Approximately(FWS.equipmentData.ThrottleLevelerL, FWS.equipmentData.EngineL.idlePoint));
                SetWarnVisible(ref ENGINE1_EGT_OVERLIMIT.MessageLine[1].IsMessageVisable, FWS.equipmentData.EngineL.fuel);
            }

            SetWarnVisible(ref ENGINE2_EGT_OVERLIMIT.IsVisable, FWS.equipmentData.EGTR > 1060f, true);
            if (ENGINE2_EGT_OVERLIMIT.IsVisable)
            {
                SetWarnVisible(ref ENGINE2_EGT_OVERLIMIT.MessageLine[0].IsMessageVisable, !Mathf.Approximately(FWS.equipmentData.ThrottleLevelerR, FWS.equipmentData.EngineR.idlePoint));
                SetWarnVisible(ref ENGINE2_EGT_OVERLIMIT.MessageLine[1].IsMessageVisable, FWS.equipmentData.EngineR.fuel);
            }
            #endregion

            #region N1 OVERLIMIT
            SetWarnVisible(ref ENGINE1_N1_OVERLIMIT.IsVisable, engine1N1 > 100f, true);
            if (ENGINE1_N1_OVERLIMIT.IsVisable)
            {
                SetWarnVisible(ref ENGINE1_N1_OVERLIMIT.MessageLine[0].IsMessageVisable, !Mathf.Approximately(FWS.equipmentData.ThrottleLevelerL, FWS.equipmentData.EngineL.idlePoint));
                SetWarnVisible(ref ENGINE1_N1_OVERLIMIT.MessageLine[1].IsMessageVisable, FWS.equipmentData.EngineL.fuel);
            }

            SetWarnVisible(ref ENGINE2_N1_OVERLIMIT.IsVisable, engine2N1 > 100f, true);
            if (ENGINE2_N1_OVERLIMIT.IsVisable)
            {
                SetWarnVisible(ref ENGINE2_N1_OVERLIMIT.MessageLine[0].IsMessageVisable, !Mathf.Approximately(FWS.equipmentData.ThrottleLevelerR, FWS.equipmentData.EngineR.idlePoint));
                SetWarnVisible(ref ENGINE2_N1_OVERLIMIT.MessageLine[1].IsMessageVisable, FWS.equipmentData.EngineR.fuel);
            }
            #endregion

            #region N2 OVERLIMIT
            SetWarnVisible(ref ENGINE1_N2_OVERLIMIT.IsVisable, engine1N2 > 100f, true);
            if (ENGINE1_N2_OVERLIMIT.IsVisable)
            {
                SetWarnVisible(ref ENGINE1_N2_OVERLIMIT.MessageLine[0].IsMessageVisable, !Mathf.Approximately(FWS.equipmentData.ThrottleLevelerL, FWS.equipmentData.EngineL.idlePoint));
                SetWarnVisible(ref ENGINE1_N2_OVERLIMIT.MessageLine[1].IsMessageVisable, FWS.equipmentData.EngineL.fuel);
            }

            SetWarnVisible(ref ENGINE2_N2_OVERLIMIT.IsVisable, engine2N2 > 100f, true);
            if (ENGINE2_N2_OVERLIMIT.IsVisable)
            {
                SetWarnVisible(ref ENGINE2_N2_OVERLIMIT.MessageLine[0].IsMessageVisable, !Mathf.Approximately(FWS.equipmentData.ThrottleLevelerR, FWS.equipmentData.EngineR.idlePoint));
                SetWarnVisible(ref ENGINE2_N2_OVERLIMIT.MessageLine[1].IsMessageVisable, FWS.equipmentData.EngineR.fuel);
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