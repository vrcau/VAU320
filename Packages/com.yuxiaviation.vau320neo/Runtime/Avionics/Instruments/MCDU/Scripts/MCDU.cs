using System;
using UdonSharp;
using UnityEngine.UI;

namespace A320VAU.MCDU
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class MCDU : UdonSharpBehaviour
    {
        public MCDUPage CurrentPage { get; private set; }

        public MCDUPage DirPage;
        public MCDUPage ProgPage;
        public MCDUPage PerfPage;
        public MCDUPage InitPage;
        public MCDUPage DataPage;
        public MCDUPage FPlnPage;
        public MCDUPage RadNavPage;
        public MCDUPage FuelPredPage;
        public MCDUPage SecFPlnPage;
        public MCDUPage AtcCommPage;
        
        public MCDUPage McduMenuPage;

        #region UI

        public Text titleLineText;
        public Text scratchpadText;
        
        #region Text
        public Text l1Text;
        public Text l2Text;
        public Text l3Text;
        public Text l4Text;
        public Text l5Text;
        public Text l6Text;
        
        public Text r1Text;
        public Text r2Text;
        public Text r3Text;
        public Text r4Text;
        public Text r5Text;
        public Text r6Text;
        #endregion

        #region Label
        public Text l1Label;
        public Text l2Label;
        public Text l3Label;
        public Text l4Label;
        public Text l5Label;
        public Text l6Label;
        
        public Text r1Label;
        public Text r2Label;
        public Text r3Label;
        public Text r4Label;
        public Text r5Label;
        public Text r6Label;
        #endregion
        #endregion

        #region KeyFunctions
        #region LineSelectKeys
        public void L1() => CurrentPage.L1();
        public void L2() => CurrentPage.L2();
        public void L3() => CurrentPage.L3();
        public void L4() => CurrentPage.L4();
        public void L5() => CurrentPage.L5();
        public void L6() => CurrentPage.L6();

        public void R1() => CurrentPage.R1();
        public void R2() => CurrentPage.R2();
        public void R3() => CurrentPage.R3();
        public void R4() => CurrentPage.R4();
        public void R5() => CurrentPage.R5();
        public void R6() => CurrentPage.R6();
        #endregion
        
        #region PageKeys
        public void DIR() => ToPage(DirPage);
        public void Prog() => ToPage(ProgPage);
        public void Perf() => ToPage(PerfPage);
        public void Init() => ToPage(InitPage);
        public void Data() => ToPage(DataPage);
        public void FPLN() => ToPage(FPlnPage);
        public void SecFPLN() => ToPage(SecFPlnPage);
        public void RadNav() => ToPage(RadNavPage);
        public void FuelPred() => ToPage(FuelPredPage);
        public void AtcComm() => ToPage(AtcCommPage);

        public void McduMenu() => ToPage(McduMenuPage);

        public void Airport() => ToPage(FPlnPage);
        #endregion
        
        #region SlewKeys
        public void Up() => CurrentPage.Up();
        public void Down() => CurrentPage.Down();
        public void PrevPage() => CurrentPage.PrevPage();
        public void NextPage() => CurrentPage.NextPage();
        #endregion

        #region AlphaNumberKeys
        public void Number0() => Input("1");
        public void Number1() => Input("1");
        public void Number2() => Input("2");
        public void Number3() => Input("3");
        public void Number4() => Input("4");
        public void Number5() => Input("5");
        public void Number6() => Input("6");
        public void Number7() => Input("7");
        public void Number8() => Input("8");
        public void Number9() => Input("9");
        public void Point() => Input(".");

        public void PlusOrNeg()
        {
            // TODO
        }
        #endregion

        #region LetterKeys
        public void InputA() => Input("A");
        public void InputB() => Input("B");
        public void InputC() => Input("C");
        public void InputD() => Input("D");
        public void InputE() => Input("E");
        public void InputF() => Input("F");
        public void InputG() => Input("G");
        public void InputH() => Input("H");
        public void InputI() => Input("I");
        public void InputJ() => Input("J");
        public void InputK() => Input("K");
        public void InputL() => Input("L");
        public void InputM() => Input("M");
        public void InputN() => Input("N");
        public void InputO() => Input("O");
        public void InputP() => Input("P");
        public void InputQ() => Input("Q");
        public void InputR() => Input("R");
        public void InputS() => Input("S");
        public void InputT() => Input("T");
        public void InputU() => Input("U");
        public void InputV() => Input("V");
        public void InputW() => Input("W");
        public void InputX() => Input("X");
        public void InputY() => Input("Y");
        public void InputZ() => Input("Z");
        #endregion

        public void ForwardSlash() => Input("/");
        public void Space() => Input(" "); // It's a space

        public void Overfly()
        {
            // TODO
        }

        public void CLR()
        {
            // TODO
        }
        #endregion

        private void Start()
        {
            ToPage(McduMenuPage);
        }

        public void ToPage(UdonSharpBehaviour page)
        {
            if (page == null) return;
            
            ClearDisplay();
            CurrentPage = (MCDUPage)page;
            CurrentPage.OnPageInit(this);
        }

        public void ClearDisplay()
        {
            titleLineText.text = "";
            l1Text.text = "";
            l2Text.text = "";
            l3Text.text = "";
            l4Text.text = "";
            l5Text.text = "";
            l6Text.text = "";
            
            r1Text.text = "";
            r2Text.text = "";
            r3Text.text = "";
            r4Text.text = "";
            r5Text.text = "";
            r6Text.text = "";

            l1Label.text = "";
            l2Label.text = "";
            l3Label.text = "";
            l4Label.text = "";
            l5Label.text = "";
            l6Label.text = "";
            
            r1Label.text = "";
            r2Label.text = "";
            r3Label.text = "";
            r4Label.text = "";
            r5Label.text = "";
            r6Label.text = "";
        }

        public void Input(string content)
        {
            // TODO
        }
    }
}