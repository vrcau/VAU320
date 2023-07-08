using A320VAU.Avionics;
using A320VAU.Brake;
using A320VAU.DFUNC;
using A320VAU.SFEXT;
using EsnyaSFAddons.DFUNC;
using EsnyaSFAddons.SFEXT;
using SaccFlightAndVehicles;
using UdonSharp;
using UnityEngine;
using YuxiFlightInstruments.BasicFlightData;
using YuxiFlightInstruments.Navigation;

namespace A320VAU.ECAM {
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class ECAMDataInterface : UdonSharpBehaviour {
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

        [Header("Flaps")]
        public DFUNC_AdvancedFlaps Flap;

        [Header("APU")]
        public SFEXT_AuxiliaryPowerUnit APU;

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

        //synced targetAngle actuatorBroken _wingBroken
        public float FlapRefAngle => Flap.angle / Flap.maxAngle;

        public int FlapTargetPosition => Flap.targetDetentIndex;

        public bool FlapInPosition => Mathf.Approximately(Flap.angle, Flap.targetAngle);

        public bool IsAPURunning =>
            //get => APU.started;
            Mathf.Approximately(APU.apuAudioSource.volume, 1.0f);

        //synced float n1 n2 egt ect ff throttleLeveler 
        //synced bool reversing, starter, fuel，fire

    #region Eng1 params

        public float N1LRef => EngineL.n1 / EngineL.takeOffN1;

        public float N2LRef => EngineL.n2 / EngineL.takeOffN2;

        public float EGTL => EngineL.egt;

        public float FuelFlowL => Mathf.Round(EngineL.ff / 20) * 20;

        public bool IsEngineLStarting => EngineL.starter;

        public bool Reversing => EngineL.reversing; //判断反推：reversing

        public float ThrottleLevelerL => EngineL.throttleLeveler;

        public float TargetRefN1L =>
            !Reversing
                ? (ThrottleLevelerL - EngineL.idlePoint) / (1 - EngineL.idlePoint)
                : (EngineL.idlePoint - ThrottleLevelerL) / (1 - EngineL.idlePoint);

        public bool IsEngineLRunning => EngineL.fuel && EngineL.n1 > 0.63f * EngineL.idleN1 && !EngineL.stall;

        public bool IsEngineLStall => EngineL.stall;

    #endregion

    #region Eng2 params

        public float N1RRef => EngineR.n1 / EngineR.takeOffN1;

        public float N2RRef => EngineR.n2 / EngineR.takeOffN2;

        public float EGTR => EngineR.egt;

        public float FuelFlowR => Mathf.Round(EngineR.ff / 20) * 20;

        public bool IsEngineRStarting => EngineR.starter;

        public bool ReversingR => EngineR.reversing; //判断反推：reversing

        public float ThrottleLevelerR => EngineR.throttleLeveler; //用于判断起飞构型

        public float TargetRefN1R =>
            !Reversing
                ? (ThrottleLevelerR - EngineR.idlePoint) / (1 - EngineR.idlePoint)
                : (EngineR.idlePoint - ThrottleLevelerR) / (1 - EngineR.idlePoint);

        public bool IsEngineRRunning => EngineR.fuel && EngineR.n1 > 0.63f * EngineR.idleN1 && !EngineR.stall;

        public bool EngineRStall => EngineR.stall;

    #endregion
    }
}