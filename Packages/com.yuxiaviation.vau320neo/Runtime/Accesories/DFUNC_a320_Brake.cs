﻿using System;
using SaccFlightAndVehicles;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace A320VAU.Brake {
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class DFUNC_a320_Brake : UdonSharpBehaviour {
        /*320专用的刹车，增加了地面摩擦阻力,停留刹车*/

        public float autoBrakeInput;
        public bool isManuelBrakeInUse { get; private set; }

        public UdonSharpBehaviour SAVControl;

        [Tooltip("Looping sound to play while brake is active")]
        public AudioSource Airbrake_snd;

        [Tooltip("Will Crash if not set")]
        public Animator BrakeAnimator;

        [Tooltip("Position the ground brake force will be applied at")]
        public Transform GroundBrakeForcePosition;

        [Tooltip(
            "Because you have to hold the break, and the keyboardcontrols script can only send events, this option is here.")]
        public KeyCode KeyboardControl = KeyCode.B;

        public KeyCode ParkBreakControl = KeyCode.P;
        [UdonSynced] public bool ParkBreakSet;
        public float AirbrakeStrength = 4f;
        public float GroundBrakeStrength = 6;

        [Tooltip("Water brake functionality requires that floatscript is being used")]
        public float WaterBrakeStrength = 1f;

        public bool NoPilotAlwaysGroundBrake = true;

        [Tooltip("Speed below which the ground break works meters/s")]
        public float GroundBrakeSpeed = 40f;

        [Tooltip("地面滑行时候机轮的摩擦阻力")]
        public float ConstantDrag = 0.1f;

        [Tooltip("停留刹车指示")]
        public GameObject Dial_Funcon;

        //other functions can set this +1 to disable breaking
        [NonSerialized] public bool _DisableGroundBrake;
        private float AirbrakeLerper;
        private int BRAKE_STRING = Animator.StringToHash("brake");
        [NonSerialized] [UdonSynced] public float BrakeInput;
        private float BrakeStrength;
        private bool Braking;
        private bool BrakingLastFrame;

        [NonSerialized] [FieldChangeCallback(nameof(DisableGroundBrake_))]
        public int DisableGroundBrake;

        private SaccEntity EntityControl;
        private bool HasAirBrake;
        private bool IsOwner;
        private float LastDrag;
        private float NextUpdateTime;

        private float
            NonLocalActiveDelay; //this var is for adding a min delay for disabling for non-local users to account for lag

        private bool prevKeyPress;
        private bool prevTriggered;
        private float RotMultiMaxSpeedDivider;
        private bool Selected;
        private float triggerTapTime = 1;

        private bool UseLeftTrigger;
        private Rigidbody VehicleRigidbody;

        public int DisableGroundBrake_ {
            set {
                _DisableGroundBrake = value > 0;
                DisableGroundBrake = value;
            }
            get => DisableGroundBrake;
        }

        private void Update() {
            var DeltaTime = Time.deltaTime;
            if (IsOwner) {
                triggerTapTime += Time.deltaTime;
                var Speed = (float)SAVControl.GetProgramVariable("Speed");
                var CurrentVel = (Vector3)SAVControl.GetProgramVariable("CurrentVel");
                var Taxiing = (bool)SAVControl.GetProgramVariable("Taxiing");
                if ((bool)SAVControl.GetProgramVariable("Piloting")) {
                    float KeyboardBrakeInput = 0;
                    float VRBrakeInput = 0;

                    if (Selected) {
                        float Trigger;
                        if (UseLeftTrigger)
                            Trigger = Input.GetAxisRaw("Oculus_CrossPlatform_PrimaryIndexTrigger");
                        else
                            Trigger = Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryIndexTrigger");
                        VRBrakeInput = Trigger;
                        if (Trigger > 0.75f) {
                            if (!prevTriggered) {
                                if (triggerTapTime > .4f) //no double tap
                                {
                                    triggerTapTime = 0;
                                }
                                else //double tap detected, switch break
                                {
                                    ToggleParkBrake();
                                    Dial_Funcon.SetActive(ParkBreakSet);
                                    triggerTapTime = 1;
                                }
                            }

                            prevTriggered = true;
                        }
                        else {
                            prevTriggered = false;
                        }
                        //VR双击启动地面刹车
                    } //获取轴输入

                    if (Input.GetKey(KeyboardControl)) KeyboardBrakeInput = 1; //获取键盘输入
                    if (Input.GetKey(ParkBreakControl)) {
                        if (!prevKeyPress) {
                            ToggleParkBrake();
                            Dial_Funcon.SetActive(ParkBreakSet);
                            prevKeyPress = true;
                        }
                    } //生成parkbreak开关或brakeinput
                    else {
                        prevKeyPress = false;
                    }

                    BrakeInput = Mathf.Max(VRBrakeInput, KeyboardBrakeInput);
                    isManuelBrakeInUse = BrakeInput > 0;

                    if (BrakeInput < autoBrakeInput) {
                        BrakeInput = autoBrakeInput;
                    }

                    if (Taxiing) {
                        //ground brake checks if vehicle is on top of a rigidbody, and if it is, brakes towards its speed rather than zero
                        //does not work if owner of vehicle does not own the rigidbody 
                        var gdhr = (Rigidbody)SAVControl.GetProgramVariable("GDHitRigidbody");
                        if (gdhr) {
                            var RBSpeed = ((Vector3)SAVControl.GetProgramVariable("CurrentVel") - gdhr.velocity)
                                .magnitude;
                            var speed = (VehicleRigidbody.GetPointVelocity(GroundBrakeForcePosition.position) -
                                         gdhr.velocity).normalized;
                            speed = Vector3.ProjectOnPlane(speed, EntityControl.transform.up);
                            var BrakeForce = speed.normalized * ConstantDrag * DeltaTime; //摩擦阻力
                            if (BrakeInput > 0 && RBSpeed < GroundBrakeSpeed && !_DisableGroundBrake) {
                                BrakeForce += speed.normalized * BrakeInput * BrakeStrength * DeltaTime;
                                if (speed.sqrMagnitude < BrakeForce.sqrMagnitude) BrakeForce = speed;
                            }

                            if (ParkBreakSet && RBSpeed < 5) BrakeForce = speed;
                            VehicleRigidbody.AddForceAtPosition(-speed * BrakeInput * BrakeStrength * DeltaTime,
                                GroundBrakeForcePosition.position, ForceMode.VelocityChange);
                        }
                        else {
                            var speed = VehicleRigidbody.GetPointVelocity(GroundBrakeForcePosition.position);
                            speed = Vector3.ProjectOnPlane(speed, EntityControl.transform.up);
                            var BrakeForce = speed.normalized * ConstantDrag * DeltaTime; //摩擦阻力
                            if (BrakeInput > 0 && Speed < GroundBrakeSpeed && !_DisableGroundBrake) {
                                BrakeForce += speed.normalized * BrakeInput * BrakeStrength * DeltaTime;
                                if (speed.sqrMagnitude <
                                    BrakeForce.sqrMagnitude) BrakeForce = speed; //this'll stop the vehicle exactly
                            }

                            if (ParkBreakSet && Speed < 5) BrakeForce = speed;
                            //else ToggleParkBrake();
                            VehicleRigidbody.AddForceAtPosition(-BrakeForce, GroundBrakeForcePosition.position,
                                ForceMode.VelocityChange);
                        }
                    } //滑行时候刹车 上面两个分支是根据是否检测到飞机所在的地面刚体决定把速度刹到地面速度还是0

                    if (!HasAirBrake && !(bool)SAVControl.GetProgramVariable("Taxiing")) BrakeInput = 0;
                    //remove the drag added last frame to add the new value for this frame
                    var extradrag = (float)SAVControl.GetProgramVariable("ExtraDrag");
                    var newdrag = AirbrakeStrength * BrakeInput;
                    var dragtoadd = -LastDrag + newdrag;
                    extradrag += dragtoadd;
                    LastDrag = newdrag;
                    SAVControl.SetProgramVariable("ExtraDrag", extradrag);

                    //send events to other users to tell them to enable the script so they can see the animation
                    Braking = BrakeInput > .02f;
                    if (Braking) {
                        if (!BrakingLastFrame) {
                            if (Airbrake_snd && !Airbrake_snd.isPlaying) Airbrake_snd.Play();
                            SendCustomNetworkEvent(NetworkEventTarget.All, nameof(EnableForAnimation));
                        }

                        if (Time.time > NextUpdateTime) {
                            RequestSerialization();
                            NextUpdateTime = Time.time + .4f;
                        }
                    }
                    else {
                        if (BrakingLastFrame) {
                            var brk = BrakeInput;
                            BrakeInput = 0;
                            RequestSerialization();
                            BrakeInput = brk;
                        }
                    }

                    if (AirbrakeLerper < .03 && BrakeInput < .03)
                        if (Airbrake_snd && Airbrake_snd.isPlaying)
                            Airbrake_snd.Stop();
                    BrakingLastFrame = Braking;
                }
                else {
                    if (Taxiing) {
                        //outside of vehicle, simpler version, ground brake always max
                        Rigidbody gdhr = null;
                        {
                            gdhr = (Rigidbody)SAVControl.GetProgramVariable("GDHitRigidbody");
                        }
                        if (gdhr) {
                            var RBSpeed = ((Vector3)SAVControl.GetProgramVariable("CurrentVel") - gdhr.velocity)
                                .magnitude;
                            if (RBSpeed < GroundBrakeSpeed && !_DisableGroundBrake)
                                VehicleRigidbody.velocity = Vector3.MoveTowards(VehicleRigidbody.velocity,
                                    gdhr.GetPointVelocity(EntityControl.CenterOfMass.position),
                                    BrakeStrength * DeltaTime);
                        }
                        else {
                            if (Speed < GroundBrakeSpeed && !_DisableGroundBrake)
                                VehicleRigidbody.velocity = Vector3.MoveTowards(VehicleRigidbody.velocity, Vector3.zero,
                                    BrakeStrength * DeltaTime);
                        }
                    }
                }
            }
            else {
                //this object is enabled for non-owners only while animating
                NonLocalActiveDelay -= DeltaTime;
                if (NonLocalActiveDelay < 0 && AirbrakeLerper < 0.01) {
                    DisableForAnimation();
                    return;
                }
            }

            AirbrakeLerper = Mathf.Lerp(AirbrakeLerper, BrakeInput, 2f * DeltaTime);
            BrakeAnimator.SetFloat(BRAKE_STRING, AirbrakeLerper);
            if (Airbrake_snd) {
                Airbrake_snd.pitch = AirbrakeLerper * .2f + .9f;
                Airbrake_snd.volume = AirbrakeLerper *
                                      Mathf.Min((float)SAVControl.GetProgramVariable("Speed") * RotMultiMaxSpeedDivider,
                                          1);
            }
        }

        public void DFUNC_LeftDial() {
            UseLeftTrigger = true;
        }

        public void DFUNC_RightDial() {
            UseLeftTrigger = false;
        }

        public void SFEXT_L_EntityStart() {
            EntityControl = (SaccEntity)SAVControl.GetProgramVariable("EntityControl");
            VehicleRigidbody = EntityControl.GetComponent<Rigidbody>();
            HasAirBrake = AirbrakeStrength != 0;
            RotMultiMaxSpeedDivider = 1 / (float)SAVControl.GetProgramVariable("RotMultiMaxSpeed");
            IsOwner = (bool)SAVControl.GetProgramVariable("IsOwner");
            var localPlayer = Networking.LocalPlayer;
            if (localPlayer != null && !localPlayer.isMaster)
                gameObject.SetActive(false);
            else
                gameObject.SetActive(true);
            if (!GroundBrakeForcePosition) GroundBrakeForcePosition = EntityControl.CenterOfMass;
            Dial_Funcon.SetActive(ParkBreakSet);
        }

        public void DFUNC_Selected() {
            Selected = true;
            prevTriggered = false;
            prevKeyPress = false;
        }

        public void DFUNC_Deselected() {
            BrakeInput = 0;
            Selected = false;
        }

        public void SFEXT_O_PilotEnter() {
            prevTriggered = false;
            prevKeyPress = false;
            if (!NoPilotAlwaysGroundBrake) {
                if ((bool)SAVControl.GetProgramVariable("Floating"))
                    BrakeStrength = WaterBrakeStrength;
                else if ((bool)SAVControl.GetProgramVariable("Taxiing")) BrakeStrength = GroundBrakeStrength;
            }
        }

        public void SFEXT_O_PilotExit() {
            BrakeInput = 0;
            RequestSerialization();
            Selected = false;
            if (!NoPilotAlwaysGroundBrake) BrakeStrength = 0;
        }

        public void SFEXT_G_Explode() {
            BrakeInput = 0;
            BrakeAnimator.SetFloat(BRAKE_STRING, 0);
        }

        public void SFEXT_O_TakeOwnership() {
            gameObject.SetActive(true);
            IsOwner = true;
        }

        public void SFEXT_O_LoseOwnership() {
            gameObject.SetActive(false);
            IsOwner = false;
        }

        public void EnableForAnimation() {
            if (!IsOwner) {
                if (Airbrake_snd) Airbrake_snd.Play();
                gameObject.SetActive(true);
                NonLocalActiveDelay = 3;
            }
        }

        public void DisableForAnimation() {
            BrakeAnimator.SetFloat(BRAKE_STRING, 0);
            BrakeInput = 0;
            AirbrakeLerper = 0;
            if (Airbrake_snd) {
                Airbrake_snd.pitch = 0;
                Airbrake_snd.volume = 0;
            }

            gameObject.SetActive(false);
        }

        public void SFEXT_G_TouchDownWater() {
            BrakeStrength = WaterBrakeStrength;
        }

        public void SFEXT_G_TouchDown() {
            BrakeStrength = GroundBrakeStrength;
        }

        private void ToggleParkBrake() {
            ParkBreakSet = !ParkBreakSet;
            RequestSerialization();
        }
    }
}