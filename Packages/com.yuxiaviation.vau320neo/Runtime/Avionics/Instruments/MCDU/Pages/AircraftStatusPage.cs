using UdonSharp;

namespace A320VAU.MCDU {
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class AircraftStatusPage : MCDUPage {
        private MCDU _mcdu;

        public override void OnPageInit(MCDU mcdu) {
            _mcdu = mcdu;

            mcdu.titleLineText.text = "V320-251N";

            mcdu.l1Label.text = "ENG";
            mcdu.l1Text.text = "<color=#3FFF43>LEAP-1A26</color>";
            mcdu.l2Label.text = "ACTIVE NAV DATA BASE";
            mcdu.l2Text.text = " <color=#30FFFF>V-CNS NAVAID DATABASE</color>";
            mcdu.l3Label.text = "SECOND NAV DATABASE";
            mcdu.l3Text.text = " INOP";

            mcdu.l5Label.text = "GHC CODE";
            mcdu.l5Text.text = "<color=#30FFFF>[  ]</color>";
            mcdu.l6Label.text = "IDLE/PERF";
            mcdu.l6Text.text = "<color=#3FFF43>+0.0/+0.0</color>";

            mcdu.r6Label.text = "SOFTWARE";
            mcdu.r6Text.text = "STATUS/XLOAD>";
        }
    }
}