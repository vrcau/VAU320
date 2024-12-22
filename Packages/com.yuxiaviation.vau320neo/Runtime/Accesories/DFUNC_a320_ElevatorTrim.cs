using System;
using System.Diagnostics.Eventing.Reader;
using SaccFlightAndVehicles;
//using Serilog.Filters;
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using YuxiFlightInstruments.BasicFlightData;

//note:this code is original from https://github.com/esnya/EsnyaSFAddons
//to satisfy vau320's demand, add autotrim
//to optimize change vellift in SAV to trim
//2024-09-29 尝试一个新东西，先把这个脚本作用在JoystickOverridde上，（FBW？）
namespace A320VAU.DFUNC {
    [UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
    public class DFUNC_a320_ElevatorTrim : UdonSharpBehaviour {

        public YFI_FlightDataInterface BasicFlightData;
        public RadioAltimeter.RadioAltimeter radioAltimeter;
        [Header("配平输入")]
        //[Tooltip("最大配平强度，作用于velLift变量，对于320而言就是10 （-10-10，-10时机头会稍微往下掉）")]
        //[Range(0, 50)] public float trimStrength = 10;
        //[Tooltip("配平强度偏置，最终的VelLift =trimStrength *x +  trimBias")]
        //[Range(0, 50)] public float trimBias = 8;
        
        private float prevTrim;
        
        public float initialTrim = -0.1f;
        [UdonSynced] public float trim;//当前配平位置，-1~1
        public float critiaclAOA = 20f;//临界攻角，sav.pitchaoa低于该数值时，触发afloorProtect;

        [Header("controller")]
        public float targetLoadFactor = 1;
        public float targetAoa = 0;

        public float TrimError = 0;
        private float TrimErrorLastFrame = 0;
        public float TrimErrorIntergrate = 0;
        public float TrimErrorDerivative = 0;
        
        public float kp = 0.04f; //载荷系数控制率 0.6 0.015 俯仰角控制率 0.02 0.001
        public float ki = 0.0015f;
        public float kd = 0.0001f;

        [Header("animation")]
        public string animatorParameterName = "elevtrim";

        [Header("Haptics")]
        public Vector3 vrInputAxis = Vector3.forward;
        [Range(0, 1)] public float hapticDuration = 0.2f;
        [Range(0, 1)] public float hapticAmplitude = 0.5f;
        [Range(0, 1)] public float hapticFrequency = 0.1f;

        [Header("Debug")]
        public Transform debugControllerTransform;
        [Tooltip("0-直接法则 1-飞行模式 2-地面模式 3-拉平模式")]
        public int trimMode = 1; //0-直接法则 1-飞行模式 2-地面模式 3-拉平模式
        public bool TrimActive = true; //当侧杆(SFEXT_O_JoystickGrabbed/SFEXT_O_JoystickDropped)以及AP(JoystickOverride)无输入时，配平才激活
        public bool TrimActiveLastFrame = false;
        public bool afloorProtect = false;

        public Vector3 FBWRotationInputs;

        private void ResetStatus() {
            //自动配平默认开启
            trimMode = 0;
            Dial_Funcon.SetActive(TrimActive);
            prevTrim = trim = initialTrim;
            if (vehicleAnimator) vehicleAnimator.SetFloat(animatorParameterName, .5f);
            //SAVControl.SetProgramVariable("VelLiftStart", trimStrength* trim + trimBias);
            vehicleRigidbody = SAVControl.VehicleRigidbody;
            TrimError = 0;
            TrimErrorIntergrate = 0;
            TrimErrorDerivative = 0;
            TrimErrorLastFrame = 0;
            targetAoa = 0;
    }

        private void PilotUpdate() {

            /*检查是否有输入
            //当杆量输入小于特定数值时，开始执行配平
            //当前键盘是否输入
            var Wi = Input.GetKey(KeyCode.W); 
            var Si = Input.GetKey(KeyCode.S);
            var Ai = Input.GetKey(KeyCode.A);
            var Di = Input.GetKey(KeyCode.D);
            var upi = Input.GetKey(KeyCode.UpArrow);
            var downi = Input.GetKey(KeyCode.DownArrow);
            var lefti = Input.GetKey(KeyCode.LeftArrow);
            var righti = Input.GetKey(KeyCode.RightArrow);
            var isKeyboardInput = Wi || Si || Ai || Di || upi || downi || lefti || righti;

            //当前VR摇杆是否输入
            float JoyStickGrip;
            if (SAVControl.SwitchHandsJoyThrottle) {
                JoyStickGrip = Input.GetAxisRaw("Oculus_CrossPlatform_PrimaryHandTrigger"); 
            }
            else {
                JoyStickGrip = Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryHandTrigger");
            }
            var isVRGrabInput = !Mathf.Approximately(JoyStickGrip,0f);

            //当前外设摇杆是否输入（待验证）
            var LStickPos = Vector2.zero;
            LStickPos.x = Input.GetAxisRaw("Oculus_CrossPlatform_PrimaryThumbstickHorizontal");
            LStickPos.y = Input.GetAxisRaw("Oculus_CrossPlatform_PrimaryThumbstickVertical");
            var isFlightStickInput = (!SAVControl.InVR) && (!Mathf.Approximately(LStickPos.x, 0f) || !Mathf.Approximately(LStickPos.y, 0f));

            var hasStickInput = isKeyboardInput || isVRGrabInput || isFlightStickInput;

            //var SAVJoystickOverridden = (int)SAVControl.GetProgramVariable("JoystickOverridden");
            if (hasStickInput != hasStickInputLastFrame) {
                if (hasStickInput) {
                    //正在通过摇杆操纵
                    TrimActive = false;
                    trim = initialTrim;
                    //if (SAVJoystickOverridden > 0)
                        //SAVControl.SetProgramVariable("JoystickOverridden", SAVJoystickOverridden - 1);
                }
                else {
                    //if (hasStickInputLastFrame  && Mathf.Abs(BasicFlightData.verticalG - 1) < 0.7f ) {
                    TrimActive = true;
                    //SAVControl.SetProgramVariable("JoystickOverridden", (int)SAVControl.GetProgramVariable("JoystickOverridden") + 1);
                    //targetPitch = BasicFlightData.pitch;
                    trim = initialTrim;
                    TrimError = 0;
                    TrimErrorIntergrate = 0;  
                    //}
                }
            }
            hasStickInputLastFrame = hasStickInput;
            */
            //if (SAVControl.Taxiing) {
            //TrimActive = false;
            //if (SAVJoystickOverridden > 0)
            //SAVControl.SetProgramVariable("JoystickOverridden", SAVJoystickOverridden - 1);
            //}

            //计算配平值
            float DeltaTime = Time.deltaTime;

            //if (TrimActive && autoTrim == 1 && !SAVControl.Taxiing) {
            //    TrimError = (targetPitch - BasicFlightData.pitch);//俯仰角控制率
            //    //TrimError = (1f - BasicFlightData.verticalG); //载荷因数控制率 (对于键盘过于不友好（一般玩家的操作习惯松开按键时载荷因数一般都还没稳定呢))
            //    TrimErrorIntergrate = Mathf.Clamp(TrimError * DeltaTime + TrimErrorIntergrate, -10, 10);//处理积分饱和
            //    TrimErrorDerivative = (TrimError - TrimErrorLastFrame) / DeltaTime;
            //    //trim = Mathf.MoveTowards(trim, Mathf.Clamp(kp * TrimError + ki * TrimErrorIntergrate, -1, 1), 0.1f);
            //    trim = Mathf.Clamp(kp * TrimError + ki * TrimErrorIntergrate + kd * TrimErrorDerivative, -1, 1);
            //    var GLimitStrength = Mathf.Clamp(-((BasicFlightData.verticalG) / 3f) + 1, 0, 1);
            //    trim *= GLimitStrength;
            //    TrimErrorLastFrame = TrimError;
            //}
            var pitchInputs = SAVControl.RotationInputs.x;

            //飞行模式
            if (TrimActive &&
                !SAVControl.Taxiing &&
                radioAltimeter.radioAltitude >= 50 ) {
                //if (SAVControl.AngleOfAttack < SAVControl.MaxAngleOfAttackPitch) {
                //不知道为啥不好使
                //    Debug.Log(SAVControl.AngleOfAttack);
                //    Debug.Log(SAVControl.MaxAngleOfAttackPitch);
                //    trimMode = 0;
                //    trim = 0;
                //}
                //else {
                if (trimMode != 1) {
                    trimMode = 1;
                    TrimError = TrimErrorIntergrate = TrimErrorDerivative = 0f;
                    Debug.Log("[FBW]Flight Mode");
                }
                
                if(SAVControl.JoystickOverridden != 0) {
                    trim = initialTrim;
                }
                else { 
                    if (SAVControl.AngleOfAttackPitch < critiaclAOA) {
                        afloorProtect = false;
                        targetLoadFactor = StickInputtoLoadFactor(pitchInputs, DeltaTime);
                        TrimError = (targetLoadFactor - BasicFlightData.verticalG);
                        TrimErrorIntergrate = Mathf.Clamp(TrimError * DeltaTime + TrimErrorIntergrate, -1, 1);//处理积分饱和
                        TrimErrorDerivative = (TrimError - TrimErrorLastFrame) / DeltaTime;
                    }
                    else {
                        afloorProtect = true;
                        targetAoa = StickInputtoAoa(pitchInputs, DeltaTime);
                        TrimError = targetAoa - BasicFlightData.AOAPitch;//todo:afloor控制律
                        TrimErrorIntergrate = 0;
                        TrimErrorDerivative = 0;
                    }
                
                    //trim = Mathf.MoveTowards(trim, Mathf.Clamp(kp * TrimError + ki * TrimErrorIntergrate, -1, 1), 0.1f);
                    trim = Mathf.Clamp(kp * TrimError + ki * TrimErrorIntergrate + kd * TrimErrorDerivative, -1, 1);
                    TrimErrorLastFrame = TrimError;
                }

            }

            //地面模式
            else if (TrimActive && SAVControl.Taxiing) {
                if (trimMode != 2) {
                    trimMode = 2;
                    TrimError = TrimErrorIntergrate = TrimErrorDerivative = 0f;
                    Debug.Log("[FBW]Ground Mode");
                }
                trim = initialTrim;
                targetLoadFactor = StickInputtoLoadFactor(pitchInputs, DeltaTime);
            }

            //拉平模式
            else if (TrimActive &&
                radioAltimeter.radioAltitude < 50 &&
                !SAVControl.Taxiing &&
                BasicFlightData.verticalSpeed < -0.6 &&
                SAVControl.JoystickOverridden == 0) {

                var targetTrim = initialTrim;
                if (trimMode != 3) {
                    trimMode = 3;
                    Debug.Log("[FBW]Touchdown Mode");
                    targetTrim = trim - 0.05f;
                }
                //俯仰角控制率
                /*
                targetPitch = -2f;
                TrimError = (targetPitch - BasicFlightData.pitch);
                TrimErrorIntergrate += TrimError;
                TrimErrorDerivative = (TrimError - TrimErrorLastFrame) / DeltaTime;
                trim = Mathf.Clamp(kp * TrimError + ki * TrimErrorIntergrate + kd * TrimErrorDerivative, -1, 1);
                */
                TrimError = TrimErrorIntergrate = TrimErrorDerivative = 0f;
                trim = Mathf.MoveTowards(trim, targetTrim, DeltaTime * 0.025f);
            }
            
            //手动配平
            else if (!TrimActive) {
                var input = GetSliderInput();
                trim = Mathf.Clamp(trim + input, -1, 1);
                if (!Mathf.Approximately(input, 0) &&
                    Time.frameCount % Mathf.FloorToInt(hapticDuration / Time.fixedDeltaTime) == 0) PlayHapticEvent();
            }
            

        }

        private float StickInputtoLoadFactor(float pitchInputs, float deltaTime) {
            var maxLoad = 2f;
            var minLoad = 0f;

            var maxLoadRate = 1f * deltaTime;//每秒最大变化1g
            //SAV脚本中已经做了输入的平方化，所以这里直接线性转换为载荷目标
            if (pitchInputs > 0.01) {//推杆 
                targetLoadFactor = Mathf.MoveTowards(targetLoadFactor,
                   (minLoad - 1f) * Mathf.Pow(pitchInputs, 2) + 1,
                    maxLoadRate);
            }
            else if ((pitchInputs < -0.01)) {//拉杆 
                targetLoadFactor = Mathf.MoveTowards(targetLoadFactor,
                     (maxLoad - 1f) * Mathf.Pow(pitchInputs, 2) + 1,
                    maxLoadRate);
            }
            else {
                targetLoadFactor = Mathf.MoveTowards(targetLoadFactor, 1, maxLoadRate);
            }
            if (trimMode != 1)
                //飞行模式以外给一个比较小的载荷目标约束，避免离地时模式切换导致配平位置突变
                return Mathf.Clamp(targetLoadFactor, 1f - 0.5f, 1f + 0.5f);
            else
                return targetLoadFactor;
        }

        private float StickInputtoAoa(float pitchInputs, float deltaTime) {
           
            var maxLoadRate = 10f * deltaTime;//每秒最大变化1单位
            //SAV脚本中已经做了输入的平方化，所以这里直接线性转换为载荷目标
            if (pitchInputs > 0.01) {//推杆 
                targetAoa = Mathf.MoveTowards(targetAoa,
                    -(critiaclAOA) * Mathf.Pow(pitchInputs, 2),
                    maxLoadRate);
            }
            else if ((pitchInputs < -0.01)) {//拉杆
                targetAoa = Mathf.MoveTowards(targetAoa,
                    (critiaclAOA) * Mathf.Pow(pitchInputs, 2),
                    maxLoadRate);
            }
            else {
                targetAoa = Mathf.MoveTowards(targetAoa, 1, maxLoadRate);
            }
            //if (trimMode != 1) return Mathf.Clamp(targetAoa, -3, 3);
            //else
                return targetAoa;
        }
        
        private void LocalUpdate() {
            var trimChanged = !Mathf.Approximately(trim, prevTrim);
            prevTrim = trim;
            if (trimChanged) {
                SetDirty();
                if (vehicleAnimator) vehicleAnimator.SetFloat(animatorParameterName, Remap01(trim, -1, 1));
                //SAVControl.SetProgramVariable("VelLiftStart", trim * trimStrength + trimBias);
                DebugOut.text = "FBW[WIP]\n[F6]\n" + (trim).ToString("f2") + (TrimActive ? "\nAuto": "\n");
            }
        }

        private void FixedUpdate() {
            if (!isOwner) return;

            var rotlift = Mathf.Clamp(SAVControl.AirSpeed / rotMultiMaxSpeed, -1, 1);
            //var DeltaTime = Time.fixedDeltaTime;
            vehicleRigidbody.AddForceAtPosition((trim * SAVControl.PitchStrength) * rotlift * SAVControl.Atmosphere * -transform.up, transform.position, ForceMode.Force);
            
            //尝试了不同的作用力方式
            //1.改VelLiftStart
            //SAVControl.SetProgramVariable("VelLiftStart", trim * trimStrength + trimBias);
            //2.AddForceAtPosition

            //Vector3 trimPitching = Vector3.zero;
            //trim *= SAVControl.PitchStrength;
            //trim *= rotlift * Mathf.Min(SAVControl.AoALiftPitch, SAVControl.AoALiftYaw);
            //var downspeed = -Vector3.Dot(SAVControl.AirVel, SAVControl.VehicleTransform.up);
            //trimPitching = ((((SAVControl.VehicleTransform.up * trim) + (SAVControl.VehicleTransform.up * downspeed * SAVControl.VelStraightenStrPitch * SAVControl.AoALiftPitch * rotlift)) * SAVControl.Atmosphere));
            //vehicleRigidbody.AddForceAtPosition(trimPitching, transform.position, ForceMode.Force);//deltatime is built into ForceMode.Force

            //3.写JoystickOverride
            //FBWRotationInputs.x = Mathf.Clamp(trim, -1, 1);
            //FBWRotationInputs.y = 0;
            //FBWRotationInputs.z = 0;
            //SAVControl.SetProgramVariable("JoystickOverride", FBWRotationInputs);
        }
        public void TrimUp() {
            trim += desktopStep;
        }

        public void TrimDown() {
            trim -= desktopStep;
        }

        private void PlayHapticEvent() {
            var hand = trackingTarget == VRCPlayerApi.TrackingDataType.LeftHand
                ? VRC_Pickup.PickupHand.Left
                : VRC_Pickup.PickupHand.Right;
            Networking.LocalPlayer.PlayHapticEventInHand(hand, hapticDuration, hapticAmplitude, hapticFrequency);
        }

        private void ToggleAutoTrim() {
            if (!TrimActive) {
                TrimActive = true;
                Debug.Log("[FBW]AUTO TRIM");
            }
            else {
                TrimActive = false;
                Debug.Log("[FBW]MAN TRIM");
            }

            Dial_Funcon.SetActive(TrimActive);

            TrimError = 0;
            TrimErrorIntergrate = 0;
        }

        private float Remap01(float value, float oldMin, float oldMax) {
            return (value - oldMin) / (oldMax - oldMin);
        }

    #region DFUNC

        public float controllerSensitivity = 0.5f;
        public KeyCode desktopUp = KeyCode.T, desktopDown = KeyCode.Y;

        public float desktopStep = 0.02f;

        public KeyCode desktopEnableAuto = KeyCode.F6;
        public GameObject Dial_Funcon;
        public TextMeshPro DebugOut;
        private string triggerAxis;
        private VRCPlayerApi.TrackingDataType trackingTarget;

        public SaccEntity entityControl;
        public SaccAirVehicle SAVControl;
        private Transform controlsRoot;
        private Rigidbody vehicleRigidbody;
        private Animator vehicleAnimator;
        private bool hasPilot, isPilot, isOwner, isSelected, isDirty, triggered, prevTriggered;
        private bool InVR;
        private bool triggerLastFrame;
        private Vector3 prevTrackingPosition;
        private float sliderInput;
        private float rotMultiMaxSpeed;
        private float triggerTapTime = 1;

        public void DFUNC_LeftDial() {
            triggerAxis = "Oculus_CrossPlatform_PrimaryIndexTrigger";
            trackingTarget = VRCPlayerApi.TrackingDataType.LeftHand;
        }

        public void DFUNC_RightDial() {
            triggerAxis = "Oculus_CrossPlatform_SecondaryIndexTrigger";
            trackingTarget = VRCPlayerApi.TrackingDataType.RightHand;
        }

        public void DFUNC_Selected() {
            gameObject.SetActive(true);
            isSelected = true;
            prevTriggered = false;
        }

        public void DFUNC_Deselected() {
            gameObject.SetActive(trimMode > 0);
            isSelected = false;
            triggerTapTime = 1;
        }

        public void SFEXT_L_EntityStart() {
            controlsRoot = SAVControl.ControlsRoot;
            rotMultiMaxSpeed = SAVControl.RotMultiMaxSpeed;
            if (!controlsRoot) controlsRoot = entityControl.transform;
            vehicleAnimator = SAVControl.VehicleAnimator;
            ResetStatus();
        }

        public void SFEXT_O_PilotEnter() {
            isPilot = true;
            isOwner = true;
            isSelected = false;
            prevTriggered = false;
        }

        public void SFEXT_O_PilotExit() {
            isPilot = false;
            triggerTapTime = 1;
            isSelected = false;
        }

        public void SFEXT_O_TakeOwnership() {
            isOwner = true;
        }

        public void SFEXT_O_LoseOwnership() {
            isOwner = false;
        }

        public void SFEXT_G_PilotEnter() {
            hasPilot = true;
            gameObject.SetActive(true);
        }

        public void SFEXT_G_PilotExit() {
            hasPilot = false;
        }

        public void SFEXT_G_Explode() {
            ResetStatus();
        }

        public void SFEXT_G_RespawnButton() {
            ResetStatus();
        }



        private void OnEnable() {
            triggerLastFrame = true;
        }

        private void OnDisable() {
            isSelected = false;
        }

        private void Update() {
            isDirty = false;
            if (isPilot) PilotUpdate();
            LocalUpdate();
            if (!hasPilot && !isDirty) gameObject.SetActive(false);
        }

        public override void PostLateUpdate() {
            if (isPilot) 
            {
                prevTriggered = triggered;
                triggered = (isSelected && Input.GetAxis(triggerAxis) > 0.75f) || debugControllerTransform;
                triggerTapTime += Time.deltaTime;
                
                if (triggered) 
                {
                    var trackingPosition =
                        controlsRoot.InverseTransformPoint(Networking.LocalPlayer.GetTrackingData(trackingTarget)
                            .position);
                    if (debugControllerTransform)
                        trackingPosition = controlsRoot.InverseTransformPoint(debugControllerTransform.position);

                    if (prevTriggered) 
                    {
                        sliderInput =
                            Mathf.Clamp(
                                Vector3.Dot(trackingPosition - prevTrackingPosition, vrInputAxis) *
                                controllerSensitivity, -1, 1);
                    }
                    else //enable and disable
                    {
                        if (triggerTapTime > .4f) //no double tap
                        {
                            triggerTapTime = 0;
                        }
                        else //double tap detected, switch trim
                        {
                            ToggleAutoTrim();
                            triggerTapTime = 1;
                        }
                    }

                    prevTrackingPosition = trackingPosition;
                }
                else {
                    sliderInput = 0;
                }

                if (Input.GetKeyDown(desktopUp)) sliderInput = desktopStep;
                if (Input.GetKeyDown(desktopDown)) sliderInput = -desktopStep;

                if (Input.GetKeyDown(desktopEnableAuto)) ToggleAutoTrim();
            }
        }

        private void SetDirty() {
            isDirty = true;
        }

        private float GetSliderInput() {
            //使用了sav的标记方法，UP=-1 DOWN=1
            return -sliderInput;
        }

    #endregion
    }
}