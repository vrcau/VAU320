using System.Globalization;
using UdonSharp;
using UnityEngine;

namespace A320VAU.Common {
    public class AirbusAvionicsTheme : UdonSharpBehaviour {
        public const string Danger = "#FC0019";
        public const string Amber = "#FD7D22";
        public const string Green = "#3FFF43";
        public const string Blue = "#30FFFF";
        public const string Carmine = "#FF00FF";

        public Color DangerColor { get; private set; }
        public Color AmberColor { get; private set; }
        public Color GreenColor { get; private set; }
        public Color BlueColor { get; private set; }
        public Color CarmineColor { get; private set; }

        private void Start() {
            DangerColor = GetColorByHtmlString(Danger);
            AmberColor = GetColorByHtmlString(Amber);
            GreenColor = GetColorByHtmlString(Green);
            BlueColor = GetColorByHtmlString(Blue);
            CarmineColor = GetColorByHtmlString(Carmine);
        }

        public Color GetColorByHtmlString(string hex) {
            hex = hex.Replace("0x", "");
            hex = hex.Replace("#", "");

            byte a = 255;
            var r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
            var g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
            var b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);

            if (hex.Length == 8) a = byte.Parse(hex.Substring(6, 2), NumberStyles.HexNumber);

            return new Color32(r, g, b, a);
        }
    }
}