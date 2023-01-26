using UdonSharp;
using UnityEngine.UI;
using URC;

namespace A320VAU
{
    public class RMP : UdonSharpBehaviour
    {
        public Transceiver VHF1;

        public Text ActiveFrequencyText;

        private void LateUpdate() {
            ActiveFrequencyText.text = VHF1.Frequency.ToString("000.000");
        }
    }
}
