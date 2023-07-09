using A320VAU.FWS;
using Avionics.Systems.Common;
using JetBrains.Annotations;
using SaccFlightAndVehicles;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace A320VAU.Common {
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class ECAMDisplay : UdonSharpBehaviour {
    #region Aircraft Systems

        private DependenciesInjector _injector;
        
        private SaccAirVehicle _airVehicle;
        private SaccEntity _saccEntity;
        private AircraftSystemData _aircraftSystemData;
        private AirbusAvionicsTheme _airbusAvionicsTheme;
        private FWS.FWS _fws;
    #endregion

    #region UI Elements
        [Header("Left Engine")]
        public Text N1L;

        public Text N2L;
        public Text EGTL;
        public Image StartingL;
        public Text FFL;
        public GameObject eng1AvailFlag;

        [Header("Right Engine")]
        public Text N1R;

        public Text N2R;
        public Text EGTR;
        public Image StartingR;
        public Text FFR;
        public GameObject eng2AvailFlag;
        
        [Header("Flap")]
        public Text flapText;

        [Header("Memo")]
        public Text LeftMemoText;
        public Text RightMemoText;
    #endregion
        
    #region Animation Hashs
        private readonly int ENG1N1_HASH = Animator.StringToHash("ENG1N1");
        private readonly int ENG2N1_HASH = Animator.StringToHash("ENG2N1");
        private readonly int ENG1EGT_HASH = Animator.StringToHash("ENG1EGT");
        private readonly int ENG2EGT_HASH = Animator.StringToHash("ENG2EGT");
        private readonly int ENG1N1CMD_HASH = Animator.StringToHash("ENG1N1Cmd");
        private readonly int ENG2N1CMD_HASH = Animator.StringToHash("ENG2N1Cmd");
        private readonly int FLAP_HASH = Animator.StringToHash("flapPos");
    #endregion
        
        [Header("Animator")]
        public Animator ECAMAnimator;
        
        // for warning text
        private const int SingleLineMaxLength = 24;
        
        // for engine started event
        private bool _isEngine1RunningLastFrame;
        private bool _isEngine2RunningLastFrame;
        
        // for instrument animation
        private const float MAX_EGT = 1000f;
        private const float MAX_N1 = 1.2f;

        private void Start() {
            _injector = DependenciesInjector.GetInstance(this);

            _airVehicle = _injector.saccAirVehicle;
            _saccEntity = _injector.saccEntity;
            _airbusAvionicsTheme = _injector.airbusAvionicsTheme;
            _aircraftSystemData = _injector.equipmentData;
            _fws = _injector.fws;
            
            eng1AvailFlag.SetActive(false);
            eng2AvailFlag.SetActive(false);
            flapText.text = "0";

            ResetAllPages();
            ToPage(SystemPage.Status);
            
            UpdateMemo();
            UpdateEngineStatus();
            UpdateFlapStatus(true);
        }
    #region Pages

        public GameObject enginePage;
        public GameObject statusPage;

        public GameObject enginePageIndicator;
        public GameObject statusPageIndicator;

        [PublicAPI] public SystemPage CurrentPage { get; private set; }
        [PublicAPI] public ECAMPage CurrentPageBehaviour { get; private set; }

    #endregion

    #region Update
        public void LateUpdate() {
            UpdateEngineStatus();
            UpdateFlapStatus();

            // Update ECAM SD Page
            if (CurrentPageBehaviour != null) CurrentPageBehaviour.OnPageUpdate();
        }

    #region EWD Update
        private void UpdateFlapStatus(bool forceUpdate = false) {
            if (forceUpdate || !_aircraftSystemData.flapInPosition) // Only Update when moving
            {
                flapText.color = _airbusAvionicsTheme.BlueColor;
                ECAMAnimator.SetFloat(FLAP_HASH, _aircraftSystemData.flapAngle);
                switch (_aircraftSystemData.flapTargetIndex) {
                    case 0:
                        flapText.text = "0";
                        break;
                    case 1:
                        flapText.text = _airVehicle.AirSpeed < 51.44f ? "1+F" : "1";
                        break;
                    case 2:
                        flapText.text = "2";
                        break;
                    case 3:
                        flapText.text = "3";
                        break;
                    case 4:
                        flapText.text = "FULL";
                        break;
                }
            }
            else {
                flapText.color = _airbusAvionicsTheme.GreenColor;
            }
        }

        private void UpdateEngineStatus() {
            N1L.text = (_aircraftSystemData.engine1n1 * 100).ToString("F1");
            N2L.text = (_aircraftSystemData.engine1n2 * 100).ToString("F1");
            EGTL.text = _aircraftSystemData.engine1EGT.ToString("F0");
            FFL.text = _aircraftSystemData.engine1fuelFlow.ToString("F0");

            StartingL.color = _aircraftSystemData.isEngine1Starting
                ? new Color(0.376f, 0.376f, 0.376f)
                : new Color(0, 0, 0);

            ECAMAnimator.SetFloat(ENG1N1_HASH, _aircraftSystemData.engine1n1 / MAX_N1);
            ECAMAnimator.SetFloat(ENG1EGT_HASH, _aircraftSystemData.engine1EGT / MAX_EGT);
            ECAMAnimator.SetFloat(ENG1N1CMD_HASH, _aircraftSystemData.engine1TargetN1);


            if (_aircraftSystemData.isEngine1Running) {
                if (!_isEngine1RunningLastFrame) {
                    eng1AvailFlag.SetActive(true);
                    _saccEntity.SendEventToExtensions("SFEXT_G_SFEXT_G_EngineStarted");
                }

                if (_aircraftSystemData.isEngine1Avail) eng1AvailFlag.SetActive(false);
            }
            else {
                if (_isEngine1RunningLastFrame) _saccEntity.SendEventToExtensions("SFEXT_G_EngineShutDown");
                eng1AvailFlag.SetActive(false);
            }

            _isEngine1RunningLastFrame = _aircraftSystemData.isEngine1Running;

            // 2发
            N1R.text = (_aircraftSystemData.engine2n1 * 100).ToString("F1");
            N2R.text = (_aircraftSystemData.engine2n2 * 100).ToString("F1");
            EGTR.text = _aircraftSystemData.engine2EGT.ToString("F0");
            FFR.text = _aircraftSystemData.engine2fuelFlow.ToString("F0");

            StartingR.color = _aircraftSystemData.isEngine2Starting
                ? new Color(0.376f, 0.376f, 0.376f)
                : new Color(0, 0, 0);

            ECAMAnimator.SetFloat(ENG2N1_HASH, _aircraftSystemData.engine2n1 / MAX_N1);
            ECAMAnimator.SetFloat(ENG2EGT_HASH, _aircraftSystemData.engine2EGT / MAX_EGT);
            ECAMAnimator.SetFloat(ENG2N1CMD_HASH, _aircraftSystemData.engine2TargetN1);

            // AVAIL FLAG
            if (_aircraftSystemData.isEngine2Running) {
                if (!_isEngine2RunningLastFrame) {
                    eng2AvailFlag.SetActive(true);
                    _saccEntity.SendEventToExtensions("SFEXT_G_SFEXT_G_EngineStarted");
                }

                if (_aircraftSystemData.isEngine2Avail) eng2AvailFlag.SetActive(false);
            }
            else {
                if (_isEngine2RunningLastFrame) _saccEntity.SendEventToExtensions("SFEXT_G_EngineShutDown");
                eng2AvailFlag.SetActive(false);
            }

            _isEngine2RunningLastFrame = _aircraftSystemData.isEngine2Running;
        }

        public void UpdateMemo() {
            var rightMemoText = "";
            var leftMemoText = "";
            var hasWarning = false;
            foreach (var memo in _fws.fwsWarningMessageDatas)
                if (memo.isVisable)
                    switch (memo.Type) {
                        // Like LDG INHIBIT, T.O INHIBIT, LAND ASAP
                        case WarningType.SpecialLine:
                            rightMemoText +=
                                $"<color={GetColorHexByWarningColor(memo.TitleColor)}>{memo.WarningTitle}</color>\n";
                            break;
                        // Like GND SPRLS AMRED, APU BLEED
                        case WarningType.Memo:
                            switch (memo.Zone) {
                                // Left of the ECAM
                                case DisplayZone.Left:
                                    if (!hasWarning)
                                        leftMemoText +=
                                            $"<color={GetColorHexByWarningColor(memo.TitleColor)}>{memo.WarningTitle}</color>\n";
                                    break;
                                // Right of the ECAM
                                case DisplayZone.Right:
                                    rightMemoText +=
                                        $"<color={GetColorHexByWarningColor(memo.TitleColor)}>{memo.WarningTitle}</color>\n";
                                    break;
                            }

                            break;
                        // System failure cased by other System failure, Like *HYD *F/CTL
                        case WarningType.Secondary:
                            rightMemoText +=
                                $"<color={GetColorHexByWarningColor(memo.TitleColor)}>{memo.WarningTitle}</color>\n";
                            break;
                        // Config Memo (Like T.O CONFIG, LDG CONFIG MEMO) and Primary System failure (Like ENG1 FIRE)
                        default:
                            if (memo.Type != WarningType.ConfigMemo) hasWarning = true;
                            // Do not show Config Memo when already a System Failure Warning visable
                            if (!((memo.Type == WarningType.ConfigMemo) & hasWarning)) {
                                leftMemoText +=
                                    $"<color={GetColorHexByWarningColor(memo.TitleColor)}>{memo.WarningGroup} {memo.WarningTitle}</color>";
                                // Config Memo don't require title warp
                                if (memo.Type != WarningType.ConfigMemo) leftMemoText += "\n";
                                var lastLineLength = 0;
                                foreach (var messageLine in memo.MessageLine)
                                    if (messageLine.isMessageVisible) {
                                        leftMemoText +=
                                            $"<color={GetColorHexByWarningColor(messageLine.MessageColor)}>{messageLine.MessageText}</color>";
                                        // Warp when a single line text length >= 24
                                        if (lastLineLength + messageLine.MessageText.Length >= SingleLineMaxLength) {
                                            leftMemoText += "\n";
                                            lastLineLength = 0;
                                        }
                                        else {
                                            lastLineLength = messageLine.MessageText.Length;
                                        }
                                    }
                            }

                            break;
                    }

            LeftMemoText.text = leftMemoText;
            RightMemoText.text = rightMemoText;
        }

    #endregion

    #endregion

    #region Page Navigation

    #region Button Functions

        [PublicAPI]
        public void ToggleEnginePage() {
            TogglePage(SystemPage.Engine);
        }
        
        [PublicAPI]
        public void ToggleStatusPage() {
            TogglePage(SystemPage.Status);
        }

    #endregion
        
        private void TogglePage(SystemPage page) {
            if (CurrentPage != page) {
                ToPage(page);
                return;
            }

            ToPage(SystemPage.None);
        }

        [PublicAPI]
        public void ToPage(SystemPage page) {
            ResetAllPages();
            if (page != SystemPage.None)
                CurrentPage = page;

            switch (page) {
                // IDLE Auto
                case SystemPage.None:
                    // fallback for now
                    CurrentPage = SystemPage.Status;
                    ToPage(statusPage);
                    statusPageIndicator.SetActive(true);
                    break;
                case SystemPage.Engine:
                    ToPage(enginePage);
                    enginePageIndicator.SetActive(true);
                    break;
                case SystemPage.Status:
                    ToPage(statusPage);
                    statusPageIndicator.SetActive(true);
                    break;
            }
        }

        private void ToPage(GameObject pageObject) {
            CurrentPageBehaviour = pageObject.GetComponent<ECAMPage>();
            pageObject.SetActive(true);
        }

        private void ResetAllPages() {
            enginePageIndicator.SetActive(false);
            statusPageIndicator.SetActive(false);

            CurrentPage = SystemPage.None;

            enginePage.SetActive(false);
            statusPage.SetActive(false);
        }

    #endregion

    #region Utils
        private static string GetColorHexByWarningColor(WarningColor color) {
            switch (color) {
                case WarningColor.Amber:
                    return AirbusAvionicsTheme.Amber;
                case WarningColor.Danger:
                    return AirbusAvionicsTheme.Danger;
                case WarningColor.Green:
                    return AirbusAvionicsTheme.Green;
                case WarningColor.Blue:
                    return AirbusAvionicsTheme.Blue;
                case WarningColor.White:
                    return "#FFFFFF";
                default:
                    return AirbusAvionicsTheme.Green;
            }
        }
    #endregion
    }
}