using A320VAU.Avionics;
using A320VAU.Brake;
using A320VAU.DFUNC;
using A320VAU.ECAM;
using A320VAU.SFEXT;
using EsnyaSFAddons.DFUNC;
using EsnyaSFAddons.SFEXT;
using SaccFlightAndVehicles;
using UdonSharp;
using UnityEditor;
using UnityEngine;
using VirtualAviationJapan;
using YuxiFlightInstruments.BasicFlightData;
#if !COMPILER_UDONSHARP && UNITY_EDITOR
using UdonSharpEditor;
#endif

namespace A320VAU.Common {
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class DependenciesInjector : UdonSharpBehaviour {
        [Header("Aircraft Systems")]
        public SaccAirVehicle saccAirVehicle;

        public SaccEntity saccEntity;

        public YFI_FlightDataInterface flightData;

        public SFEXT_a320_AdvancedEngine engine1;
        public SFEXT_a320_AdvancedEngine engine2;

        public SFEXT_AuxiliaryPowerUnit apu;

        public DFUNC_AdvancedFlaps flaps;

        public DFUNC_Gear gear;
        public SFEXT_a320_AdvancedGear leftLadingGear;
        public SFEXT_a320_AdvancedGear rightLadingGear;
        public SFEXT_a320_AdvancedGear frontLadingGear;
        public DFUNC_a320_Brake brake;

        public DFUNC_a320_LandingLight landingLight;
        public DFUNC_Canopy canopy;

        public GPWS_OWML gpws;
        public RadioAltimeter.RadioAltimeter radioAltimeter;

        public ECAMDataInterface equipmentData;

        public FMGC.FMGC fmgc;

        public AirbusAvionicsTheme airbusAvionicsTheme;

        [Header("World")]
        public NavaidDatabase navaidDatabase;

        [Header("Engines Dependencies Search Settings")]
        public string engine1Name = "AdvancedEngineL";

        public string engine2Name = "AdvancedEngineR";

        [Header("Engines Dependencies Search Settings")]
        public string leftLadingGearName = "AdvancedGear_L";

        public string rightLadingGearName = "AdvancedGear_R";
        public string frontLadingGearName = "AdvancedGear_C";

        private void Start() {
            navaidDatabase = GameObject.Find(nameof(NavaidDatabase)).GetComponent<NavaidDatabase>();
        }

        public static DependenciesInjector GetInstance(UdonSharpBehaviour behaviour) {
            return behaviour.GetComponentInParent<DependenciesInjector>();
        }
    }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
    [CustomEditor(typeof(DependenciesInjector))]
    public class DependenciesInjectorEditor : Editor {
        public override void OnInspectorGUI() {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;

            var injector = target as DependenciesInjector;
            if (injector == null) return;

            base.OnInspectorGUI();
            if (GUILayout.Button("Setup")) Setup(injector);
        }

        private static void Setup(DependenciesInjector injector) {
            injector.saccEntity = injector.GetComponentInChildren<SaccEntity>(true);
            injector.saccAirVehicle = injector.GetComponentInChildren<SaccAirVehicle>(true);

            injector.flightData = injector.GetComponentInChildren<YFI_FlightDataInterface>(true);
            injector.apu = injector.GetComponentInChildren<SFEXT_AuxiliaryPowerUnit>(true);
            injector.flaps = injector.GetComponentInChildren<DFUNC_AdvancedFlaps>(true);

            injector.brake = injector.GetComponentInChildren<DFUNC_a320_Brake>(true);
            injector.landingLight = injector.GetComponentInChildren<DFUNC_a320_LandingLight>(true);
            injector.canopy = injector.GetComponentInChildren<DFUNC_Canopy>(true);

            injector.gpws = injector.GetComponentInChildren<GPWS_OWML>(true);
            injector.radioAltimeter = injector.GetComponentInChildren<RadioAltimeter.RadioAltimeter>(true);

            injector.equipmentData = injector.GetComponentInChildren<ECAMDataInterface>(true);

            injector.fmgc = injector.GetComponentInChildren<FMGC.FMGC>(true);

            injector.airbusAvionicsTheme = injector.GetComponentInChildren<AirbusAvionicsTheme>(true);

            // Engines
            var engines = injector.GetComponentsInChildren<SFEXT_a320_AdvancedEngine>(true);
            foreach (var engine in engines)
                if (engine.gameObject.name == injector.engine1Name)
                    injector.engine1 = engine;
                else if (engine.gameObject.name == injector.engine2Name) injector.engine2 = engine;

            // Gears
            injector.gear = injector.GetComponentInChildren<DFUNC_Gear>(true);
            var gears = injector.GetComponentsInChildren<SFEXT_a320_AdvancedGear>(true);
            foreach (var gear in gears)
                if (gear.gameObject.name == injector.leftLadingGearName)
                    injector.leftLadingGear = gear;
                else if (gear.gameObject.name == injector.rightLadingGearName)
                    injector.rightLadingGear = gear;
                else if (gear.gameObject.name == injector.frontLadingGearName) injector.frontLadingGear = gear;
        }
    }
#endif
}