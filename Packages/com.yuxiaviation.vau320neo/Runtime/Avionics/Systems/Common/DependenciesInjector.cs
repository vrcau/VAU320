using System;
using A320VAU.Avionics;
using A320VAU.Brake;
using A320VAU.DFUNC;
using A320VAU.Common;
using A320VAU.SFEXT;
using Avionics.Systems.Common;
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

        public AircraftSystemData equipmentData;

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

        private void Awake() => Setup();

        internal void Setup() {
            saccEntity = GetComponentInChildren<SaccEntity>(true);
            saccAirVehicle = GetComponentInChildren<SaccAirVehicle>(true);

            flightData = GetComponentInChildren<YFI_FlightDataInterface>(true);
            apu = GetComponentInChildren<SFEXT_AuxiliaryPowerUnit>(true);
            flaps = GetComponentInChildren<DFUNC_AdvancedFlaps>(true);

            brake = GetComponentInChildren<DFUNC_a320_Brake>(true);
            landingLight = GetComponentInChildren<DFUNC_a320_LandingLight>(true);
            canopy = GetComponentInChildren<DFUNC_Canopy>(true);

            gpws = GetComponentInChildren<GPWS_OWML>(true);
            radioAltimeter = GetComponentInChildren<RadioAltimeter.RadioAltimeter>(true);

            equipmentData = GetComponentInChildren<AircraftSystemData>(true);

            fmgc = GetComponentInChildren<FMGC.FMGC>(true);

            airbusAvionicsTheme = GetComponentInChildren<AirbusAvionicsTheme>(true);

            // Worlds
            navaidDatabase = GetNavaidDatabase();

            // Engines
            var engines = GetComponentsInChildren<SFEXT_a320_AdvancedEngine>(true);
            foreach (var engine in engines)
                if (engine.gameObject.name == engine1Name)
                    engine1 = engine;
                else if (engine.gameObject.name == engine2Name) engine2 = engine;

            // Gears
            gear = GetComponentInChildren<DFUNC_Gear>(true);
            var gears = GetComponentsInChildren<SFEXT_a320_AdvancedGear>(true);
            foreach (var gear in gears)
                if (gear.gameObject.name == leftLadingGearName)
                    leftLadingGear = gear;
                else if (gear.gameObject.name == rightLadingGearName)
                    rightLadingGear = gear;
                else if (gear.gameObject.name == frontLadingGearName) frontLadingGear = gear;
            
        #if !COMPILER_UDONSHARP && UNITY_EDITOR
            EditorUtility.SetDirty(this);
        #endif
        }
        
        private static NavaidDatabase GetNavaidDatabase() {
            var navaidDatabaseObject = GameObject.Find(nameof(NavaidDatabase));
            if (navaidDatabaseObject == null) {
                Debug.LogError("Can't find NavaidDatabase GameObject: NavaidDatabase");
                return null;
            }

            var navaidDatabase = navaidDatabaseObject.GetComponent<NavaidDatabase>();
            if (navaidDatabase == null)
                Debug.LogError($"Can't find NavaidDatabase Component on GameObject: {navaidDatabaseObject.name}",
                    navaidDatabaseObject);

            return navaidDatabase;
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
            if (GUILayout.Button("Setup")) injector.Setup();
        }
    }
#endif
}