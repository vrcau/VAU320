using A320VAU.Common;

namespace A320VAU.MCDU {
    public partial class PerformancePage {
        private void TakeoffUI() {
            _mcdu.titleLineText.text = "TAKEOFF";

            _mcdu.l1Label.text = "V1   FLP PETR";
            _mcdu.l1Text.text = $"<color={AirbusAvionicsTheme.Blue}>{_fmgc.performance.v1.ToString()}</color>     F=<color={AirbusAvionicsTheme.Green}>147</color>";
            _mcdu.l2Label.text = "VR   SLT PETR";
            _mcdu.l2Text.text = $"<color={AirbusAvionicsTheme.Blue}>{_fmgc.performance.vr.ToString()}</color>     S=<color={AirbusAvionicsTheme.Green}>191</color>";
            _mcdu.l3Label.text = "V2      CLEAN";
            _mcdu.l3Text.text = $"<color={AirbusAvionicsTheme.Blue}>{_fmgc.performance.v2.ToString()}</color>     O=<color={AirbusAvionicsTheme.Green}>212</color>";
            _mcdu.l4Label.text = "TRANS ALT";
            _mcdu.l4Text.text = $"<color={AirbusAvionicsTheme.Blue}>6000</color>";
            _mcdu.l5Label.text = "THR RED/ACC";
            _mcdu.l5Text.text = $"<color={AirbusAvionicsTheme.Blue}>{_fmgc.performance.reduceThrustAltitude.ToString()}/{_fmgc.performance.accelerateAltitude.ToString()}</color>";

            // _mcdu.r2Label.text = "TO SHIFT";
            // _mcdu.r2Text.text = $"<color={AirbusAvionicsTheme.Blue}>[M][  ]*</color>";
            _mcdu.r3Label.text = "FLAPS/THS";
            _mcdu.r3Text.text =
                $"<color={AirbusAvionicsTheme.Blue}>{_fmgc.performance.takeoffFlapSetting}/[   ]</color>";
            _mcdu.r4Label.text = "FLEX TO TEMP";
            _mcdu.r4Text.text = $"<color={AirbusAvionicsTheme.Blue}>[   ]</color>";
            _mcdu.r5Label.text = "ENG OUT ACC";
            _mcdu.r5Text.text =
                $"<color={AirbusAvionicsTheme.Blue}>{_fmgc.performance.engineOutAccelerateAltitude}</color>";

            _mcdu.l6Label.text = "NEXT";
            _mcdu.l6Text.text = "PHASE>";
        }
    }
}