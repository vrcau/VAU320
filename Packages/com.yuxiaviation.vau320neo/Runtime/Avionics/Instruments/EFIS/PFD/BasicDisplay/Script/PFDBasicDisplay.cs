using A320VAU.Common;
using A320VAU.Utils;
using Avionics.Systems.Common;
using EsnyaSFAddons.DFUNC;
using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

namespace A320VAU.PFD {
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class PFDBasicDisplay : UdonSharpBehaviour {
    #region Aircraft Systems

        private DependenciesInjector _injector;

        private ADIRU.ADIRU _adiru;
        private RadioAltimeter.RadioAltimeter _radioAltimeter;
        private AircraftSystemData _aircraftSystemData;
        private FCU.FCU _fcu;
        private DFUNC_AdvancedFlaps _flaps;
        private SystemEventBus _eventBus;

    #endregion

        private VRCPlayerApi _localPlayer;

        private readonly float UPDATE_INTERVAL = UpdateIntervalUtil.GetUpdateIntervalFromFPS(20);
        private float _lastUpdate;

        [Header("EFIS Indicator")]
        public GameObject flightDirectionIndicator;

        public GameObject landingSystemIndicator;

        private float _altitude;
        private float BankAngle;

        private float PitchAngle;
        private float RadioHeight;

        [PublicAPI] public bool isFlightDirectionOn { get; private set; } = true;
        [PublicAPI] public bool isLandingSystemOn { get; private set; }

        private void Start() {
            _injector = DependenciesInjector.GetInstance(this);

            _adiru = _injector.adiru;
            _radioAltimeter = _injector.radioAltimeter;
            _aircraftSystemData = _injector.equipmentData;
            _fcu = _injector.fcu;
            _flaps = _injector.flaps;
            _eventBus = _injector.systemEventBus;

            _eventBus.RegisterSaccEvent(this);

            _localPlayer = Networking.LocalPlayer;

            // Reset Flight Direction and Landing System
            flightDirectionIndicator.SetActive(isFlightDirectionOn);
            flightDirectionFail.SetActive(isFlightDirectionOn);

            landingSystem.SetActive(isLandingSystemOn);
            landingSystemIndicator.SetActive(isLandingSystemOn);
        }

        public void SFEXT_O_RespawnButton() {
            isFlightDirectionOn = true;
            isLandingSystemOn = false;

            flightDirectionIndicator.SetActive(isFlightDirectionOn);
            flightDirectionFail.SetActive(isFlightDirectionOn);

            landingSystem.SetActive(isLandingSystemOn);
            landingSystemIndicator.SetActive(isLandingSystemOn);
        }

    #region Math

        private static float Remap01(float value, float valueMin, float valueMax) {
            value = Mathf.Clamp01((value - valueMin) / (valueMax - valueMin));
            return value;
        }

    #endregion

    #region UI Elements

        [Header("UI Elements")]
        public GameObject VSbackground;

        public Text VSText;
        public Text RadioHeightText;
        public Text MeterAltitudeText;
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

        public GameObject pilotInputDisplay;

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

    #region Animation Hashs

        // animator strings that are sent every frame are converted to int for optimization
        private readonly int AIRSPEED_HASH = Animator.StringToHash("AirSpeedNormalize");
        private readonly int AIRSPEED_SECLECT_HASH = Animator.StringToHash("AirSpeedSelectNormalize");
        private readonly int PITCH_HASH = Animator.StringToHash("PitchAngleNormalize");
        private readonly int BANK_HASH = Animator.StringToHash("BankAngleNormalize");
        private readonly int ALT_HASH = Animator.StringToHash("AltitudeNormalize");
        private readonly int ALT10_HASH = Animator.StringToHash("Altitude10Normalize");
        private readonly int ALT100_HASH = Animator.StringToHash("Altitude100Normalize");
        private readonly int ALT1000_HASH = Animator.StringToHash("Altitude1000Normalize");
        private readonly int ALT10000_HASH = Animator.StringToHash("Altitude10000Normalize");
        private readonly int ROC_HASH = Animator.StringToHash("VerticalSpeedNormalize");
        private readonly int HEADING_HASH = Animator.StringToHash("HeadingNormalize");
        private readonly int SLIP_ANGLE_HASH = Animator.StringToHash("SlipAngleNormalize");
        private readonly int RH_HASH = Animator.StringToHash("RHNormalize");
        private readonly int TRKPCH_HASH = Animator.StringToHash("TRKPCHNormalize");
        private readonly int VMAX_HASH = Animator.StringToHash("VMAXNormalize");
        private readonly int VSW_HASH = Animator.StringToHash("VSWNormalize");
        private readonly int VFE_NEXT_HASH = Animator.StringToHash("VFENEXTNormalize");
        private readonly int VLS_HASH = Animator.StringToHash("VLSNormalize");
        private readonly int INPUT_X_HASH = Animator.StringToHash("PilotInputX");
        private readonly int INPUT_Y_HASH = Animator.StringToHash("PilotInputY");

    #endregion

    #region Update

        private void LateUpdate() {
            if (!UpdateIntervalUtil.CanUpdate(ref _lastUpdate, UPDATE_INTERVAL)) return;
            
            //这里可以用来做仪表更新延迟之类的逻辑
            PitchAngle = _adiru.irs.pitch;
            BankAngle = _adiru.irs.bank;
            RadioHeight = _radioAltimeter.radioAltitude;
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

            UpdatePilotInput();
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

        private void UpdateAirspeed() {
            foreach (var item in disableOnGround) item.SetActive(!_aircraftSystemData.isAircraftGrounded);
            foreach (var item in enableOnGround) item.SetActive(_aircraftSystemData.isAircraftGrounded);

            IndicatorAnimator.SetFloat(AIRSPEED_HASH, _adiru.adr.instrumentAirSpeed / MAXSPEED);

        #region Target Speed

            IndicatorAnimator.SetFloat(AIRSPEED_SECLECT_HASH, _fcu.TargetSpeed / 500f);

            TargetSpeedTopText.text = _fcu.TargetSpeed.ToString();
            TargetSpeedBottomText.text = _fcu.TargetSpeed.ToString();

            TargetSpeedBottom.SetActive(false);
            TargetSpeedTop.SetActive(false);
            if (_adiru.adr.instrumentAirSpeed - _fcu.TargetSpeed > 45)
                TargetSpeedBottom.SetActive(true);

            if (_fcu.TargetSpeed - _adiru.adr.instrumentAirSpeed > 45)
                TargetSpeedTop.SetActive(true);

        #endregion

        #region VMAX

            var VMAX = VMO;
            if (_aircraftSystemData.flapTargetSpeedLimit < VMAX)
                VMAX = (int)_flaps.targetSpeedLimit;

            if (_aircraftSystemData.flapCurrentSpeedLimit < VMAX)
                VMAX = (int)_flaps.speedLimit;

            if (!_aircraftSystemData.isGearsUp && VLE < VMAX)
                VMAX = VLE;

            IndicatorAnimator.SetFloat(VMAX_HASH, VMAX / 360f);

        #endregion

        #region VSW

            var VSW = VSWCONF0;
            switch (_aircraftSystemData.flapCurrentIndex) {
                case 1:
                    VSW = VSWCONF1;
                    break;
                case 2:
                    VSW = VSWCONF2;
                    break;
                case 3:
                    VSW = VSWCONF3;
                    break;
                case 4:
                    VSW = VSWCONFFULL;
                    break;
            }

            IndicatorAnimator.SetFloat(VSW_HASH, VSW / 300f);

        #endregion

        #region VFE NEXT

            var VFENext = _flaps.speedLimits[1];
            switch (_flaps.targetDetentIndex) {
                case 1:
                    VFENext = _flaps.speedLimits[2];
                    break;
                case 2:
                    VFENext = _flaps.speedLimits[3];
                    break;
                case 3:
                    VFENext = _flaps.speedLimits[4];
                    break;
            }

            IndicatorAnimator.SetFloat(VFE_NEXT_HASH, VFENext / 240f);

        #endregion

        #region VLS

            var VLS = 1.28f * VSW;
            switch (_flaps.detentIndex) {
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

        private void UpdateMachNumber() {
            if (_adiru.adr.mach > 0.5f) {
                MachNumberText.gameObject.SetActive(true);
                MachNumberText.text = "." + (_adiru.adr.mach*100f).ToString("f0");
            }
            else {
                MachNumberText.gameObject.SetActive(false);
            }
        }

    #endregion

    #region Altitude

        private void UpdateAltitude() {
            //默认都会写Altitude
            _altitude = _adiru.adr.pressureAltitude;
            IndicatorAnimator.SetFloat(ALT_HASH, _altitude / MAXALT);

            var meterAltitude = (int)(_altitude / 3.28084f);
            MeterAltitudeText.text = $"{meterAltitude - meterAltitude % 10} <color=#FFFFFF>M</color>";

            if (!altbybit) return;
            IndicatorAnimator.SetFloat(ALT10_HASH, _altitude % 100 / 100f);
            IndicatorAnimator.SetFloat(ALT100_HASH, (int)(_altitude / 100f) % 10 / 10f);
            IndicatorAnimator.SetFloat(ALT1000_HASH, (int)(_altitude / 1000f) % 10 / 10f);
            IndicatorAnimator.SetFloat(ALT10000_HASH, (int)(_altitude / 10000f) % 10 / 10f);
        }

        private void UpdateRadioHeight() {
            if (!_radioAltimeter.isAvailable | RadioHeight > 2500f) {
                RadioHeightText.gameObject.SetActive(false);
            }
            else {
                RadioHeightText.gameObject.SetActive(true);
                RadioHeightText.text = RadioHeight.ToString("f0");
                var RadioAltitudeNormal = Remap01(RadioHeight, -MAXRHE, MAXRHE);
                IndicatorAnimator.SetFloat(RH_HASH, RadioAltitudeNormal);
            }
        }

    #endregion

        private void UpdateVerticalSpeed() {
            var verticalSpeed = _adiru.adr.verticalSpeed;
            var VerticalSpeedNormal = Remap01(_adiru.adr.verticalSpeed, -MAXVS, MAXVS);
            IndicatorAnimator.SetFloat(ROC_HASH, VerticalSpeedNormal);
            if (Mathf.Abs(verticalSpeed) > 200) {
                VSbackground.SetActive(true);
                if (Mathf.Abs(verticalSpeed) > 6000)
                    VSText.color = new Color(0.91373f, 0.54901f, 0);
                else if (1000 < RadioHeight && RadioHeight < 2000 && Mathf.Abs(verticalSpeed) > 2000)
                    VSText.color = new Color(0.91373f, 0.54901f, 0);
                else if (RadioHeight < 1200 && Mathf.Abs(verticalSpeed) > 1200)
                    VSText.color = new Color(0.91373f, 0.54901f, 0);
                else
                    VSText.color = new Color(0, 1, 0);

                VSText.text = Mathf.Abs(verticalSpeed / 100).ToString("f0");
            }
            else {
                VSbackground.gameObject.SetActive(false);
            }
        }

        private void UpdateHeading() {
            IndicatorAnimator.SetFloat(HEADING_HASH, (_adiru.irs.heading - HDGoffset + 360) % 360 / 360f);
        }

        private void UpdatePitch() {
            //玄学问题，Pitch 跟 Bank 调用不了Remap01??
            var PitchAngleNormal = Mathf.Clamp01((PitchAngle + MAXPITCH) / (MAXPITCH + MAXPITCH));
            IndicatorAnimator.SetFloat(PITCH_HASH, PitchAngleNormal);
        }

        private void UpdateBank() {
            var BankAngleNormal = Mathf.Clamp01((BankAngle + MAXBANK) / (MAXBANK + MAXBANK));
            IndicatorAnimator.SetFloat(BANK_HASH, BankAngleNormal);
        }

        private void UpdateSlip() {
            IndicatorAnimator.SetFloat(SLIP_ANGLE_HASH,
                Mathf.Clamp01((_adiru.irs.trackSlipAngle + MAXSLIPANGLE) / (MAXSLIPANGLE + MAXSLIPANGLE)));
        }

        private void UpdateTrickPitch() {
            IndicatorAnimator.SetFloat(TRKPCH_HASH,
                Mathf.Clamp01((_adiru.irs.trackPitchAngle + MAXTRACKPITCH) / (MAXTRACKPITCH + MAXTRACKPITCH)));
        }

        private Vector3 ownerRotationInputs;

        private void UpdatePilotInput() {
            pilotInputDisplay.SetActive(_aircraftSystemData.isAircraftGrounded &&
                                        (_aircraftSystemData.isEngine1Avail || _aircraftSystemData.isEngine2Avail));

            if (!pilotInputDisplay.activeSelf) return;

            var rotationInputs = _aircraftSystemData.pilotInput;
            if (_aircraftSystemData.isOwner) {
                if (_localPlayer.IsUserInVR()) {
                    ownerRotationInputs = rotationInputs;
                }
                else {
                    // prevent instant movement in desktop mode
                    ownerRotationInputs = Vector3.MoveTowards(ownerRotationInputs, rotationInputs, 7 * Time.deltaTime);
                }

                IndicatorAnimator.SetFloat(INPUT_Y_HASH, ownerRotationInputs.x * 0.5f + 0.5f);
                IndicatorAnimator.SetFloat(INPUT_X_HASH, ownerRotationInputs.z * 0.5f + 0.5f);
                
                return;
            }

            IndicatorAnimator.SetFloat(INPUT_Y_HASH, rotationInputs.x * 0.5f + 0.5f);
            IndicatorAnimator.SetFloat(INPUT_X_HASH, rotationInputs.z * 0.5f + 0.5f);
        }

    #endregion

    #region Touch Switch Event

        [PublicAPI]
        public void ToggleFlightDirection() {
            isFlightDirectionOn = !isFlightDirectionOn;
            flightDirectionIndicator.SetActive(isFlightDirectionOn);
            flightDirectionFail.SetActive(isFlightDirectionOn);
        }

        [PublicAPI]
        public void ToggleLandingSystem() {
            isLandingSystemOn = !isLandingSystemOn;
            landingSystem.SetActive(isLandingSystemOn);
            landingSystemIndicator.SetActive(isLandingSystemOn);
        }

    #endregion
    }
}