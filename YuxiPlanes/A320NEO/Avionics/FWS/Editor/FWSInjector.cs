using UnityEngine;
using UnityEditor;

namespace A320VAU.FWS.Editor
{
    public class FWSInjector : EditorWindow
    {

        [MenuItem("VAU320neo/FWSInjector")]
        private static void ShowWindow()
        {
            var window = GetWindow<FWSInjector>();
            window.titleContent = new GUIContent("FWSInjector");
            window.Show();
        }

        private void OnGUI()
        {

        }
    }
}
