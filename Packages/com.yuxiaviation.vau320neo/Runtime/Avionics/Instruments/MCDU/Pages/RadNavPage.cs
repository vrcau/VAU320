using UdonSharp;

namespace A320VAU.MCDU
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class RadNavPage : MCDUPage
    {
        public override void OnPageInit(MCDU mcdu)
        {
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
            mcdu.r5Label.text = "ADF2/FREQ";
            mcdu.r5Text.text = "<color=#30FFFF>[  . ]/[  ]</color>";
        }
    }
}