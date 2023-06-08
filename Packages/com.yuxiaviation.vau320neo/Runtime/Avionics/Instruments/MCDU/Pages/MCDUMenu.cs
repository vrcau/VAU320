using UdonSharp;

namespace A320VAU.MCDU
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class MCDUMenu : MCDUPage
    {
        private MCDU _mcdu;

        public AircraftStatusPage aircraftStatusPage;
        
        public override void OnPageInit(MCDU mcdu)
        {
            _mcdu = mcdu;
            
            mcdu.titleLineText.text = "MCDU MENU";
            mcdu.l1Text.text = "<color=#3FFF43><FMGC (REQ)</color>";
            mcdu.l2Text.text = "<ATSU";
            mcdu.l3Text.text = "<AIDS";
            mcdu.l4Text.text = "<CFDS";

            mcdu.r1Label.text = "SELECT";
            mcdu.r1Text.text = "NAV B/UP>";
        }

        public override void L1()
        {
            _mcdu.ToPage(aircraftStatusPage);
        }

        public override void L2()
        {
            _mcdu.SendMCDUMessage("INOP");
        }

        public override void L3()
        {
            _mcdu.SendMCDUMessage("INOP");
        }

        public override void L4()
        {
            _mcdu.SendMCDUMessage("INOP");
        }

        public override void R1()
        {
            _mcdu.SendMCDUMessage("INOP");
        }
    }
}