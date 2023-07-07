using System;
using System.Runtime.CompilerServices;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using SaccFlightAndVehicles;
using EsnyaSFAddons.SFEXT;
using EsnyaSFAddons.DFUNC;
using A320VAU.SFEXT;
using A320VAU.Common;
using A320VAU.FWS;
using VRC.Udon.Common.Interfaces;

namespace A320VAU.ECAM
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
    public class ECAMDisplay : UdonSharpBehaviour
    {
        #region Aircraft Systems
        public SaccAirVehicle airVehicle;
        public SaccEntity EntityControl;
        public ECAMDataInterface AdvancedData;
        public AirbusAvionicsTheme AirbusAvionicsTheme;
        public FWS.FWS FWS;
        #endregion

        #region UI Elements
        [Header("Left Engine")]
        public Text N1L;
        public Text N2L;
        public Text EGTL;
        public Image StartingL;
        public Text FFL;
        public GameObject eng1AvailFlag;
        [NonSerialized] public bool isEng1RunnningLastFarme = false;

        [Header("Right Engine")]
        public Text N1R;
        public Text N2R;
        public Text EGTR;
        public Image StartingR;
        public Text FFR;
        public GameObject eng2AvailFlag;
        [NonSerialized] public bool isEng2RunnningLastFarme = false;

        [Header("Flap")]
        public Text flapText;
        
        [Header("APU")]
        public SFEXT_AuxiliaryPowerUnit APUControllor;
        private bool IsAPUStart;

        public Text LeftMemoText;
        public Text RightMemoText;
        #endregion

        #region Animation 
        [Header("Animator")]
        public Animator ECAMAnimator;
        private readonly int ENG1N1_HASH = Animator.StringToHash("ENG1N1");
        private readonly int ENG2N1_HASH = Animator.StringToHash("ENG2N1");
        private readonly int ENG1EGT_HASH = Animator.StringToHash("ENG1EGT");
        private readonly int ENG2EGT_HASH = Animator.StringToHash("ENG2EGT");
        private readonly int ENG1N1CMD_HASH = Animator.StringToHash("ENG1N1Cmd");
        private readonly int ENG2N1CMD_HASH = Animator.StringToHash("ENG2N1Cmd");
        private readonly int FLAP_HASH = Animator.StringToHash("flapPos");
        #endregion

        #region Pages
        public GameObject enginePage;
        public GameObject statusPage;

        public GameObject enginePageIndicator;
        public GameObject statusPageIndicator;

        public SystemPage CurrentPage { get; private set; }
        public ECAMPage CurrentPageBehaviour { get; private set; }
        #endregion
        
        private float N1RefMax = 1.2f;
        private float EgtMax = 1000f;

        public void Start()
        {
            eng1AvailFlag.SetActive(false);
            eng2AvailFlag.SetActive(false);
            flapText.text = "0";

            ResetAllPages();
            ToPage(SystemPage.Status);
            UpdateMemo();
        }

        private void OnEnable()
        {
            UpdateEngineStatus();
            UpdateFlapStatus(true);
        }

        #region Update
        public void LateUpdate()
        {
            UpdateEngineStatus();
            UpdateFlapStatus();

            if (CurrentPageBehaviour != null) CurrentPageBehaviour.OnPageUpdate();
        }

        #region EWD Update
        private void UpdateFlapStatus(bool forceUpdate = false)
        {
            if (forceUpdate || !AdvancedData.FlapInPosition) // Only Upadte when moving
            {
                flapText.color = AirbusAvionicsTheme.BlueColor;
                ECAMAnimator.SetFloat(FLAP_HASH, AdvancedData.FlapRefAngle);
                switch (AdvancedData.FlapTargetPosition)
                {
                    case 0:
                        flapText.text = "0";
                        break;
                    case 1:
                        if (airVehicle.AirSpeed < 51.44f)
                            flapText.text = "1+F";
                        else
                            flapText.text = "1";
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
            else
                flapText.color = AirbusAvionicsTheme.GreenColor;
        }

        private void UpdateEngineStatus()
        {
            N1L.text = (AdvancedData.N1LRef * 100).ToString("F1");
            N2L.text = (AdvancedData.N2LRef * 100).ToString("F1");
            EGTL.text = AdvancedData.EGTL.ToString("F0");
            FFL.text = AdvancedData.FuelFlowL.ToString("F0");

            if (AdvancedData.IsEngineLStarting)
                StartingL.color = new Color(0.376f, 0.376f, 0.376f);
            else
                StartingL.color = new Color(0, 0, 0);

            ECAMAnimator.SetFloat(ENG1N1_HASH, AdvancedData.N1LRef / (N1RefMax));
            ECAMAnimator.SetFloat(ENG1EGT_HASH, AdvancedData.EGTL / EgtMax);
            ECAMAnimator.SetFloat(ENG1N1CMD_HASH, AdvancedData.TargetRefN1L);

         
            if (AdvancedData.IsEngineLRunning)
            {
                if (!isEng1RunnningLastFarme)
                {
                    eng1AvailFlag.SetActive(true);
                    EntityControl.SendEventToExtensions("SFEXT_G_SFEXT_G_EngineStarted");
                }
                if (AdvancedData.EngineL.n1 > 0.9f * AdvancedData.EngineL.idleN1) eng1AvailFlag.SetActive(false);
            }
            else
            {
                if (isEng1RunnningLastFarme) EntityControl.SendEventToExtensions("SFEXT_G_EngineShutDown");
                eng1AvailFlag.SetActive(false);
            }
            isEng1RunnningLastFarme = AdvancedData.IsEngineLRunning;

            // 2发
            N1R.text = (AdvancedData.N1RRef * 100).ToString("F1");
            N2R.text = (AdvancedData.N2RRef * 100).ToString("F1");
            EGTR.text = AdvancedData.EGTR.ToString("F0");
            FFR.text = AdvancedData.FuelFlowR.ToString("F0");

            if (AdvancedData.IsEngineRStarting)
                StartingR.color = new Color(0.376f, 0.376f, 0.376f);
            else
                StartingR.color = new Color(0, 0, 0);

            ECAMAnimator.SetFloat(ENG2N1_HASH, AdvancedData.N1RRef / (N1RefMax));
            ECAMAnimator.SetFloat(ENG2EGT_HASH, AdvancedData.EGTR / EgtMax);
            ECAMAnimator.SetFloat(ENG2N1CMD_HASH, AdvancedData.TargetRefN1R);

            // AVAIL FLAG
            if (AdvancedData.IsEngineRRunning)
            {
                if (!isEng2RunnningLastFarme)
                {
                    eng2AvailFlag.SetActive(true);
                    EntityControl.SendEventToExtensions("SFEXT_G_SFEXT_G_EngineStarted");
                }
                if (AdvancedData.EngineR.n1 > 0.9f * AdvancedData.EngineR.idleN1) eng2AvailFlag.SetActive(false);
            }
            else
            {
                if (isEng2RunnningLastFarme) EntityControl.SendEventToExtensions("SFEXT_G_EngineShutDown");
                eng2AvailFlag.SetActive(false);
            }

            isEng2RunnningLastFarme = AdvancedData.IsEngineRRunning;
        }

        private readonly int SingleLineMaxLength = 24;
        public void UpdateMemo()
        {
            var rightMemoText = "";
            var leftMemoText = "";
            var hasWarning = false;
            foreach (var memo in FWS.fwsWarningMessageDatas)
            {
                if (memo.isVisable)
                {
                    switch (memo.Type)
                    {
                        // Like LDG INHIBIT, T.O INHIBIT, LAND ASAP
                        case WarningType.SpecialLine:
                            rightMemoText += $"<color={getColorHexByWarningColor(memo.TitleColor)}>{memo.WarningTitle}</color>\n";
                            break;
                        // Like GND SPRLS AMRED, APU BLEED
                        case WarningType.Memo:
                            switch (memo.Zone)
                            {
                                // Left of the ECAM
                                case DisplayZone.Left:
                                    if (!hasWarning)
                                    {
                                        leftMemoText += $"<color={getColorHexByWarningColor(memo.TitleColor)}>{memo.WarningTitle}</color>\n";
                                    }
                                    break;
                                // Right of the ECAM
                                case DisplayZone.Right:
                                    rightMemoText += $"<color={getColorHexByWarningColor(memo.TitleColor)}>{memo.WarningTitle}</color>\n";
                                    break;
                            }
                            break;
                        // System failure casued by other System failure, Like *HYD *F/CTL
                        case WarningType.Secondary:
                            rightMemoText += $"<color={getColorHexByWarningColor(memo.TitleColor)}>{memo.WarningTitle}</color>\n";
                            break;
                        // Config Memo (Like T.O CONFIG, LDG CONFIG MEMO) and Primary System failure (Like ENG1 FIRE)
                        default:
                            if (memo.Type != WarningType.ConfigMemo) hasWarning = true;
                            // Do not show Config Memo when already a System Failure Warning visable
                            if (!(memo.Type == WarningType.ConfigMemo & hasWarning))
                            {
                                leftMemoText += $"<color={getColorHexByWarningColor(memo.TitleColor)}>{memo.WarningGroup} {memo.WarningTitle}</color>";
                                // Config Memo don't require title warp
                                if (memo.Type != WarningType.ConfigMemo) leftMemoText += "\n";
                                var lastLineLength = 0;
                                foreach (var messageLine in memo.MessageLine)
                                {
                                    if (messageLine.isMessageVisible)
                                    {
                                        leftMemoText += $"<color={getColorHexByWarningColor(messageLine.MessageColor)}>{messageLine.MessageText}</color>";
                                        // Warp when a single line text length >= 24
                                        if (lastLineLength + messageLine.MessageText.Length >= SingleLineMaxLength)
                                        {
                                            leftMemoText += "\n";
                                            lastLineLength = 0;
                                        }
                                        else
                                        {
                                            lastLineLength = messageLine.MessageText.Length;
                                        }
                                    }
                                }
                            }
                            break;
                    }
                }

            }

            LeftMemoText.text = leftMemoText;
            RightMemoText.text = rightMemoText;
        }
        #endregion
        #endregion

        #region Page Navigation
        #region Button Functions
        // ReSharper disable once UnusedMember.Global
        public void ToggleEnginePage() => TogglePage(SystemPage.Engine);
        // ReSharper disable once UnusedMember.Global
        public void ToggleStatusPage() => TogglePage(SystemPage.Status);
        #endregion
        
        private void TogglePage(SystemPage page)
        {
            if (CurrentPage != page)
            {
                ToPage(page);
                return;
            }
            
            ToPage(SystemPage.None);
        }
        
        public void ToPage(SystemPage page)
        {
            ResetAllPages();
            if (page != SystemPage.None)
                CurrentPage = page;

            switch (page)
            {
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

        private void ToPage(GameObject pageObject)
        {
            CurrentPageBehaviour = pageObject.GetComponent<ECAMPage>();
            pageObject.SetActive(true);
        }

        private void ResetAllPages()
        {
            enginePageIndicator.SetActive(false);
            statusPageIndicator.SetActive(false);
            
            CurrentPage = SystemPage.None;
            
            enginePage.SetActive(false);
            statusPage.SetActive(false);
        }
        #endregion

        private string getColorHexByWarningColor(WarningColor color)
        {
            switch (color)
            {
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
            };
        }
    }
}
