using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using SaccFlightAndVehicles;

//note:this code is original from https://github.com/esnya/EsnyaSFAddons
//to satisfy vau320's demand, add autotrim

namespace A320VAU.DFUNC
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)]
    public class DFUNC_a320_ElevatorTrim : UdonSharpBehaviour
    {
        #region DFUNC
        public float controllerSensitivity = 0.5f;
        public KeyCode desktopUp = KeyCode.T, desktopDown = KeyCode.Y;
        
        public float desktopStep = 0.02f;
        [Tooltip("自动配平默认开启")]
        public bool autoTrim = true;
        public KeyCode desktopEnableAuto = KeyCode.F6;
        public GameObject Dial_Funcon;
        private string triggerAxis;
        private VRCPlayerApi.TrackingDataType trackingTarget;
        private SaccAirVehicle airVehicle;
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

        public void DFUNC_LeftDial()
        {
            triggerAxis = "Oculus_CrossPlatform_PrimaryIndexTrigger";
            trackingTarget = VRCPlayerApi.TrackingDataType.LeftHand;
        }
        public void DFUNC_RightDial()
        {
            triggerAxis = "Oculus_CrossPlatform_SecondaryIndexTrigger";
            trackingTarget = VRCPlayerApi.TrackingDataType.RightHand;
        }
        public void DFUNC_Selected()
        {
            isSelected = true;
            prevTriggered = false;
        }
        public void DFUNC_Deselected()
        {
            isSelected = false;
            triggerTapTime = 1;
        }

        public void SFEXT_L_EntityStart()
        {
            var entity = GetComponentInParent<SaccEntity>();

            airVehicle = entity.GetComponentInChildren<SaccAirVehicle>(true);
            controlsRoot = airVehicle.ControlsRoot;
            rotMultiMaxSpeed = airVehicle.RotMultiMaxSpeed;
            if (!controlsRoot) controlsRoot = entity.transform;

            vehicleRigidbody = entity.GetComponent<Rigidbody>();
            vehicleAnimator = airVehicle.VehicleAnimator;

            trimStrength = airVehicle.PitchStrength * trimStrengthMultiplier;

            ResetStatus();
        }
        public void SFEXT_O_PilotEnter()
        {
            isPilot = true;
            isOwner = true;
            isSelected = false;
            prevTriggered = false;
        }
        public void SFEXT_O_PilotExit()
        {
            isPilot = false;
            triggerTapTime = 1;
            isSelected = false;
        }
        public void SFEXT_O_TakeOwnership() => isOwner = true;
        public void SFEXT_O_LoseOwnership() => isOwner = false;
        public void SFEXT_G_PilotEnter()
        {
            hasPilot = true;
            gameObject.SetActive(true);
        }
        public void SFEXT_G_PilotExit() => hasPilot = false;
        public void SFEXT_G_Explode() => ResetStatus();
        public void SFEXT_G_RespawnButton() => ResetStatus();

        private void OnEnable() => triggerLastFrame = true;
        private void OnDisable() => isSelected = false;

        private void Update()
        {
            isDirty = false;

            if (isPilot)
            {
                PilotUpdate();
            }

            if (isOwner) OwnerUpdate();
            LocalUpdate();

            if (!hasPilot && !isDirty) gameObject.SetActive(false);
        }

        public override void PostLateUpdate()
        {
            if (isPilot)
            {
                prevTriggered = triggered;
                triggered = isSelected && Input.GetAxis(triggerAxis) > 0.75f || debugControllerTransform;
                triggerTapTime += Time.deltaTime;
                if (triggered)
                {
                    var trackingPosition = controlsRoot.InverseTransformPoint(Networking.LocalPlayer.GetTrackingData(trackingTarget).position);
                    if (debugControllerTransform) trackingPosition = controlsRoot.InverseTransformPoint(debugControllerTransform.position);

                    if (prevTriggered)
                    {
                        sliderInput = Mathf.Clamp(Vector3.Dot(trackingPosition - prevTrackingPosition, vrInputAxis) * controllerSensitivity, -1, 1);
                    }
                    else //enable and disable
                    {
                        if (triggerTapTime > .4f)//no double tap
                        {
                            triggerTapTime = 0;
                        }
                        else//double tap detected, switch trim
                        {
                            ToggleAutoTrim();
                            triggerTapTime = 1;
                        }
                    }
                    prevTrackingPosition = trackingPosition;
                }
                else
                {
                    sliderInput = 0;
                }




                if (Input.GetKeyDown(desktopUp)) sliderInput = desktopStep;
                if (Input.GetKeyDown(desktopDown)) sliderInput = -desktopStep;

                if (Input.GetKeyDown(desktopEnableAuto)) ToggleAutoTrim();
            }
        }

        private void ToggleAutoTrim()
        {
            autoTrim = !autoTrim;
            Dial_Funcon.SetActive(autoTrim);
        }

        private void SetDirty()
        {
            isDirty = true;
        }

        private float GetSliderInput()
        {
            return sliderInput;
        }
        #endregion

        public float trimStrengthMultiplier = 1;
        public float trimStrengthCurve = 1;
        public string animatorParameterName = "elevtrim";
        public Vector3 vrInputAxis = Vector3.forward;

        public float trimBias = 0;
        private float trimStrength;

        [Header("Haptics")]
        [Range(0, 1)] public float hapticDuration = 0.2f;
        [Range(0, 1)] public float hapticAmplitude = 0.5f;
        [Range(0, 1)] public float hapticFrequency = 0.1f;

        [System.NonSerialized] [UdonSynced] public float trim;
        private float prevTrim;

        private void ResetStatus()
        {
            autoTrim = true;
            Dial_Funcon.SetActive(autoTrim);
            prevTrim = trim = 0;
            if (vehicleAnimator) vehicleAnimator.SetFloat(animatorParameterName, .5f);
        }

        private void FixedUpdate()
        {
            if (!isOwner) return;

            var airspeed = Vector3.Dot(airVehicle.AirVel, transform.forward);
            if (airspeed < 0.1f) return;

            var rotlift = Mathf.Clamp(airspeed / rotMultiMaxSpeed, -1, 1);

            vehicleRigidbody.AddForceAtPosition(-transform.up * (Mathf.Sign(trim) * Mathf.Pow(Mathf.Abs(trim), trimStrengthCurve) + trimBias) * trimStrength * rotlift * airVehicle.Atmosphere, transform.position, ForceMode.Force);

        }

        private void PilotUpdate()
        {

            float input = 0f;
            //2022-12-03添加自动配平功能
            if (autoTrim)
            {
                //简单的根据油门配平的逻辑
                //https://nihe.91maths.com/linear.php
                //trim = -4f * airVehicle.ThrottleInput + 3.3f;
                trim = -0.79f * airVehicle.ThrottleInput + 0.09f;
            }
            else
            {
                input = GetSliderInput();
                trim = Mathf.Clamp(trim + input, -1, 1);
            }

            if (!Mathf.Approximately(input, 0) && Time.frameCount % Mathf.FloorToInt(hapticDuration / Time.fixedDeltaTime) == 0)
            {
                PlayHapticEvent();
            }
        }

        private void OwnerUpdate()
        {
        }

        private void LocalUpdate()
        {
            var trimChanged = !Mathf.Approximately(trim, prevTrim);
            prevTrim = trim;
            if (trimChanged)
            {
                SetDirty();
                if (vehicleAnimator) vehicleAnimator.SetFloat(animatorParameterName, Remap01(trim, -1, 1));
            }
        }

        public void TrimUp()
        {
            trim += desktopStep;
        }
        public void TrimDown()
        {
            trim -= desktopStep;
        }

        private void PlayHapticEvent()
        {
            var hand = trackingTarget == VRCPlayerApi.TrackingDataType.LeftHand ? VRC_Pickup.PickupHand.Left : VRC_Pickup.PickupHand.Right;
            Networking.LocalPlayer.PlayHapticEventInHand(hand, hapticDuration, hapticAmplitude, hapticFrequency);
        }

        private float Remap01(float value, float oldMin, float oldMax)
        {
            return (value - oldMin) / (oldMax - oldMin);
        }

        [Header("Debug")]
        public Transform debugControllerTransform;
    }
}
