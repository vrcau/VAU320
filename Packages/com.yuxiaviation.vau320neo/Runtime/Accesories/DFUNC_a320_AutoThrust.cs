﻿using A320VAU.SFEXT;
using SaccFlightAndVehicles;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

namespace A320VAU {
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DFUNC_a320_AutoThrust : UdonSharpBehaviour {
        [SerializeField] UdonSharpBehaviour SAVControl;
        public SFEXT_a320_AdvancedEngine[] engines = {};

        [Tooltip("Object enabled when function is active (used on MFD)")]
        public GameObject Dial_Funcon;

        public bool AllowCruiseGrounded;
        public Text HUDText_knotstarget;
        private SaccEntity EntityControl;
        private bool UseLeftTrigger = false;
        private bool TriggerLastFrame;
        private Transform VehicleTransform;
        private VRCPlayerApi localPlayer;
        private float CruiseTemp;
        private float SpeedZeroPoint;
        private float TriggerTapTime = 0;
        [System.NonSerializedAttribute] public bool Cruise;
        private float CruiseProportional = .1f;
        private float CruiseIntegral = .1f;
        private float CruiseIntegrator;
        private float CruiseIntegratorMax = 5;
        private float CruiseIntegratorMin = -5;
        private float Cruiselastframeerror;
        private bool func_active;
        private bool Selected;
        private bool Piloting;
        private bool EngineOn;
        private bool InVTOL;
        private bool InReverse;
        private bool InVR;
        private bool CruiseThrottleOverridden;
        private Transform ControlsRoot;
        [System.NonSerializedAttribute] public float SetSpeed;

        public void DFUNC_LeftDial() {
            UseLeftTrigger = true;
        }

        public void DFUNC_RightDial() {
            UseLeftTrigger = false;
        }

        public void SFEXT_L_EntityStart() {
            localPlayer = Networking.LocalPlayer;
            if (localPlayer != null) {
                InVR = localPlayer.IsUserInVR();
            }

            EntityControl = (SaccEntity)SAVControl.GetProgramVariable("EntityControl");
            ControlsRoot = (Transform)SAVControl.GetProgramVariable("ControlsRoot");
            VehicleTransform = EntityControl.transform;
            if (Dial_Funcon) Dial_Funcon.SetActive(false);
        }

        public void DFUNC_Selected() {
            TriggerLastFrame = true;
            gameObject.SetActive(true);
            Selected = true;
        }

        public void DFUNC_Deselected() {
            if (!Cruise) {
                gameObject.SetActive(false);
            }

            Selected = false;
        }

        public void SFEXT_O_PilotEnter() {
            if (Dial_Funcon) Dial_Funcon.SetActive(Cruise);
            Piloting = true;
            if (HUDText_knotstarget) {
                HUDText_knotstarget.text = string.Empty;
            }
        }

        public void SFEXT_P_PassengerEnter() {
            if (Dial_Funcon) Dial_Funcon.SetActive(Cruise);
            if (HUDText_knotstarget) {
                HUDText_knotstarget.text = string.Empty;
            }
        }

        public void SFEXT_O_PilotExit() {
            Piloting = false;
            Selected = false;
        }

        public void SFEXT_G_EngineOn() {
            EngineOn = true;
        }

        public void SFEXT_G_EngineOff() {
            EngineOn = false;
            gameObject.SetActive(false);
            func_active = false;
            if (Cruise) {
                SetCruiseOff();
            }
        }

        public void SFEXT_G_Explode() {
            if (Cruise) {
                SetCruiseOff();
            }
        }

        public void SFEXT_G_TouchDown() {
            if (Cruise && !AllowCruiseGrounded) {
                SetCruiseOff();
            }
        }

        public void SFEXT_O_EnterVTOL() {
            if (Cruise) {
                SetCruiseOff();
            }

            InVTOL = true;
        }

        public void SFEXT_O_ExitVTOL() {
            InVTOL = false;
        }

        public void SFEXT_O_StartReversing() {
            if (Cruise) {
                SetCruiseOff();
            }

            InReverse = true;
        }

        public void SFEXT_O_StopReversing() {
            InReverse = false;
        }

        private void LateUpdate() {
            if (EngineOn) {
                if (InVR) {
                    if (Selected) {
                        float Trigger;
                        if (UseLeftTrigger) {
                            Trigger = Input.GetAxisRaw("Oculus_CrossPlatform_PrimaryIndexTrigger");
                        }
                        else {
                            Trigger = Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryIndexTrigger");
                        }

                        if (Trigger > 0.75) {
                            //for setting speed in VR
                            Vector3 handpos = ControlsRoot.position -
                                              localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand)
                                                  .position;
                            handpos = ControlsRoot.InverseTransformDirection(handpos);

                            //enable and disable
                            if (!TriggerLastFrame) {
                                if (!Cruise) {
                                    if ((!(bool)SAVControl.GetProgramVariable("Taxiing") || AllowCruiseGrounded) &&
                                        !InVTOL && !InReverse) {
                                        SetCruiseOn();
                                    }
                                }

                                if (Time.time - TriggerTapTime < .4f) //double tap detected, turn off cruise
                                {
                                    SetCruiseOff();
                                }

                                SpeedZeroPoint = handpos.z;
                                CruiseTemp = SetSpeed;
                                TriggerTapTime = Time.time;
                            }

                            float SpeedDifference = (SpeedZeroPoint - handpos.z) * 250;
                            SetSpeed = Mathf.Clamp(CruiseTemp + SpeedDifference, 0, 2000);

                            TriggerLastFrame = true;
                        }
                        else {
                            TriggerLastFrame = false;
                        }
                    }
                }
                else {
                    bool ShiftCtrl = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftControl);
                    if (ShiftCtrl) {
                        if (CruiseThrottleOverridden) {
                            foreach (var engine in engines) {
                                engine.isAutoThrustActive = false;
                            }

                            CruiseThrottleOverridden = false;
                        }
                    }
                    else {
                        if (Cruise) {
                            if (!CruiseThrottleOverridden) {
                                foreach (var engine in engines) {
                                    engine.isAutoThrustActive = true;
                                }

                                CruiseThrottleOverridden = true;
                            }
                        }
                    }
                }

                float DeltaTime = Time.deltaTime;
                float equals = Input.GetKey(KeyCode.Equals) ? DeltaTime * 10 : 0;
                float minus = Input.GetKey(KeyCode.Minus) ? DeltaTime * 10 : 0;
                SetSpeed = Mathf.Max(SetSpeed + (equals - minus), 0);

                if (func_active) {
                    float error = (SetSpeed - (float)SAVControl.GetProgramVariable("AirSpeed"));

                    CruiseIntegrator += error * DeltaTime;
                    CruiseIntegrator = Mathf.Clamp(CruiseIntegrator, CruiseIntegratorMin, CruiseIntegratorMax);

                    foreach (var engine in engines) {
                        engine.autoThrustInput =
                            Mathf.Clamp((CruiseProportional * error) + (CruiseIntegral * CruiseIntegrator), 0, 1);
                    }
                }
            }

