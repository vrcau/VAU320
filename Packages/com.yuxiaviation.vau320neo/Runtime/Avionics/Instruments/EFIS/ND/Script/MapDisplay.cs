using A320VAU.Common;
using A320VAU.Utils;
using JetBrains.Annotations;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using Varneon.VUdon.ArrayExtensions;
using VirtualAviationJapan;

namespace A320VAU.ND.Pages {
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(1000)] // After Virtual-CNS NavaidDatabase
    public class MapDisplay : UdonSharpBehaviour {
        private readonly float UPDATE_INTERVAL = UpdateIntervalUtil.GetUpdateIntervalFromFPS(10);
        private float _lastUpdate;

        [Tooltip("unit: nm")]
        public int defaultRange = 40;

        public EFISVisibilityType defaultVisibilityType = EFISVisibilityType.NONE;

        private DependenciesInjector _injector;
        private ADIRU.ADIRU _adiru; // Temp workaround

        private GameObject[] _markers = { };
        private NavaidDatabase _navaidDatabase;
        private float magneticDeclination;
        private float scale;

        [PublicAPI] public EFISVisibilityType VisibilityType { get; private set; }
        [PublicAPI] public int Range { get; private set; }

        private void Start() {
            _injector = DependenciesInjector.GetInstance(this);

            _navaidDatabase = _injector.navaidDatabase;
            _adiru = _injector.adiru;

            if (_navaidDatabase == null) {
                Debug.LogError("Can't get NavaidDatabase instance, Map unavailable", this);
                gameObject.SetActive(false);
                return;
            }

            magneticDeclination = _navaidDatabase.magneticDeclination;
            InstantiateMarkers(defaultRange, defaultVisibilityType);
        }

        private void Update() {
            if (!UpdateIntervalUtil.CanUpdate(ref _lastUpdate, UPDATE_INTERVAL)) return;

            var entityTransform = _adiru.irs.position;
            var rotation =
                Quaternion.AngleAxis(
                    Vector3.SignedAngle(Vector3.forward, Vector3.ProjectOnPlane(entityTransform, Vector3.up),
                        Vector3.up) + magneticDeclination, Vector3.forward);
            transform.localRotation = rotation;

            var inverseRotation = Quaternion.Inverse(rotation);

            var position = -entityTransform * scale;
            transform.localPosition = rotation * (Vector3.right * position.x + Vector3.up * position.y);

            UpdateMarkerRotations(_markers, inverseRotation);
        }

        private void InstantiateMarkers(int range, EFISVisibilityType efisVisibilityType) {
            Range = range;
            VisibilityType = efisVisibilityType;
            scale = uiRadius / (range * 926.0f);

            foreach (var marker in _markers) Destroy(marker);
            _markers = new GameObject[0];

            for (var index = 0; index < _navaidDatabase.identities.Length; index++) {
                var type = (NavaidCapability)_navaidDatabase.capabilities[index];
                if (type == NavaidCapability.ILS) break;

                var identity = _navaidDatabase.identities[index];
                var navaidTransform = _navaidDatabase.transforms[index];

                switch (type) {
                    case NavaidCapability.NDB:
                        if (efisVisibilityType == EFISVisibilityType.NDB)
                            _markers = _markers.Add(InstantiateMarker(ndbTemplate, identity, navaidTransform));
                        break;
                    case NavaidCapability.VOR:
                        if (efisVisibilityType == EFISVisibilityType.VORDME)
                            _markers = _markers.Add(InstantiateMarker(vorTemplate, identity, navaidTransform));
                        break;
                    case NavaidCapability.VORDME:
                        if (efisVisibilityType == EFISVisibilityType.VORDME)
                            _markers = _markers.Add(InstantiateMarker(vorDmeTemplate, identity, navaidTransform));
                        break;
                    default:
                        if (efisVisibilityType == EFISVisibilityType.VORDME)
                            _markers = _markers.Add(InstantiateMarker(dmeOrTacanTemplate, identity, navaidTransform));
                        break;
                }
            }

            if (efisVisibilityType != EFISVisibilityType.WPT && efisVisibilityType != EFISVisibilityType.APPT) return;
            for (var index = 0; index < _navaidDatabase.waypointIdentities.Length; index++) {
                var identity = _navaidDatabase.waypointIdentities[index];
                var waypointTransform = _navaidDatabase.waypointTransforms[index];
                var type = (WaypointType)_navaidDatabase.waypointTypes[index];

                switch (type) {
                    case WaypointType.Aerodrome:
                        if (efisVisibilityType == EFISVisibilityType.APPT)
                            _markers = _markers.Add(InstantiateMarker(airportTemplate, identity, waypointTransform));
                        break;
                    default:
                        if (efisVisibilityType == EFISVisibilityType.WPT)
                            _markers = _markers.Add(InstantiateMarker(waypointTemplate, identity, waypointTransform));
                        break;
                }
            }
        }

        private GameObject InstantiateMarker(GameObject template, string identity, Transform navaidTransform) {
            var marker = Instantiate(template);
            var markerTransform = marker.transform;
            markerTransform.gameObject.name = $"Marker-{identity}";
            markerTransform.SetParent(transform, false);
            markerTransform.GetComponentInChildren<Text>().text = identity;

            var navaidPosition = navaidTransform.position * scale;
            markerTransform.localPosition = Vector3.right * navaidPosition.x + Vector3.up * navaidPosition.z;

            return marker;
        }

        private static void UpdateMarkerRotations(GameObject[] markers, Quaternion rotation) {
            foreach (var marker in markers) {
                if (marker == null) continue;
                marker.transform.localRotation = rotation;
            }
        }

        [PublicAPI]
        public void SetRange(int range) {
            InstantiateMarkers(range, VisibilityType);
        }

        [PublicAPI]
        public void SetVisibilityType(EFISVisibilityType visibilityType) {
            InstantiateMarkers(Range, visibilityType);
        }

    #region UI Elements

        public int uiRadius = 180;

        // Templates
        public GameObject vorTemplate,
            vorDmeTemplate,
            ndbTemplate,
            dmeOrTacanTemplate,
            waypointTemplate,
            airportTemplate;

    #endregion
    }

    public enum EFISVisibilityType {
        CSTR,
        WPT,
        VORDME,
        NDB,
        APPT,
        NONE
    }
}