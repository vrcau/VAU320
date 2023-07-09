using A320VAU.Brake;
using A320VAU.Common;
using A320VAU.SFEXT;
using EsnyaSFAddons.DFUNC;
using EsnyaSFAddons.SFEXT;
using JetBrains.Annotations;
using SaccFlightAndVehicles;
using UdonSharp;
using UnityEngine;

namespace Avionics.Systems.Common {
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class AircraftSystemData : UdonSharpBehaviour {
        /*
         写作ECAMDataInterface，但是接下来所有设备的参数建议都放在这并且从这里访问，例如发动机是否启动，是否起火，起落架状态
        尽量以最少的网络同步量与Update把所有需要的extension参数同步地整到手，特别是ESFA里面的私有成员
        应该有完善的调试功能，打印自身状态

        为了便于维护，开发组件时请按以下顺序查找机上设备的相关变量
        ECAMDataInterface
        BasicFlightData
        SaccAirVehicle
         */

        private DependenciesInjector _dependenciesInjector;

        private SFEXT_a320_AdvancedEngine EngineL;
        private SFEXT_a320_AdvancedEngine EngineR;

        private DFUNC_AdvancedFlaps Flap;
        private SFEXT_AuxiliaryPowerUnit APU;

        private SFEXT_a320_AdvancedGear LeftLandingGear;
        private SFEXT_a320_AdvancedGear RightLandingGear;
        private SFEXT_a320_AdvancedGear CenterLandingGear;
        private DFUNC_a320_Brake Brake;

        private DFUNC_Canopy Canopy;

        //synced targetAngle actuatorBroken _wingBroken

    #region Flaps
        [PublicAPI] public float flapAngle => Flap.angle / Flap.maxAngle;
        [PublicAPI] public int flapCurrentIndex => Flap.detentIndex;
        [PublicAPI] public int flapTargetIndex => Flap.targetDetentIndex;
        [PublicAPI] public bool flapInPosition => Mathf.Approximately(Flap.angle, Flap.targetAngle);

        [PublicAPI] public float flapCurrentSpeedLimit => Flap.speedLimit;
        [PublicAPI] public float flapTargetSpeedLimit => Flap.targetSpeedLimit;
    #endregion

    #region Gears
        [PublicAPI] public bool IsGearsDown => Mathf.Approximately(LeftLandingGear.targetPosition, 1f) &&
                                               Mathf.Approximately(CenterLandingGear.targetPosition, 1f) &&
                                               Mathf.Approximately(RightLandingGear.targetPosition, 1f);

        [PublicAPI] public bool IsGearsInTransition =>
            Mathf.Approximately(LeftLandingGear.position, LeftLandingGear.targetPosition) &&
            Mathf.Approximately(CenterLandingGear.position, CenterLandingGear.targetPosition) &&
            Mathf.Approximately(RightLandingGear.position, RightLandingGear.targetPosition);

        [PublicAPI] public bool IsGearsDownLock => IsGearsDown && !IsGearsInTransition;
    #endregion

        [PublicAPI] public bool isCabinDoorOpen => Canopy.CanopyOpen;
        [PublicAPI] public bool isParkBreakSet => Brake.ParkBreakSet;
        [PublicAPI] public bool isAPURunning =>
            //get => APU.started;
            Mathf.Approximately(APU.apuAudioSource.volume, 1.0f);

        //synced float n1 n2 egt ect ff throttleLeveler 
        //synced bool reversing, starter, fuel，fire

        [PublicAPI] public bool isBothThrottleLevelerIdle =>
            isEngine1ThrottleLevelerIdle && isEngine2ThrottleLevelerIdle;
        
    #region ENG1 Params
        [PublicAPI] public bool isEngine1Avail => EngineL.n1 > 0.9f * EngineL.idleN1;
        [PublicAPI] public float engine1n1 => EngineL.n1 / EngineL.takeOffN1;
        [PublicAPI] public float engine1n2 => EngineL.n2 / EngineL.takeOffN2;
        [PublicAPI] public float engine1EGT => EngineL.egt;
        [PublicAPI] public float engine1fuelFlow => Mathf.Round(EngineL.ff / 20) * 20;

        [PublicAPI] public bool isEngine1Starting => EngineL.starter;
        [PublicAPI] public bool isEngine1Reversing => EngineL.reversing; //判断反推：reversing
        [PublicAPI] public float engine1ThrottleLeveler => EngineL.throttleLeveler;

        [PublicAPI] public bool isEngine1ThrottleLevelerIdle =>
            Mathf.Approximately(engine1ThrottleLeveler, EngineL.idlePoint);

        [PublicAPI] public float engine1TargetN1 =>
            !isEngine1Reversing
                ? (engine1ThrottleLeveler - EngineL.idlePoint) / (1 - EngineL.idlePoint)
                : (EngineL.idlePoint - engine1ThrottleLeveler) / (1 - EngineL.idlePoint);

        [PublicAPI] public bool isEngine1Running => EngineL.fuel && EngineL.n1 > 0.63f * EngineL.idleN1 && !EngineL.stall;

        [PublicAPI] public bool isEngine1Stall => EngineL.stall;
        [PublicAPI] public bool isEngine1Fire => EngineL.fire;
        [PublicAPI] public bool isEngine1Fuel => EngineL.fuel;
    #endregion

    #region ENG2 params
        [PublicAPI] public bool isEngine2Avail => EngineR.n1 > 0.9f * EngineR.idleN1;
        [PublicAPI] public float engine2n1 => EngineR.n1 / EngineR.takeOffN1;
        [PublicAPI] public float engine2n2 => EngineR.n2 / EngineR.takeOffN2;
        [PublicAPI] public float engine2EGT => EngineR.egt;
        [PublicAPI] public float engine2fuelFlow => Mathf.Round(EngineR.ff / 20) * 20;

        [PublicAPI] public bool isEngine2Starting => EngineR.starter;
        [PublicAPI] public bool isEngine2Reversing => EngineR.reversing; //判断反推：reversing
        [PublicAPI] public float engine2ThrottleLeveler => EngineR.throttleLeveler;
        [PublicAPI] public bool isEngine2ThrottleLevelerIdle =>
            Mathf.Approximately(engine2ThrottleLeveler, EngineR.idlePoint);

        [PublicAPI] public float engine2TargetN1 =>
            !isEngine2Reversing
                ? (engine2ThrottleLeveler - EngineR.idlePoint) / (1 - EngineR.idlePoint)
                : (EngineR.idlePoint - engine2ThrottleLeveler) / (1 - EngineR.idlePoint);

        [PublicAPI] public bool isEngine2Running => EngineR.fuel && EngineR.n1 > 0.63f * EngineR.idleN1 && !EngineR.stall;

        [PublicAPI] public bool isEngine2Stall => EngineR.stall;
        [PublicAPI] public bool isEngine2Fire => EngineR.fire;
        [PublicAPI] public bool isEngine2Fuel => EngineR.fuel;
    #endregion

        private void Start() {
            _dependenciesInjector = DependenciesInjector.GetInstance(this);

            EngineL = _dependenciesInjector.engine1;
            EngineR = _dependenciesInjector.engine2;

            Flap = _dependenciesInjector.flaps;
            APU = _dependenciesInjector.apu;

            LeftLandingGear = _dependenciesInjector.leftLadingGear;
            RightLandingGear = _dependenciesInjector.rightLadingGear;
            CenterLandingGear = _dependenciesInjector.frontLadingGear;

            Brake = _dependenciesInjector.brake;

            Canopy = _dependenciesInjector.canopy;
        }
    }
}