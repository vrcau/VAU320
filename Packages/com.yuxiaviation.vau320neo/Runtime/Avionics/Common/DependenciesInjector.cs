using System.Linq;
using System.Reflection;
using A320VAU.Avionics;
using A320VAU.Brake;
using A320VAU.DFUNC;
using A320VAU.SFEXT;
using EsnyaSFAddons.DFUNC;
using EsnyaSFAddons.SFEXT;
using SaccFlightAndVehicles;
using UdonSharp;
using UnityEditor;
using UnityEngine;
using VirtualAviationJapan;
using VRC.Udon.Editor;
using YuxiFlightInstruments.BasicFlightData;
using YuxiFlightInstruments.Navigation;

namespace A320VAU.Common
{
    public class DependenciesInjector : MonoBehaviour
    {
        [Header("Aircraft Systems")] public SaccAirVehicle saccAirVehicle;
        public SaccEntity saccEntity;

        public YFI_FlightDataInterface flightData;

        public SFEXT_a320_AdvancedEngine engine1;
        public SFEXT_a320_AdvancedEngine engine2;

        public SFEXT_AuxiliaryPowerUnit apu;

        public DFUNC_AdvancedFlaps flaps;

        public SFEXT_a320_AdvancedGear leftLadingGear;
        public SFEXT_a320_AdvancedGear rightLadingGear;
        public SFEXT_a320_AdvancedGear frontLadingGear;
        public DFUNC_a320_Brake brake;

        public DFUNC_a320_LandingLight landingLight;
        public DFUNC_Canopy canopy;

        public YFI_NavigationReceiver navigationReceiver1;
        public YFI_NavigationReceiver navigationReceiver2;
        public GPWS_OWML gpws;

        public AirbusAvionicsTheme airbusAvionicsTheme;

        [Header("World")]
        public NavaidDatabase navaidDatabase;

        [Header("Navigation Receivers Dependencies Search Settings")]
        public string navigationReceiver1Name = "NaviReciver1";

        public string navigationReceiver2Name = "NaviReciver2";

        [Header("Engines Dependencies Search Settings")]
        public string engine1Name = "AdvancedEngineL";

        public string engine2Name = "AdvancedEngineR";

        [Header("Engines Dependencies Search Settings")]
        public string leftLadingGearName = "AdvancedGear_L";

        public string rightLadingGearName = "AdvancedGear_R";
        public string frontLadingGearName = "AdvancedGear_C";
    }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
    [CustomEditor(typeof(DependenciesInjector))]
    public class DependenciesInjectorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var injector = target as DependenciesInjector;
            if (injector == null) return;

            base.OnInspectorGUI();
            if (GUILayout.Button("Setup"))
            {
                Setup(injector);
            }

            if (GUILayout.Button("Setup World"))
            {
                SetupWorld(injector);
            }

            if (GUILayout.Button("Inject"))
            {
                var behaviours = injector.GetComponentsInChildren<UdonSharpBehaviour>(true);
                var fields = typeof(DependenciesInjector).GetFields()
                    .Where(field => field.DeclaringType == typeof(DependenciesInjector) & !field.Name.StartsWith("_"))
                    .ToArray();

                foreach (var behaviour in behaviours)
                {
                    if (behaviour.GetProgramVariable("EntityControl") == null)
                        behaviour.SetProgramVariable("EntityControl", injector.saccEntity);
                    
                    var behaviourFields = behaviour.GetType().GetFields();

                    foreach (var field in fields)
                    {
                        var behaviourField = behaviourFields.FirstOrDefault(f => f.Name == field.Name);
                        
                        if (behaviourField != null &&
                            behaviourField.Name == field.Name & behaviourField.FieldType == field.FieldType &&
                            behaviourField.GetValue(behaviour) == null)
                            behaviour.SetProgramVariable(field.Name, field.GetValue(injector));  
                    }
                }
            }
        }

        private static void SetupWorld(DependenciesInjector injector)
        {
            injector.navaidDatabase = GameObject.Find(nameof(NavaidDatabase)).GetComponent<NavaidDatabase>();
        }

        private static void Setup(DependenciesInjector injector)
        {
            injector.saccEntity = injector.GetComponentInChildren<SaccEntity>(true);
            injector.saccAirVehicle = injector.GetComponentInChildren<SaccAirVehicle>(true);

            injector.flightData = injector.GetComponentInChildren<YFI_FlightDataInterface>(true);
            injector.apu = injector.GetComponentInChildren<SFEXT_AuxiliaryPowerUnit>(true);
            injector.flaps = injector.GetComponentInChildren<DFUNC_AdvancedFlaps>(true);

            injector.brake = injector.GetComponentInChildren<DFUNC_a320_Brake>(true);
            injector.landingLight = injector.GetComponentInChildren<DFUNC_a320_LandingLight>(true);
            injector.canopy = injector.GetComponentInChildren<DFUNC_Canopy>(true);

            injector.gpws = injector.GetComponentInChildren<GPWS_OWML>(true);

            injector.airbusAvionicsTheme = injector.GetComponentInChildren<AirbusAvionicsTheme>();

            // Engines
            var engines = injector.GetComponentsInChildren<SFEXT_a320_AdvancedEngine>(true);
            foreach (var engine in engines)
            {
                if (engine.gameObject.name == injector.engine1Name)
                {
                    injector.engine1 = engine;
                }
                else if (engine.gameObject.name == injector.engine2Name)
                {
                    injector.engine2 = engine;
                }
            }

            // NavigationReceiver
            var receivers = injector.GetComponentsInChildren<YFI_NavigationReceiver>(true);
            foreach (var receiver in receivers)
            {
                if (receiver.gameObject.name == injector.navigationReceiver1Name)
                {
                    injector.navigationReceiver1 = receiver;
                }
                else if (receiver.gameObject.name == injector.navigationReceiver2Name)
                {
                    injector.navigationReceiver2 = receiver;
                }
            }

            // Gears
            var gears = injector.GetComponentsInChildren<SFEXT_a320_AdvancedGear>(true);
            foreach (var gear in gears)
            {
                if (gear.gameObject.name == injector.leftLadingGearName)
                {
                    injector.leftLadingGear = gear;
                }
                else if (gear.gameObject.name == injector.rightLadingGearName)
                {
                    injector.rightLadingGear = gear;
                }
                else if (gear.gameObject.name == injector.frontLadingGearName)
                {
                    injector.frontLadingGear = gear;
                }
            }
        }
    }
#endif
}