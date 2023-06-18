
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using YuxiFlightInstruments.BasicFlightData;
using A320VAU.Avionics;
using EsnyaSFAddons.DFUNC;
using A320VAU.SFEXT;
using UnityEngine.Serialization;

namespace A320VAU.PFD
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PFDBasicDisplay : UdonSharpBehaviour
    {
        #region Aircraft Systems
        [Header("Aircraft Systems")]
        [Tooltip("Flight Data Interface")]
        public YFI_FlightDataInterface FlightData;
        public GPWS_OWML GPWSController;
        public FCU.FCU FCU;
        public DFUNC_AdvancedFlaps Flaps;
        public SFEXT_a320_AdvancedGear Gear;
        #endregion

        #region Indicator Settings
        [Header("Indicator Settings")]
        [Tooltip("仪表的动画控制器")]
        public Animator IndicatorAnimator;
        [Header("下面的量程都是单侧的")]
        [Tooltip("速度表最大量程(节)")]
        public float MAXSPEED = 600f;
        [Tooltip("俯仰角最大量程(度)")]
        public float MAXPITCH = 90f;
        [Tooltip("滚转角最大量程(度)")]
        public float MAXBANK = 180f;
        [Tooltip("高度表最大量程(英尺)")]
        public float MAXALT = 99990f;
        [Tooltip("标高指示范围")]
        public float MAXRHE = 600f;
        [Header("对于数字每一位都需要单独动画的仪表")]
        public bool altbybit = true;

        [Tooltip("一些罗盘动画起始角度并不为0")]
        public float HDGoffset = 180;
        [Tooltip("爬升率最大量程(英尺/分钟)")]
        public float MAXVS = 6000;
        //侧滑这个数值先固定着
        [Tooltip("最大侧滑角")]
        public float MAXSLIPANGLE = 40;

        [Tooltip("最大垂直侧滑")]
        public float MAXTRACKPITCH = 25;
        #endregion

        [Header("UI element")]
        public GameObject VSbackground;
        public Text VSText;
        public Text RadioHeightText;
        public Text MachNumberText;

        public GameObject TargetSpeedTop;
        public Text TargetSpeedTopText;
        public GameObject TargetSpeedBottom;
        public Text TargetSpeedBottomText;

        public GameObject landingSystem;

        public GameObject flightDirectionFail;

        [Header("Speed element")]
        public GameObject[] disableOnGround;
        public GameObject[] enableOnGround;

        [Header("EFIS Indicator")] 
        public GameObject flightDirectionIndicator;
        public GameObject landingSystemIndicator;

        public bool isFlightDirectionOn { get; private set; }
        public bool isLandingSystemOn { get; private set; }

        private float _altitude = 0f;

        #region Animation Hash
        //animator strings that are sent every frame are converted to int for optimization
        private int AIRSPEED_HASH = Animator.StringToHash("AirSpeedNormalize");
        private int AIRSPEED_SECLECT_HASH = Animator.StringToHash("AirSpeedSelectNormalize");
        private int PITCH_HASH = Animator.StringToHash("PitchAngleNormalize");
        private int BANK_HASH = Animator.StringToHash("BankAngleNormalize");
        private int ALT_HASH = Animator.StringToHash("AltitudeNormalize");
        private int ALT10_HASH = Animator.StringToHash("Altitude10Normalize");
        private int ALT100_HASH = Animator.StringToHash("Altitude100Normalize");
        private int ALT1000_HASH = Animator.StringToHash("Altitude1000Normalize");
        private int ALT10000_HASH = Animator.StringToHash("Altitude10000Normalize");
        private int ROC_HASH = Animator.StringToHash("VerticalSpeedNormalize");
        private int HEADING_HASH = Animator.StringToHash("HeadingNormalize");
        private int SLIPANGLE_HASH = Animator.StringToHash("SlipAngleNormalize");
        private int RH_HASH = Animator.StringToHash("RHNormalize");
        private int TRKPCH_HASH = Animator.StringToHash("TRKPCHNormalize");
        private int VMAX_HASH = Animator.StringToHash("VMAXNormalize");
        private int VSW_HASH = Animator.StringToHash("VSWNormalize");
        private int VFE_NEXT_HASH = Animator.StringToHash("VFENEXTNormalize");
        private int VLS_HASH = Animator.StringToHash("VLSNormalize");
        #endregion

        private float PitchAngle = 0f;
        private float BankAngle = 0f;
        private float HeadingAngle = 0f;
        private float RadioHeight = 0f;

        private void Start()
        {
            // Reset Flight Direction and Landing System
            flightDirectionIndicator.SetActive(isFlightDirectionOn);
            flightDirectionFail.SetActive(isFlightDirectionOn);
            
            landingSystem.SetActive(isLandingSystemOn);
            landingSystemIndicator.SetActive(isLandingSystemOn);
        }

        #region Update
        private void LateUpdate()
        {
            //这里可以用来做仪表更新延迟之类的逻辑
            PitchAngle = FlightData.pitch;
            BankAngle = FlightData.bank;
            HeadingAngle = FlightData.magneticHeading;
            RadioHeight = (float)GPWSController.GetProgramVariable("radioAltitude");
            //AirSpeed
            UpdateAirspeed();
            //Altitude
            UpdateAltitude();
            //RH
            UpdateRadioHeight();
            //VS
            UpdateVerticalSpeed();
            //Heading
            UpdateHeading();
            //Bank
            UpdateBank();
            //Pitch
            UpdatePitch();
            //Slip
            UpdateSlip();
            //TrackPitch
            UpdateTrickPitch();

            UpdateMachNumber();

        }

        #region Speed
        [Header("Speed")]
        public int VMO = 350;
        public int VLE = 280;

        // VSW
        public int VSWCONF0 = 145;
        public int VSWCONF1 = 113;
        public int VSWCONF2 = 107;
        public int VSWCONF3 = 104;
        public int VSWCONFFULL = 102;

        // F and S
        // public int SSpeed = 178;
        // public int FSpeed = 137;

        public int GreenDotSpeed = 195;

        private void UpdateAirspeed()
        {
            foreach (var item in disableOnGround)
            {
                item.SetActive(!FlightData.SAVControl.Taxiing);
            }
            foreach (var item in enableOnGround)
            {
                item.SetActive(FlightData.SAVControl.Taxiing);
            }

            IndicatorAnimator.SetFloat(AIRSPEED_HASH, FlightData.TAS / MAXSPEED);

            #region Target Speed
            IndicatorAnimator.SetFloat(AIRSPEED_SECLECT_HASH, FCU.TargetSpeed / 500f);

            TargetSpeedTopText.text = FCU.TargetSpeed.ToString();
            TargetSpeedBottomText.text = FCU.TargetSpeed.ToString();

            TargetSpeedBottom.SetActive(false);
            TargetSpeedTop.SetActive(false);
            if (FlightData.TAS - FCU.TargetSpeed > 45)
                TargetSpeedBottom.SetActive(true);

            if (FCU.TargetSpeed - FlightData.TAS > 45)
                TargetSpeedTop.SetActive(true);
            #endregion

            #region VMAX
            var VMAX = VMO;
            if (Flaps.targetSpeedLimit < VMAX)
                VMAX = (int)Flaps.targetSpeedLimit;

            if (Flaps.speedLimit < VMAX)
                VMAX = (int)Flaps.speedLimit;

            if (Gear.position != 0 && VLE < VMAX)
                VMAX = VLE;

            IndicatorAnimator.SetFloat(VMAX_HASH, VMAX / 360f);
            #endregion

            #region VSW
            var VSW = VSWCONF0;
            if (Flaps.detentIndex == 1) VSW = VSWCONF1;
            if (Flaps.detentIndex == 2) VSW = VSWCONF2;
            if (Flaps.detentIndex == 3) VSW = VSWCONF3;
            if (Flaps.detentIndex == 4) VSW = VSWCONFFULL;

            IndicatorAnimator.SetFloat(VSW_HASH, VSW / 300f);
            #endregion

            #region VFE NEXT
            var VFENext = Flaps.speedLimits[1];
            switch (Flaps.targetDetentIndex)
            {
                case 1:
                    VFENext = Flaps.speedLimits[2];
                    break;
                case 2:
                    VFENext = Flaps.speedLimits[3];
                    break;
                case 3:
                    VFENext = Flaps.speedLimits[4];
                    break;
            }

            IndicatorAnimator.SetFloat(VFE_NEXT_HASH, VFENext / 240f);
            #endregion

            #region VLS
            var VLS = 1.28f * VSW;
            switch (Flaps.detentIndex)
            {
                case 2:
                    VLS = 1.13f * VSW;
                    break;
                case 1:
                    VLS = 1.28f * VSW;
                    break;
            }

            IndicatorAnimator.SetFloat(VLS_HASH, VLS / 200f);
            #endregion
        }
        
        private void UpdateMachNumber()
        {
            if (FlightData.mach > 0.5f)
            {
                MachNumberText.gameObject.SetActive(true);
                MachNumberText.text = "." + (FlightData.mach * 100).ToString("f0");
            }
            else
            {
                MachNumberText.gameObject.SetActive(false);
            }
        }
        #endregion

        #region Altitude
        private void UpdateAltitude()
        {

            //默认都会写Altitude
            _altitude = FlightData.altitude;
            IndicatorAnimator.SetFloat(ALT_HASH, (_altitude / MAXALT));
            
            if (!altbybit) return;
            IndicatorAnimator.SetFloat(ALT10_HASH, (_altitude % 100) / 100f);
            IndicatorAnimator.SetFloat(ALT100_HASH, ((int)(_altitude / 100f) % 10) / 10f);
            IndicatorAnimator.SetFloat(ALT1000_HASH, ((int)(_altitude / 1000f) % 10) / 10f);
            IndicatorAnimator.SetFloat(ALT10000_HASH, ((int)(_altitude / 10000f) % 10) / 10f);

        }

        private void UpdateRadioHeight()
        {
            if (RadioHeight > 2500f)
            {
                RadioHeightText.gameObject.SetActive(false);
            }
            else
            {
                RadioHeightText.gameObject.SetActive(true);
                RadioHeightText.text = RadioHeight.ToString("f0");
                var RadioAltitudeNormal = Remap01(RadioHeight, -MAXRHE, MAXRHE);
                IndicatorAnimator.SetFloat(RH_HASH, RadioAltitudeNormal);
            }

        }
        #endregion

        private void UpdateVerticalSpeed()
        {
            var verticalSpeed = FlightData.verticalSpeed;
            float VerticalSpeedNormal = Remap01(FlightData.verticalSpeed, -MAXVS, MAXVS);
            IndicatorAnimator.SetFloat(ROC_HASH, VerticalSpeedNormal);
            if (Mathf.Abs(verticalSpeed) > 200)
            {
                VSbackground.SetActive(true);
                if (Mathf.Abs(verticalSpeed) > 6000) VSText.color = new Color(0.91373f, 0.54901f, 0);
                else if (1000 < RadioHeight && RadioHeight < 2000 && Mathf.Abs(verticalSpeed) > 2000) VSText.color = new Color(0.91373f, 0.54901f, 0);
                else if ((RadioHeight < 1200) && Mathf.Abs(verticalSpeed) > 1200) VSText.color = new Color(0.91373f, 0.54901f, 0);
                else VSText.color = new Color(0, 1, 0);
                VSText.text = Mathf.Abs(verticalSpeed / 100).ToString("f0");
            }
            else
            {
                VSbackground.gameObject.SetActive(false);
            }
        }
        
        private void UpdateHeading()
        {
            IndicatorAnimator.SetFloat(HEADING_HASH, ((HeadingAngle - HDGoffset + 360) % 360) / 360f);
        }
        
        private void UpdatePitch()
        {
            //玄学问题，Pitch 跟 Bank 调用不了Remap01??
            float PitchAngleNormal = Mathf.Clamp01((PitchAngle + MAXPITCH) / (MAXPITCH + MAXPITCH));
            IndicatorAnimator.SetFloat(PITCH_HASH, PitchAngleNormal);
        }
        
        private void UpdateBank()
        {
            float BankAngleNormal = Mathf.Clamp01((BankAngle + MAXBANK) / (MAXBANK + MAXBANK));
            IndicatorAnimator.SetFloat(BANK_HASH, BankAngleNormal);
        }
        
        private void UpdateSlip()
        {
            IndicatorAnimator.SetFloat(SLIPANGLE_HASH, Mathf.Clamp01((FlightData.SlipAngle + MAXSLIPANGLE) / (MAXSLIPANGLE + MAXSLIPANGLE)));
        }
        
        private void UpdateTrickPitch()
        {
            IndicatorAnimator.SetFloat(TRKPCH_HASH, Mathf.Clamp01((FlightData.trackPitchAngle + MAXTRACKPITCH) / (MAXTRACKPITCH + MAXTRACKPITCH)));
        }
        #endregion

        #region Touch Switch Event
        // ReSharper disable once UnusedMember.Global
        public void ToggleFlightDirection()
        {
            isFlightDirectionOn = !isFlightDirectionOn;
            flightDirectionIndicator.SetActive(isFlightDirectionOn);
            flightDirectionFail.SetActive(isFlightDirectionOn);
        }

        // ReSharper disable once UnusedMember.Global
        public void ToggleLandingSystem()
        {
            isLandingSystemOn = !isLandingSystemOn;
            landingSystem.SetActive(isLandingSystemOn);
            landingSystemIndicator.SetActive(isLandingSystemOn);
        }
        #endregion

        #region Math
        private static float Remap01(float value, float valueMin, float valueMax)
        {
            value = Mathf.Clamp01((value - valueMin) / (valueMax - valueMin));
            return value;
        }
        #endregion
    }
}

