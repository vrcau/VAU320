using System;
using System.Linq;
using A320VAU.PFD;
using A320VAU.SFEXT;
using EsnyaSFAddons.DFUNC;
using SaccFlightAndVehicles;
using UdonSharpEditor;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEngine;
using YuxiFlightInstruments.ElectricalBus;

namespace A320VAU.Editor.AircraftDebugger
{
    public class AircraftDebugger : EditorWindow
    {
        private SaccEntity _saccEntity;
        
        [MenuItem("v320neo/Debugger")]
        private static void ShowWindow()
        {
            var window = GetWindow<AircraftDebugger>();
            window.Show();
        }

        private void CreateGUI()
        {
            titleContent = new GUIContent("v320neo Debugger");
            
            var uiAsset =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    "Packages/com.yuxiaviation.vau320neo/Editor/AircraftDebugger/AircraftDebugger.uxml");
            var ui = uiAsset.CloneTree();

            minSize = new Vector2(610, 600);
            
            rootVisualElement.Add(ui);

            // Event
            // Flaps
            rootVisualElement.Query<Button>("flaps-0").First().clicked += () => SetFlaps(0);
            rootVisualElement.Query<Button>("flaps-1").First().clicked += () => SetFlaps(1);
            rootVisualElement.Query<Button>("flaps-2").First().clicked += () => SetFlaps(2);
            rootVisualElement.Query<Button>("flaps-3").First().clicked += () => SetFlaps(3);
            rootVisualElement.Query<Button>("flaps-full").First().clicked += () => SetFlaps(4);
            
            // Gears
            rootVisualElement.Query<Button>("gear-up").First().clicked += () => SetGear(false);
            rootVisualElement.Query<Button>("gear-down").First().clicked += () => SetGear(true);
            
            // ELEC
            rootVisualElement.Query<Button>("battery-on").First().clicked += () => SetBattery(true);
            rootVisualElement.Query<Button>("battery-off").First().clicked += () => SetBattery(false);
            
            // Config
            rootVisualElement.Query<Button>("cold-and-dark").First().clicked +=
                () => SetConfig(AircraftConfigType.ColdAndDark);
            rootVisualElement.Query<Button>("adiru-apu-on").First().clicked +=
                () => SetConfig(AircraftConfigType.AdiruApuOn);
            rootVisualElement.Query<Button>("engine-started").First().clicked +=
                () => SetConfig(AircraftConfigType.EngineStarted);
            rootVisualElement.Query<Button>("takeoff").First().clicked +=
                () => SetConfig(AircraftConfigType.Takeoff);
            rootVisualElement.Query<Button>("landing").First().clicked +=
                () => SetConfig(AircraftConfigType.Landing);
            rootVisualElement.Query<Button>("cruise").First().clicked +=
                () => SetConfig(AircraftConfigType.Cruise);
            
            // SaccVehicle
            rootVisualElement.Query<Button>("pilot").First().clicked += Pilot;
            rootVisualElement.Query<Button>("explode").First().clicked += Explode;
            rootVisualElement.Query<Button>("respawn").First().clicked += Respawn;
            
            UpdateDebugger();
        }

        private void SetBattery(bool isBatteryOn)
        {
            var electricalBus = _saccEntity.GetComponentInChildren<YFI_ElectricalBus>();
            if (electricalBus == null) return;

            if (isBatteryOn != electricalBus.batteryOn)
                electricalBus.OnToggleBattery();
        }

        private void OnFocus() => UpdateDebugger();

        private void UpdateDebugger()
        {
            if (Selection.activeGameObject == null)
            {
                rootVisualElement.AddToClassList("disable");
                return;
            }
            
            _saccEntity = (Selection.activeGameObject.GetComponentInParent<Rigidbody>() ??
                               Selection.activeGameObject.GetComponentInChildren<Rigidbody>())?.GetComponent<SaccEntity>();

            if (_saccEntity == null)
            {
                rootVisualElement.AddToClassList("disable");
                return;
            }
            
            rootVisualElement.RemoveFromClassList("disable");
            
            if (Application.isPlaying)
                rootVisualElement.RemoveFromClassList("disable-playmode");
            else
                rootVisualElement.AddToClassList("disable-playmode");
        }

