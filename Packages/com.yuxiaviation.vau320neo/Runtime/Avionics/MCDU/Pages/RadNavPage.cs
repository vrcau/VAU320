using System;
using A320VAU.Common;
using UdonSharp;

namespace A320VAU.MCDU
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class RadNavPage : MCDUPage
    {
        private FMGC.FMGC _fmgc;
        private MCDU _mcdu;

        private void Start()
        {
            _fmgc = DependenciesInjector.GetInstance(this).fmgc;
        }

        public override void OnPageInit(MCDU mcdu)
        {
            _mcdu = mcdu;
            
            mcdu.titleLineText.text = "RADIO NAV";
            UpdateUI();
        }

        private void UpdateUI()
        {
            _mcdu.l1Label.text = "VOR1/FREQ";
            _mcdu.l1Text.text = "<color=#30FFFF>[  ]/[  . ]</color>";
            _mcdu.l2Label.text = "CRS";
            _mcdu.l2Text.text = "<color=#30FFFF>[ ]</color>";
            _mcdu.l3Label.text = " LS /FREQ";
            _mcdu.l3Text.text = "<color=#30FFFF>[  ]/[  . ]</color>";
            _mcdu.l4Label.text = "CRS";
            _mcdu.l4Text.text = "<color=#30FFFF>[ ]</color>";
            _mcdu.l5Label.text = "ADF1/FREQ";
            _mcdu.l5Text.text = "<color=#30FFFF>[  ]/[  . ]</color>";

            _mcdu.r1Label.text = "VOR2/FREQ";
            _mcdu.r1Text.text = "<color=#30FFFF>[  . ]/[  ]</color>";
            _mcdu.r2Label.text = "CRS";
            _mcdu.r2Text.text = "<color=#30FFFF>[ ]</color>";
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
                _mcdu.ClearInput();
                return;
            }
            
            _mcdu.SendMCDUMessage("FORMAT ERROR");
        }
    }
}