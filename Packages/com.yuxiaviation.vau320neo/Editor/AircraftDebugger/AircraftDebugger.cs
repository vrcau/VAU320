using System;
using System.Linq;
using EsnyaSFAddons.DFUNC;
using SaccFlightAndVehicles;
using UdonSharpEditor;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEngine;

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
            
            rootVisualElement.Add(ui);

            // Event
            // Flaps
            rootVisualElement.Query<Button>("flaps-0").First().clicked += () => SetFlaps(0);
            rootVisualElement.Query<Button>("flaps-1").First().clicked += () => SetFlaps(1);
            rootVisualElement.Query<Button>("flaps-2").First().clicked += () => SetFlaps(2);
            rootVisualElement.Query<Button>("flaps-3").First().clicked += () => SetFlaps(3);
            rootVisualElement.Query<Button>("flaps-full").First().clicked += () => SetFlaps(4);
            
            // Config
            rootVisualElement.Query<Button>("cold-and-dark").First().clicked +=
                () => SetConfig(AircraftConfigType.ColdAndDark);
            rootVisualElement.Query<Button>("adiru-apu-on").First().clicked +=
                () => SetConfig(AircraftConfigType.AdiruApuOn);
            rootVisualElement.Query<Button>("engine-started").First().clicked +=
                () => SetConfig(AircraftConfigType.EngineStarted);
            
            // SaccVehicle
            rootVisualElement.Query<Button>("pilot").First().clicked += Pilot;
            rootVisualElement.Query<Button>("explode").First().clicked += Explode;
            rootVisualElement.Query<Button>("respawn").First().clicked += Respawn;
        }

        private void OnGUI()
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
                UdonSharpEditorUtility.GetBackingUdonBehaviour(airVehicle).SendCustomEvent(nameof(SaccAirVehicle.SFEXT_O_RespawnButton));
        }

        private void Explode()
        {
            var airVehicle = _saccEntity.GetComponentInChildren<SaccAirVehicle>();
            if (airVehicle != null)
                UdonSharpEditorUtility.GetBackingUdonBehaviour(airVehicle).SendCustomEvent(nameof(SaccAirVehicle.Explode));
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
            var flaps = _saccEntity.GetComponentInChildren<DFUNC_AdvancedFlaps>();
            if (flaps == null) return;
            var behaviour = UdonSharpEditorUtility.GetBackingUdonBehaviour(flaps);
            var detent = behaviour.GetProgramVariable<float[]>(nameof(DFUNC_AdvancedFlaps.detents)).ElementAt(index);
            behaviour.SetProgramVariable(nameof(DFUNC_AdvancedFlaps.angle), detent);
            behaviour.SetProgramVariable(nameof(DFUNC_AdvancedFlaps.targetAngle), detent);
        }

        private void SetConfig(AircraftConfigType type)
        {
            Debug.Log(type);
        }
    }

    public enum AircraftConfigType
    {
        ColdAndDark,
        AdiruApuOn,
        EngineStarted
    }
}
