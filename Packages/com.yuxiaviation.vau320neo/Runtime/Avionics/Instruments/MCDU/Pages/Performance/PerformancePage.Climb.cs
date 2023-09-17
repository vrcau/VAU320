using A320VAU.Common;

namespace A320VAU.MCDU {
    public partial class PerformancePage {
        public void ClimbUI() {
            _mcdu.titleLineText.text = "CLB";

            _mcdu.l1Label.text = "ACT MODE";
            _mcdu.l1Text.text = $" <color={AirbusAvionicsTheme.Green}>SELECTED</color>";
            _mcdu.l2Label.text = "CI";
            _mcdu.l2Text.text = "50";
            _mcdu.l3Label.text = "MANAGED";
            _mcdu.l3Text.text = "PRESEL";
        }
    }
}