            //Cruise Control target knots
            if (Cruise) {
                if (HUDText_knotstarget) {
                    HUDText_knotstarget.text = ((SetSpeed) * 1.9438445f).ToString("F0");
                }
            }
        }

        public void KeyboardInput() {
            if (!Cruise) {
                if ((!(bool)SAVControl.GetProgramVariable("Taxiing") || AllowCruiseGrounded) && !InVTOL && !InReverse) {
                    SetCruiseOn();
                }
            }
            else {
                SetCruiseOff();
            }
        }

        public void SetCruiseOn() {
            if (Cruise) {
                return;
            }

            if (Piloting) {
                func_active = true;
                if (!InVR) {
                    gameObject.SetActive(true);
                }
            }

            if (!CruiseThrottleOverridden) {
                foreach (var engine in engines) {
                    engine.isAutoThrustActive = true;
                }

                CruiseThrottleOverridden = true;
            }

            SetSpeed = (float)SAVControl.GetProgramVariable("AirSpeed");
            Cruise = true;
            if (Dial_Funcon) {
                Dial_Funcon.SetActive(true);
            }

            EntityControl.SendEventToExtensions("SFEXT_O_CruiseEnabled");
        }

        public void SetCruiseOff() {
            if (!Cruise) {
                return;
            }

            if (Piloting) {
                func_active = false;
                if (!InVR) {
                    gameObject.SetActive(false);
                }
            }

            if (CruiseThrottleOverridden) {
                foreach (var engine in engines) {
                    engine.isAutoThrustActive = false;
                }

                CruiseThrottleOverridden = false;
            }

            Cruise = false;
            if (Dial_Funcon) {
                Dial_Funcon.SetActive(false);
            }

            if (HUDText_knotstarget) {
                HUDText_knotstarget.text = string.Empty;
            }

            EntityControl.SendEventToExtensions("SFEXT_O_CruiseDisabled");
        }

        public void SFEXT_O_ThrottleDropped() {
            if (!CruiseThrottleOverridden && Cruise) {
                SAVControl.SetProgramVariable("ThrottleOverridden",
                    (int)SAVControl.GetProgramVariable("ThrottleOverridden") + 1);
                CruiseThrottleOverridden = true;
            }
        }

        public void SFEXT_O_ThrottleGrabbed() {
            if (CruiseThrottleOverridden) {
                SAVControl.SetProgramVariable("ThrottleOverridden",
                    (int)SAVControl.GetProgramVariable("ThrottleOverridden") - 1);
                CruiseThrottleOverridden = false;
            }
        }

        public void SFEXT_O_LoseOwnership() {
            gameObject.SetActive(false);
            func_active = false;
            if (Cruise) {
                SetCruiseOff();
            }
        }
    }
}