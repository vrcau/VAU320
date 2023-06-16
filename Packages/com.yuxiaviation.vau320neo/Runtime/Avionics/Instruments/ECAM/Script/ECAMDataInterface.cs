using System;
using A320VAU.Brake;
using A320VAU.ECAM;
using A320VAU.Avionics;
using EsnyaSFAddons.DFUNC;
using EsnyaSFAddons.SFEXT;
using SaccFlightAndVehicles;
using UdonSharp;
using UnityEngine;
using A320VAU.DFUNC;
using VRC.SDKBase;
using VRC.Udon;
using YuxiFlightInstruments.BasicFlightData;
using YuxiFlightInstruments.Navigation;
using A320VAU.SFEXT;

namespace A320VAU.ECAM
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public partial class ECAMDataInterface : UdonSharpBehaviour
    {
        /*
         写作ECAMDataInterface，但是接下来所有设备的参数建议都放在这并且从这里访问，例如发动机是否启动，是否起火，起落架状态
        尽量以最少的网络同步量与Update把所有需要的extension参数同步地整到手，特别是ESFA里面的私有成员
        应该有完善的调试功能，打印自身状态

        为了便于维护，开发组件时请按以下顺序查找机上设备的相关变量
        ECAMDataInterface
        BasicFlightData
        SaccAirVehicle
         */

        public SaccAirVehicle airVehicle;
        public SaccEntity EntityControl;
        public YFI_FlightDataInterface BasicData;
        [Header("Engines")]
        public SFEXT_a320_AdvancedEngine EngineL;
        public SFEXT_a320_AdvancedEngine EngineR;
        //synced float n1 n2 egt ect ff throttleLeveler 
        //synced bool reversing, starter, fuel，fire
        #region Eng1 params
        public float N1LRef
        {
            get => EngineL.n1 / EngineL.takeOffN1;
        }
        public float N2LRef
        {
            get => EngineL.n2 / EngineL.takeOffN2;
        }
        public float EGTL
        {
            get => EngineL.egt;
        }
        public float FuelFlowL
        {
            get => (Mathf.Round(EngineL.ff / 20) * 20);
        }
        public bool IsEngineLStarting
        {
            get => EngineL.starter;
        }
        public bool Reversing //判断反推：reversing
        {
            get => EngineL.reversing;
        }
        public float ThrottleLevelerL
        {
            get => EngineL.throttleLeveler;
        }

        public float TargetRefN1L
        {
        get => (!Reversing) ?
                (ThrottleLevelerL - EngineL.idlePoint)/(1 - EngineL.idlePoint)
                : (EngineL.idlePoint - ThrottleLevelerL)/(1 - EngineL.idlePoint);
        }
        public bool IsEngineLRunning
        {
            get => EngineL.fuel && EngineL.n1 > 0.63f * EngineL.idleN1 && !EngineL.stall;
        }
        public bool IsEngineLStall
        {
            get => EngineL.stall;
        }
        #endregion

        #region Eng2 params
        public float N1RRef
        {
            get => EngineR.n1 / EngineR.takeOffN1;
        }
        public float N2RRef
        {
            get => EngineR.n2 / EngineR.takeOffN2;
        }
        public float EGTR
        {
            get => EngineR.egt;
        }
        public float FuelFlowR
        {
            get => (Mathf.Round(EngineR.ff / 20) * 20);
        }
        public bool IsEngineRStarting
        {
            get => EngineR.starter;
        }
        public bool ReversingR //判断反推：reversing
        {
            get => EngineR.reversing;
        }
        public float ThrottleLevelerR//用于判断起飞构型
        {
            get => EngineR.throttleLeveler;
        }
        public float TargetRefN1R
        {
            get => (!Reversing) ?
                    (ThrottleLevelerR - EngineR.idlePoint) / (1 - EngineR.idlePoint)
                    : (EngineR.idlePoint - ThrottleLevelerR) / (1 - EngineR.idlePoint);
        }
        public bool IsEngineRRunning
        {
            get => EngineR.fuel && EngineR.n1 > 0.63f * EngineR.idleN1 && !EngineR.stall;
        }
        public bool EngineRStall
        {
            get => EngineR.stall;
        }
        #endregion

        [Header("Flaps")]
        public DFUNC_AdvancedFlaps Flap;
        //synced targetAngle actuatorBroken _wingBroken
        public float FlapRefAngle
        {
            get => Flap.angle / Flap.maxAngle;
        }

        public int FlapTargetPosition
        {
            get => Flap.targetDetentIndex;
        }

        public bool FlapInPosition
        {
            get => Mathf.Approximately( Flap.angle, Flap.targetAngle);
        }

        [Header("APU")]
        public SFEXT_AuxiliaryPowerUnit APU;
        public bool IsAPURunning
        {
            //get => APU.started;
            get => Mathf.Approximately(APU.apuAudioSource.volume, 1.0f);
        }

        [Header("Cabin Door")]

        [Header("Gear")]
        public SFEXT_a320_AdvancedGear GearLeft;
        public SFEXT_a320_AdvancedGear GearRight;
        public SFEXT_a320_AdvancedGear GearNose;

        [Header("Other")]
        public DFUNC_a320_Brake Brake;

        public DFUNC_a320_LandingLight LandingLight;
        public DFUNC_Canopy Canopy;

        public YFI_NavigationReceiver NavigationReceiver1;
        public YFI_NavigationReceiver NavigationReceiver2;
        public GPWS_OWML GPWS;

        public DFUNC_ElevatorTrim ElevatorTrim;

    }
}
