using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using SaccFlightAndVehicles;
using EsnyaSFAddons.SFEXT;
using EsnyaSFAddons.DFUNC;
using A320VAU.Common;
using A320VAU.FWS;

namespace A320VAU.ECAM
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public partial class ECAMController : UdonSharpBehaviour
    {
        public SaccAirVehicle airVehicle;
        public AirbusAvionicsTheme AirbusAvionicsTheme;
        public FWS.FWS FWS;
        [Header("Engines")]
        public SFEXT_AdvancedEngine EngineControllorL;
        public SFEXT_AdvancedEngine EngineControllorR;

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

        [Header("APU")]
        public SFEXT_AuxiliaryPowerUnit APUControllor;
        private bool IsAPUStart;

        [Header("Flaps")]
        public DFUNC_AdvancedFlaps flapController;
        public Text flapText;
        //1+f的操作交给ecam完成

        [Header("Clock")]
        public Text HHMMText;
        public Text SSText;

        [Header("Memos")]
        public ChecklistItem[] Checklists;
        public ChecklistItem[] ActiveChecklists = new ChecklistItem[0];

        public Text LeftMemoText;
        public Text RightMemoText;

        [Header("Animator")]
        public Animator ECAMAnimator;
        private int ENG1N1_HASH = Animator.StringToHash("ENG1N1");
        private int ENG2N1_HASH = Animator.StringToHash("ENG2N1");
        private int ENG1EGT_HASH = Animator.StringToHash("ENG1EGT");
        private int ENG2EGT_HASH = Animator.StringToHash("ENG2EGT");
        private int ENG1N1CMD_HASH = Animator.StringToHash("ENG1N1Cmd");
        private int ENG2N1CMD_HASH = Animator.StringToHash("ENG2N1Cmd");
        private int FLAP_HASH = Animator.StringToHash("flapPos");
        private float N1RefMax = 1.2f;
        private float EgtMax = 1000f;

        private bool isStartingL = false;
        private bool isStartingR = false;

        [UdonSynced, FieldChangeCallback(nameof(IsLadingGearDown))]
        private bool _isLadingGearDown;
        public bool IsLadingGearDown
        {
            get => _isLadingGearDown;
            set
            {
                _isLadingGearDown = value;
                UpdateECAMMemo();
            }
        }

        public bool IsCrazyThrusday = false;

        [UdonSynced, FieldChangeCallback(nameof(IsSeatBeltsSignOn))]
        private bool _isSeatBeltsSignOn;
        public bool IsSeatBeltsSignOn
        {
            get => _isSeatBeltsSignOn;
            set
            {
                _isSeatBeltsSignOn = value;
                UpdateECAMMemo();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(IsNoSmoking))]
        private bool _isNoSmoking;
        public bool IsNoSmoking
        {
            get => _isNoSmoking;
            set
            {
                _isNoSmoking = value;
                UpdateECAMMemo();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(IsSplrsArmed))]
        private bool _isSplrsArmed;
        public bool IsSplrsArmed
        {
            get => _isSplrsArmed;
            set
            {
                _isSplrsArmed = value;
                UpdateECAMMemo();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(FlapPosition))]
        private int _flapPosition;
        public int FlapPosition
        {
            get => _flapPosition;
            set
            {
                _flapPosition = value;
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(FlapTargetPosition))]
        private int _flapTargetPosition;
        public int FlapTargetPosition
        {
            get => _flapTargetPosition;
            set
            {
                _flapTargetPosition = value;
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(IsCabinReady))]
        private bool _isCabinReady;
        public bool IsCabinReady
        {
            get => _isCabinReady;
            set
            {
                _isCabinReady = value;
                UpdateECAMMemo();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(IsParkbrakeSet))]
        private bool _isParkbrakeSet;
        public bool IsParkbrakeSet
        {
            get => _isParkbrakeSet;
            set
            {
                _isParkbrakeSet = value;
                UpdateECAMMemo();
            }
        }

        [UdonSynced, FieldChangeCallback(nameof(IsHookDown))]
        private bool _isHookDown;
        public bool IsHookDown
        {
            get => _isHookDown;
            set
            {
                _isHookDown = value;
                UpdateECAMMemo();
            }
        }

        public void Start()
        {
            IsParkbrakeSet = false;
            IsSeatBeltsSignOn = true;

            System.DateTime today = System.DateTime.Today;
            if (today.DayOfWeek == System.DayOfWeek.Thursday) IsCrazyThrusday = true;

            ActiveChecklists = new ChecklistItem[] {
                Checklists[1]
            };

            // UpdateChecklist();
            eng1AvailFlag.SetActive(false);
            eng2AvailFlag.SetActive(false);
            flapText.text = "0";
        }

        public void LateUpdate()
        {
            UpdateClock();
            UpdateEngineStatus();
            UpdateAPUStatus();
            // UpdateRightMemo();
            UpdateFlapStatus();
        }

        public override void OnDeserialization()
        {
            UpdateECAMMemo();
        }

        private void UpdateClock()
        {
            HHMMText.text = DateTime.UtcNow.ToShortTimeString();
            SSText.text = DateTime.UtcNow.Second.ToString("D2");
        }

        private void UpdateFlapStatus()
        {
            ECAMAnimator.SetFloat(FLAP_HASH, flapController.angle / flapController.maxAngle);

            if (FlapPosition != flapController.detentIndex)
            {
                FlapPosition = flapController.detentIndex;
                UpdateECAMMemo();
            }

            if (FlapTargetPosition != flapController.targetDetentIndex)
            {
                FlapTargetPosition = flapController.targetDetentIndex;
                switch (FlapTargetPosition)
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


        }

        private void UpdateEngineStatus()
        {
            if (!EngineControllorL.gameObject.activeInHierarchy)
            {
                return;
            }
            //左右好像反了...
            //1发
            float n1LRef = EngineControllorL.n1 / EngineControllorL.takeOffN1;
            float n2LRef = EngineControllorL.n2 / EngineControllorL.takeOffN2;
            N1L.text = (n1LRef * 100).ToString("F1");
            N2L.text = (n2LRef * 100).ToString("F1");
            EGTL.text = EngineControllorL.egt.ToString("F0");
            FFL.text = (Mathf.Round(EngineControllorL.ff / 20) * 20).ToString("F0");

            if (EngineControllorL.starter)
                StartingL.color = new Color(0.376f, 0.376f, 0.376f);
            else
                StartingL.color = new Color(0, 0, 0);

            ECAMAnimator.SetFloat(ENG1N1_HASH, n1LRef / (N1RefMax));
            ECAMAnimator.SetFloat(ENG1EGT_HASH, EngineControllorL.egt / EgtMax);
            ECAMAnimator.SetFloat(ENG1N1CMD_HASH, EngineControllorL.throttleInput);

            //AVAIL FLAG
            var isEng1Running = EngineControllorL.fuel && EngineControllorL.n1 > 0.63f * EngineControllorL.idleN1 && !EngineControllorL.stall;
            if (isEng1Running)
            {
                if (!isEng1RunnningLastFarme) eng1AvailFlag.SetActive(true);
                if (EngineControllorL.n1 > 0.9f * EngineControllorL.idleN1) eng1AvailFlag.SetActive(false);
            }
            else
                eng1AvailFlag.SetActive(false);
            isEng1RunnningLastFarme = isEng1Running;

            //2发
            float n1RRef = EngineControllorR.n1 / EngineControllorR.takeOffN1;
            float n2RRef = EngineControllorR.n2 / EngineControllorR.takeOffN2;
            N1R.text = (n1RRef * 100).ToString("F1");
            N2R.text = (n2RRef * 100).ToString("F1");
            EGTR.text = EngineControllorR.egt.ToString("F0");
            FFR.text = (Mathf.Round(EngineControllorR.ff / 20) * 20).ToString("F0");

            if (EngineControllorR.starter)
                StartingR.color = new Color(0.376f, 0.376f, 0.376f);
            else
                StartingR.color = new Color(0, 0, 0);

            ECAMAnimator.SetFloat(ENG2N1_HASH, n1RRef / (N1RefMax));
            ECAMAnimator.SetFloat(ENG2EGT_HASH, EngineControllorR.egt / EgtMax);
            ECAMAnimator.SetFloat(ENG2N1CMD_HASH, EngineControllorR.throttleInput);

            //AVAIL FLAG
            var isEng2Running = EngineControllorR.fuel && EngineControllorR.n1 > 0.63f * EngineControllorR.idleN1 && !EngineControllorR.stall;
            if (isEng2Running)
            {
                if (!isEng2RunnningLastFarme) eng2AvailFlag.SetActive(true);
                if (EngineControllorR.n1 > 0.9f * EngineControllorR.idleN1) eng2AvailFlag.SetActive(false);
            }
            else
                eng2AvailFlag.SetActive(false);
            isEng2RunnningLastFarme = isEng2Running;
        }

        private void UpdateAPUStatus()
        {
            if (IsAPUStart != APUControllor.run)
            {
                IsAPUStart = APUControllor.run;
                UpdateECAMMemo();
            }
        }

        // public void UpdateChecklist() {
        //     LeftMemoText.text = "";
        //     foreach (var item in ActiveChecklists) {
        //         var hasTitle = !string.IsNullOrEmpty(item.Title);

        //         if (hasTitle) {
        //             LeftMemoText.text += $"{item.Prefix} {item.Title}\n";
        //         } else {
        //             LeftMemoText.text += $"{item.Prefix} ";
        //         }

        //         var prefix = "".PadLeft((item.Prefix + " ").Length);

        //         for (int index = 0; index != item.CheckItems.Length; index++) {
        //             var checkitem = item.CheckItems[index];
        //             var checkItemTextLength = 20 + $"<color={MemoItemColor.Blue}>".Length - checkitem.ValueText.Length;
        //             var isChecked = GetProgramVariable(checkitem.PropertyName).ToString() == checkitem.Value;
        //             var checkItemText = "";

        //             if (!hasTitle && index == 0) {
        //                 checkItemTextLength -= (item.Prefix + " ").Length;
        //             } else {
        //                 checkItemText = prefix;
        //             }

        //             if (isChecked) {
        //                 checkItemText += $"{checkitem.Title} {checkitem.ValueText}\n";
        //             } else {
        //                 checkItemText += $"{checkitem.Title}<color={ MemoItemColor.Blue }>";
        //                 checkItemText = checkItemText.PadRight(checkItemTextLength, '.');
        //                 checkItemText += $"{checkitem.ValueText}</color>\n";
        //             }

        //             LeftMemoText.text += checkItemText;
        //         }
        //     }
        // }

        private readonly int SingleLineMaxLength = 24;
        public void UpdateMemo()
        {
            Debug.Log("Update memo");

            var memoText = "";
            foreach (var memo in FWS.FWSWarningMessageDatas)
            {
                if (memo.IsVisable)
                {
                    memoText += $"<color={getColorHexByWarningColor(memo.TitleColor)}>{memo.WarningGroup} {memo.WarningTitle}</color>\n";
                    foreach (var messageLine in memo.MessageLine)
                    {
                        if (messageLine.IsMessageVisable)
                        {
                            memoText += $"<color={getColorHexByWarningColor(messageLine.MessageColor)}>{messageLine.MessageText}</color>";
                            if (messageLine.MessageText.Length == SingleLineMaxLength)
                                memoText += "\n";
                        }
                    }
                }
            }

            LeftMemoText.text = memoText;
        }

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

        public void UpdateECAMMemo()
        {
            return;
            // UpdateLeftMemo();
            // UpdateRightMemo();
            // UpdateChecklist();
        }

        // public void UpdateLeftMemo() {
        //     var leftMemo = "";
        //     if (IsSplrsArmed) leftMemo += CreateECAMMemo(MemoItemColor.Green, "GND SPLRS ARMED");
        //     if (IsSeatBeltsSignOn) leftMemo += CreateECAMMemo(MemoItemColor.Green, "SEAT BELTS");
        //     if (IsNoSmoking) leftMemo += CreateECAMMemo(MemoItemColor.Green, "NO SMOKING");

        //     LeftMemoText.text = leftMemo;
        // }

        // public void UpdateRightMemo() {
        //     var rightMemo = "";
        //     if (IsCrazyThrusday) rightMemo += CreateECAMMemo(MemoItemColor.Green, "V ME 50!");
        //     if (IsParkbrakeSet) rightMemo += CreateECAMMemo(MemoItemColor.Green, "PARK BRAKE");
        //     if (IsCabinReady) rightMemo += CreateECAMMemo(MemoItemColor.Green, "CABIN READY");
        //     if (IsHookDown) rightMemo += CreateECAMMemo(MemoItemColor.Green, "HOOK");
        //     if (IsAPUStart) rightMemo += CreateECAMMemo(MemoItemColor.Green, "APU BLEED");
        //     RightMemoText.text = rightMemo;
        // }

        // public string CreateECAMMemo(string color, string text) {
        //     return $"<color={color}>{text}</color>\n";
        // }
    }
}
