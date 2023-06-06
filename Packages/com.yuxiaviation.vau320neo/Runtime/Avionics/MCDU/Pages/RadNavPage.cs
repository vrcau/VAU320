using UdonSharp;

namespace A320VAU.MCDU
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class RadNavPage : MCDUPage
    {
        public FMGC.FMGC fmgc;
        private MCDU _mcdu;
        
        public override void OnPageInit(MCDU mcdu)
        {
            _mcdu = mcdu;
            
            mcdu.titleLineText.text = "RADIO NAV";
            
            mcdu.l1Label.text = "VOR1/FREQ";
            mcdu.l1Text.text = "<color=#30FFFF>[  ]/[  . ]</color>";
            mcdu.l2Label.text = "CRS";
            mcdu.l2Text.text = "<color=#30FFFF>[ ]</color>";
            mcdu.l3Label.text = " LS /FREQ";
            mcdu.l3Text.text = "<color=#30FFFF>[  ]/[  . ]</color>";
            mcdu.l4Label.text = "CRS";
            mcdu.l4Text.text = "<color=#30FFFF>[ ]</color>";
            mcdu.l5Label.text = "ADF1/FREQ";
            mcdu.l5Text.text = "<color=#30FFFF>[  ]/[  . ]</color>";

            mcdu.r1Label.text = "VOR2/FREQ";
            mcdu.r1Text.text = "<color=#30FFFF>[  . ]/[  ]</color>";
            mcdu.r2Label.text = "CRS";
            mcdu.r2Text.text = "<color=#30FFFF>[ ]</color>";
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
                if (fmgc.radNav.SetILSByFrequency(frequency))
                {
                    _mcdu.ClearInput();
                    return;
                }
                
                _mcdu.SendMCDUMessage("NOT IN DATABASE");
                return;
            }

            if (MCDUInputValidationUtils.ValidateNavaid(input))
            {
                if (fmgc.radNav.SetILSByName(input))
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
                fmgc.radNav.SetILSCourse(course);
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
                if (fmgc.radNav.SetADFByFrequency(frequency))
                {
                    _mcdu.ClearInput();
                    return;
                }
                
                _mcdu.SendMCDUMessage("NOT IN DATABASE");
                return;
            }

            if (MCDUInputValidationUtils.ValidateNavaid(input))
            {
                if (fmgc.radNav.SetADFByName(input))
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
                if (fmgc.radNav.SetVORByFrequency(index, frequency))
                {
                    _mcdu.ClearInput();
                    return;
                }

                _mcdu.SendMCDUMessage("NOT IN DATABASE");
                return;
            }

            if (MCDUInputValidationUtils.ValidateNavaid(input))
            {
                if (fmgc.radNav.SetVORByName(index, input))
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
                fmgc.radNav.SetVORCourse(index, course);
                _mcdu.ClearInput();
                return;
            }
            
            _mcdu.SendMCDUMessage("FORMAT ERROR");
        }
    }
}