        private void Respawn()
        {
            var airVehicle = _saccEntity.GetComponentInChildren<SaccAirVehicle>();
            if (airVehicle != null)
                airVehicle.SFEXT_O_RespawnButton();
        }

        private void Explode()
        {
            var airVehicle = _saccEntity.GetComponentInChildren<SaccAirVehicle>();
            if (airVehicle != null)
                airVehicle.Explode();
        }

        private void Pilot()
        {
            if (_saccEntity == null) return;
            var seats = _saccEntity.GetComponentsInChildren<SaccVehicleSeat>();
            var seat = seats.FirstOrDefault(s => s.name == "Seat_CAPT");
            if (seat != null)
                UdonSharpEditorUtility.GetBackingUdonBehaviour(seat).SendCustomEvent("_interact");
        }

        private void SetFlaps(int index)
        {
            var flaps = _saccEntity.GetComponentInChildren<DFUNC_AdvancedFlaps>(true);
            if (flaps == null) return;
            var behaviour = UdonSharpEditorUtility.GetBackingUdonBehaviour(flaps);
            var detent = behaviour.GetProgramVariable<float[]>(nameof(DFUNC_AdvancedFlaps.detents)).ElementAt(index);
            behaviour.SetProgramVariable(nameof(DFUNC_AdvancedFlaps.angle), detent);
            behaviour.SetProgramVariable(nameof(DFUNC_AdvancedFlaps.targetAngle), detent);
        }

        private void SetGear(bool isDown)
        {
            var gears = _saccEntity.GetComponentsInChildren<SFEXT_a320_AdvancedGear>(true);
            foreach (var gear in gears)
            {
                var udonBehaviour = UdonSharpEditorUtility.GetBackingUdonBehaviour(gear);
                udonBehaviour.SetProgramVariable(nameof(SFEXT_a320_AdvancedGear.position),
                    isDown ? 1f : 0f);
                udonBehaviour.SetProgramVariable(nameof(SFEXT_a320_AdvancedGear.targetPosition),
                    isDown ? 1f : 0f);
            }
        }

        private void SetConfig(AircraftConfigType type)
        {
            switch (type)
            {
                case AircraftConfigType.ColdAndDark:
                    ReinitDUs();
                    SetBattery(false);
                    SetFlaps(0);
                    SetGear(true);
                    SetEngineStarted(false);
                    break;
                case AircraftConfigType.AdiruApuOn:
                    SetBattery(true);
                    BypassDUsSelfTest();
                    SetFlaps(0);
                    SetGear(true);
                    SetEngineStarted(false);
                    break;
                case AircraftConfigType.EngineStarted:
                    SetBattery(true);
                    BypassDUsSelfTest();
                    SetFlaps(0);
                    SetGear(true);
                    SetEngineStarted();
                    break;
                case AircraftConfigType.Takeoff:
                    SetBattery(true);
                    BypassDUsSelfTest();
                    SetFlaps(2);
                    SetGear(true);
                    SetEngineStarted();
                    break;
                case AircraftConfigType.Landing:
                    SetBattery(true);
                    BypassDUsSelfTest();
                    SetFlaps(4);
                    SetGear(true);
                    SetEngineStarted();
                    break;
                case AircraftConfigType.Cruise:
                    SetBattery(true);
                    BypassDUsSelfTest();
                    SetFlaps(0);
                    SetGear(false);
                    SetEngineStarted();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private void ReinitDUs()
        {
            foreach (var du in _saccEntity.GetComponentsInChildren<DU>(true))
                du.InitDU();
        }

        private void BypassDUsSelfTest()
        {
            foreach (var du in _saccEntity.GetComponentsInChildren<DU>(true))
            {
                du.BypassSelftest();
            }
        }

        private void SetEngineStarted(bool started = true)
        {
            if (!started) return;
            foreach (var engine in _saccEntity.GetComponentsInChildren<SFEXT_a320_AdvancedEngine>(true))
            {
                engine._InstantStart();
            }
        }
    }

    public enum AircraftConfigType
    {
        ColdAndDark,
        AdiruApuOn,
        EngineStarted,
        Takeoff,
        Landing,
        Cruise
    }
}
