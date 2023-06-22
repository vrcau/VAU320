using System;
using A320VAU.Common;
using UdonSharp;
using VirtualAviationJapan;

namespace A320VAU.MCDU
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class RadNavPage : MCDUPage
    {
        private FMGC.FMGC _fmgc;
        private MCDU _mcdu;
        private NavaidDatabase _navaidDatabase;

        private NavSelector _vor1;
        private NavSelector _vor2;
        private NavSelector _ils;
        private NavSelector _adf;

        private void Start()
        {
            _fmgc = DependenciesInjector.GetInstance(this).fmgc;

            _navaidDatabase = _fmgc.navaidDatabase;
            _vor1 = _fmgc.radNav.VOR1;
            _vor2 = _fmgc.radNav.VOR2;
            _ils = _fmgc.radNav.ILS;
            _adf = _fmgc.radNav.ADF;
        }

        public override void OnPageInit(MCDU mcdu)
        {
            _mcdu = mcdu;

            mcdu.titleLineText.text = "RADIO NAV";
            UpdateUI();
        }


        public override void OnPageUpdate() => UpdateUI();
        private void UpdateUI()
        {
            _mcdu.l1Label.text = "VOR1/FREQ";
            _mcdu.l1Text.text =
                $"<color=#30FFFF>{_vor1.Identity ?? "[  ]"}/{(_vor1.Index != -1 ? _navaidDatabase.frequencies[_vor1.Index].ToString("F") : "[  . ]")}</color>";
            _mcdu.l2Label.text = "CRS";
            _mcdu.l2Text.text = $"<color=#30FFFF>{(_vor1.Index != -1 ? _vor1.Course.ToString("000") : "[ ]")}</color>";
            _mcdu.l3Label.text = " LS /FREQ";
            _mcdu.l3Text.text = $"<color=#30FFFF>{_ils.Identity ?? "[  ]"}/{(_ils.Index != -1 ? _navaidDatabase.frequencies[_ils.Index].ToString("F") : "[  . ]")}</color>";
            _mcdu.l4Label.text = "CRS";
            _mcdu.l4Text.text = $"<color=#30FFFF>{(_ils.Index != -1 ? _ils.Course.ToString("000") : "[ ]")}</color>";
            _mcdu.l5Label.text = "ADF1/FREQ";
            _mcdu.l5Text.text = $"<color=#30FFFF>{_adf.Identity ?? "[  ]"}/{(_adf.Index != -1 ? _navaidDatabase.frequencies[_adf.Index].ToString("F") : "[  . ]")}</color>";

            _mcdu.r1Label.text = "VOR2/FREQ";
            _mcdu.r1Text.text = 
                $"<color=#30FFFF>{_vor2.Identity ?? "[  ]"}/{(_vor2.Index != -1 ? _navaidDatabase.frequencies[_vor2.Index].ToString("F") : "[  . ]")}</color>";
            _mcdu.r2Label.text = "CRS";
            _mcdu.r2Text.text = $"<color=#30FFFF>{(_vor2.Index != -1 ? _vor2.Course.ToString("000") : "[ ]")}</color>";
        }

        #region VOR
        // VOR1/FREQ
        public override void L1() => SetVORFrequency(1);
        // VOR1 CRS
        public override void L2() => SetVORCourse(1);

        // VOR2/FREQ
        public override void R1() => SetVORFrequency(2);
        // VOR 2 COURSE
        public override void R2() => SetVORCourse(2);
        #endregion

        #region LS
        // LS/FREQ
        public override void L3()
        {
            var input = _mcdu.scratchpad;
            
            if (MCDUInputValidationUtils.TryGetFrequency(input, out var frequency))
            {
                if (_fmgc.radNav.SetILSByFrequency(frequency))
                {
                    _mcdu.ClearInput();
                    UpdateUI();
                    return;
                }
                
                _mcdu.SendMCDUMessage("NOT IN DATABASE");
                return;
            }

            if (MCDUInputValidationUtils.ValidateNavaid(input))
            {
                if (_fmgc.radNav.SetILSByName(input))
                {
                    _mcdu.ClearInput();
                    UpdateUI();
                    return;
                }
                
                _mcdu.SendMCDUMessage("NOT IN DATABASE");
                return;
            }
            
            _mcdu.SendMCDUMessage("FORMAT ERROR");
        }

        // LS COURSE
        public override void L4()
        {
            if (MCDUInputValidationUtils.TryGetCourse(_mcdu.scratchpad, out var course))
            {
                _fmgc.radNav.SetILSCourse(course);
                UpdateUI();
                _mcdu.ClearInput();
                return;
            }
            
            _mcdu.SendMCDUMessage("FORMAT ERROR");
        }
        #endregion

        // ADF
        public override void L5()
        {
            var input = _mcdu.scratchpad;
            
            if (MCDUInputValidationUtils.TryGetFrequency(input, out var frequency))
            {
                if (_fmgc.radNav.SetADFByFrequency(frequency))
                {
                    UpdateUI();
                    _mcdu.ClearInput();
                    return;
                }
                
                _mcdu.SendMCDUMessage("NOT IN DATABASE");
                return;
            }

            if (MCDUInputValidationUtils.ValidateNavaid(input))
            {
                if (_fmgc.radNav.SetADFByName(input))
                {
                    UpdateUI();
                    _mcdu.ClearInput();
                    return;
                }
                
                _mcdu.SendMCDUMessage("NOT IN DATABASE");
                return;
            }
            
            _mcdu.SendMCDUMessage("FORMAT ERROR");
        }

        private void SetVORFrequency(int index)
        {
            var input = _mcdu.scratchpad;
            if (MCDUInputValidationUtils.TryGetFrequency(input, out var frequency))
            {
                if (_fmgc.radNav.SetVORByFrequency(index, frequency))
                {
                    UpdateUI();
                    _mcdu.ClearInput();
                    return;
                }

                _mcdu.SendMCDUMessage("NOT IN DATABASE");
                return;
            }

            if (MCDUInputValidationUtils.ValidateNavaid(input))
            {
                if (_fmgc.radNav.SetVORByName(index, input))
                {
                    UpdateUI();
                    _mcdu.ClearInput();
                    return;
                }
                
                _mcdu.SendMCDUMessage("NOT IN DATABASE");
                return;
            }
            
            _mcdu.SendMCDUMessage("FORMAT ERROR");
        }

        private void SetVORCourse(int index)
        {
            if (MCDUInputValidationUtils.TryGetCourse(_mcdu.scratchpad, out var course))
            {
                _fmgc.radNav.SetVORCourse(index, course);
                UpdateUI();
                _mcdu.ClearInput();
                return;
            }
            
            _mcdu.SendMCDUMessage("FORMAT ERROR");
        }
    }
}