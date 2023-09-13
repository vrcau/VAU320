using A320VAU.Common;
using A320VAU.Utils;
using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

namespace A320VAU.MCDU {
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class MCDU : UdonSharpBehaviour {
        private SystemEventBus _eventBus;
        private DependenciesInjector _injector;

        private readonly float UPDATE_INTERVAL = UpdateIntervalUtil.GetUpdateIntervalFromFPS(5);
        private float _lastUpdate;

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

        [HideInInspector]
        public string scratchpad;

        private bool _hasMessage;
        private string _mcduMessage = "";

        [PublicAPI] public MCDUPage CurrentPage { get; private set; }

        private void Start() {
            _injector = DependenciesInjector.GetInstance(this);
            _eventBus = _injector.systemEventBus;

            _eventBus.RegisterSaccEvent(this);

            ToPage(McduMenuPage);
        }


        public void SFEXT_O_RespawnButton() {
            ToPage(McduMenuPage);
            ClearInput();
        }

        private void LateUpdate() {
            if (!UpdateIntervalUtil.CanUpdate(ref _lastUpdate, UPDATE_INTERVAL)) return;

            if (CurrentPage != null)
                CurrentPage.OnPageUpdate();
        }

        [PublicAPI]
        public void ToPage(UdonSharpBehaviour page) {
            if (page == null) return;

            ClearDisplay();
            CurrentPage = (MCDUPage)page;
            CurrentPage.OnPageInit(this);
        }

        [PublicAPI]
        public void ClearDisplay() {
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

        [PublicAPI]
        public void SendMCDUMessage(string content) {
            _hasMessage = true;
            _mcduMessage = content;

            scratchpadText.text = content;
        }

        [PublicAPI]
        public void ClearInput() {
            scratchpad = "";
            scratchpadText.text = scratchpad;
        }

        [PublicAPI]
        public void Input(string content) {
            scratchpad += content;
            scratchpadText.text = scratchpad;
        }

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

        public void L1() {
            CurrentPage.L1();
        }

        public void L2() {
            CurrentPage.L2();
        }

        public void L3() {
            CurrentPage.L3();
        }

        public void L4() {
            CurrentPage.L4();
        }

        public void L5() {
            CurrentPage.L5();
        }

        public void L6() {
            CurrentPage.L6();
        }

        public void R1() {
            CurrentPage.R1();
        }

        public void R2() {
            CurrentPage.R2();
        }

        public void R3() {
            CurrentPage.R3();
        }

        public void R4() {
            CurrentPage.R4();
        }

        public void R5() {
            CurrentPage.R5();
        }

        public void R6() {
            CurrentPage.R6();
        }

    #endregion

    #region PageKeys

        [PublicAPI]
        public void DIR() {
            ToPage(DirPage);
        }

        [PublicAPI]
        public void Prog() {
            ToPage(ProgPage);
        }

        [PublicAPI]
        public void Perf() {
            ToPage(PerfPage);
        }

        [PublicAPI]
        public void Init() {
            ToPage(InitPage);
        }

        [PublicAPI]
        public void Data() {
            ToPage(DataPage);
        }

        [PublicAPI]
        public void FPLN() {
            ToPage(FPlnPage);
        }

        [PublicAPI]
        public void SecFPLN() {
            ToPage(SecFPlnPage);
        }

        [PublicAPI]
        public void RadNav() {
            ToPage(RadNavPage);
        }

        [PublicAPI]
        public void FuelPred() {
            ToPage(FuelPredPage);
        }

        [PublicAPI]
        public void AtcComm() {
            ToPage(AtcCommPage);
        }

        [PublicAPI]
        public void McduMenu() {
            ToPage(McduMenuPage);
        }

        [PublicAPI]
        public void Airport() {
            ToPage(FPlnPage);
        }

    #endregion

    #region SlewKeys

        [PublicAPI]
        public void Up() {
            CurrentPage.Up();
        }

        [PublicAPI]
        public void Down() {
            CurrentPage.Down();
        }

        [PublicAPI]
        public void PrevPage() {
            CurrentPage.PrevPage();
        }

        [PublicAPI]
        public void NextPage() {
            CurrentPage.NextPage();
        }

    #endregion

    #region AlphaNumberKeys

        [PublicAPI]
        public void Number0() {
            Input("0");
        }

        [PublicAPI]
        public void Number1() {
            Input("1");
        }

        [PublicAPI]
        public void Number2() {
            Input("2");
        }

        [PublicAPI]
        public void Number3() {
            Input("3");
        }

        [PublicAPI]
        public void Number4() {
            Input("4");
        }

        [PublicAPI]
        public void Number5() {
            Input("5");
        }

        [PublicAPI]
        public void Number6() {
            Input("6");
        }

        [PublicAPI]
        public void Number7() {
            Input("7");
        }

        [PublicAPI]
        public void Number8() {
            Input("8");
        }

        [PublicAPI]
        public void Number9() {
            Input("9");
        }

        [PublicAPI]
        public void Point() {
            Input(".");
        }

        [PublicAPI]
        public void PlusOrNeg() {
            if (scratchpad.Length != 0)
                switch (scratchpad[scratchpad.Length - 1]) {
                    case '+':
                        scratchpad = scratchpad.Remove(scratchpad.Length - 1) + "-";
                        break;
                    case '-':
                        scratchpad = scratchpad.Remove(scratchpad.Length - 1) + "+";
                        break;
                    default:
                        scratchpad += "+";
                        break;
                }
            else
                scratchpad += "+";

            scratchpadText.text = scratchpad;
        }

    #endregion

    #region LetterKeys

        [PublicAPI]
        public void InputA() {
            Input("A");
        }

        [PublicAPI]
        public void InputB() {
            Input("B");
        }

        [PublicAPI]
        public void InputC() {
            Input("C");
        }

        [PublicAPI]
        public void InputD() {
            Input("D");
        }

        [PublicAPI]
        public void InputE() {
            Input("E");
        }

        [PublicAPI]
        public void InputF() {
            Input("F");
        }

        [PublicAPI]
        public void InputG() {
            Input("G");
        }

        [PublicAPI]
        public void InputH() {
            Input("H");
        }

        [PublicAPI]
        public void InputI() {
            Input("I");
        }

        [PublicAPI]
        public void InputJ() {
            Input("J");
        }

        [PublicAPI]
        public void InputK() {
            Input("K");
        }

        [PublicAPI]
        public void InputL() {
            Input("L");
        }

        [PublicAPI]
        public void InputM() {
            Input("M");
        }

        [PublicAPI]
        public void InputN() {
            Input("N");
        }

        [PublicAPI]
        public void InputO() {
            Input("O");
        }

        [PublicAPI]
        public void InputP() {
            Input("P");
        }

        [PublicAPI]
        public void InputQ() {
            Input("Q");
        }

        [PublicAPI]
        public void InputR() {
            Input("R");
        }

        [PublicAPI]
        public void InputS() {
            Input("S");
        }

        [PublicAPI]
        public void InputT() {
            Input("T");
        }

        [PublicAPI]
        public void InputU() {
            Input("U");
        }

        [PublicAPI]
        public void InputV() {
            Input("V");
        }

        [PublicAPI]
        public void InputW() {
            Input("W");
        }

        [PublicAPI]
        public void InputX() {
            Input("X");
        }

        [PublicAPI]
        public void InputY() {
            Input("Y");
        }

        [PublicAPI]
        public void InputZ() {
            Input("Z");
        }

    #endregion

        [PublicAPI]
        public void ForwardSlash() {
            Input("/");
        }

        [PublicAPI]
        public void Space() {
            Input(" ");
            // It's a space
        }

        [PublicAPI]
        public void Overfly() {
            // TODO
        }

        [PublicAPI]
        public void CLR() {
            if (_hasMessage) {
                scratchpadText.text = scratchpad;
                _hasMessage = false;
                return;
            }

            switch (scratchpad) {
                case "":
                    scratchpad = "CLR";
                    break;
                case "CLR":
                    scratchpad = "";
                    break;
                default:
                    scratchpad = scratchpad.Remove(scratchpad.Length - 1);
                    break;
            }

            scratchpadText.text = scratchpad;
        }

    #endregion
    }
}