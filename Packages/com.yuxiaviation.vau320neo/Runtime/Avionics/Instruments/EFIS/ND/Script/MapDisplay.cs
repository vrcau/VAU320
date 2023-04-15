using System;
using A320VAU.Common;
using SaccFlightAndVehicles;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VirtualAviationJapan;
using YuxiFlightInstruments.BasicFlightData;
using Varneon.VUdon.ArrayExtensions;

namespace A320VAU.ND.Pages
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [DefaultExecutionOrder(1000)] // After Virtual-CNS NavaidDatabase
    public class MapDisplay : UdonSharpBehaviour
    {
        public YFI_FlightDataInterface flightData;
        public SaccEntity saccEntity;

        [Tooltip("nm")]
        public int defaultRange = 10;
        public int uiRadius = 180;

        private float scale;
        private float magneticDeclination;
        
        // Templates
        public GameObject vorTemplate, vorDmeTemplate, ndbTemplate, dmeOrTacanTemplate, waypointTemplate, airportTemplate;

        private NavaidDatabase _navaidDatabase;
        private Transform[] _vorMarkersTransform,
            _vorDmeMarkersTransform,
            _ndbMarkersTransform,
            _dmeOrTacanMarkersTransform,
            _waypointMarkersTransform,
            _airportMarkersTransform;
        
        private void Start()
        {
            _navaidDatabase = GetNavaidDatabase();
            if (_navaidDatabase == null)
            {
                VLogger.Error("Can't get NavaidDatabase instance, Map unavailable", this);
                gameObject.SetActive(false);
                return;
            }

            magneticDeclination = _navaidDatabase.magneticDeclination;
            InstantiateMarkers(defaultRange);
        }

        private void InstantiateMarkers(int range)
        {
            scale = uiRadius / (range * 926.0f);

            _vorMarkersTransform = new Transform[0];
            _vorDmeMarkersTransform = new Transform[0];
            _ndbMarkersTransform = new Transform[0];
            _dmeOrTacanMarkersTransform = new Transform[0];
            _waypointMarkersTransform = new Transform[0];
            _airportMarkersTransform = new Transform[0];
            
            for (int index = 0; index < _navaidDatabase.identities.Length; index++)
            {
                var type = (NavaidCapability)_navaidDatabase.capabilities[index];
                if (type == NavaidCapability.ILS) break;
                
                var identity = _navaidDatabase.identities[index];
                var navaidTransform = _navaidDatabase.transforms[index];

                switch (type)
                {
                    case NavaidCapability.NDB:
                        _ndbMarkersTransform = _ndbMarkersTransform.Add(InstantiateMarker(ndbTemplate, identity, navaidTransform));
                        break;
                    case NavaidCapability.VOR:
                        _vorMarkersTransform = _vorMarkersTransform.Add(InstantiateMarker(vorTemplate, identity, navaidTransform));
                        break;
                    case NavaidCapability.VORDME:
                        _vorDmeMarkersTransform = _vorDmeMarkersTransform.Add(InstantiateMarker(vorDmeTemplate, identity, navaidTransform));
                        break;
                    default:
                        _dmeOrTacanMarkersTransform = _dmeOrTacanMarkersTransform.Add(InstantiateMarker(dmeOrTacanTemplate, identity, navaidTransform));
                        break;
                }
            }

            for (int index = 0; index < _navaidDatabase.waypointIdentities.Length; index ++)
            {
                var identity = _navaidDatabase.waypointIdentities[index];
                var waypointTransform = _navaidDatabase.waypointTransforms[index];
                
                _waypointMarkersTransform = _waypointMarkersTransform.Add(InstantiateMarker(waypointTemplate, identity, waypointTransform));
            }
        }

        private Transform InstantiateMarker(GameObject template, string identity, Transform navaidTransform)
        {
            var markerTransform = Instantiate(template).transform;
            markerTransform.gameObject.name = $"Marker-{identity}";
            markerTransform.SetParent(transform, false);
            markerTransform.GetComponentInChildren<Text>().text = identity;

            var navaidPosition = navaidTransform.position * scale;
            markerTransform.localPosition = Vector3.right * navaidPosition.x + Vector3.up * navaidPosition.z;

            return markerTransform;
        }

        private void Update()
        {
            var entityTransform = saccEntity.transform;
            var rotation = Quaternion.AngleAxis(Vector3.SignedAngle(Vector3.forward, Vector3.ProjectOnPlane(entityTransform.forward, Vector3.up), Vector3.up) + magneticDeclination, Vector3.forward);
            transform.localRotation = rotation;

            var inverseRotation = Quaternion.Inverse(rotation);
            
            var position = -entityTransform.position * scale;
            transform.localPosition = rotation * (Vector3.right * position.x + Vector3.up * position.z);

            UpdateMarkerRotations(_ndbMarkersTransform, inverseRotation);
            UpdateMarkerRotations(_vorMarkersTransform, inverseRotation);
            UpdateMarkerRotations(_vorDmeMarkersTransform, inverseRotation);
            UpdateMarkerRotations(_waypointMarkersTransform, inverseRotation);
            UpdateMarkerRotations(_airportMarkersTransform, inverseRotation);
            UpdateMarkerRotations(_dmeOrTacanMarkersTransform, inverseRotation);
        }
        
        private void UpdateMarkerRotations(Transform[] markers, Quaternion rotation)
        {
            for (var i = 0; i < markers.Length; i++)
            {
                var marker = markers[i];
                if (marker == null) continue;
                marker.localRotation = rotation;
            }

        }

        public void SetRange(int range) => InstantiateMarkers(range);

        private NavaidDatabase GetNavaidDatabase()
        {
            var navaidDatabaseObject = GameObject.Find(nameof(NavaidDatabase));
            if (navaidDatabaseObject == null)
            {
                VLogger.Warn("Can't find NavaidDatabase GameObject: NavaidDatabase");
                return null;
            }

            var navaidDatabase = navaidDatabaseObject.GetComponent<NavaidDatabase>();
            if (navaidDatabase == null)
                VLogger.Warn($"Can't find NavaidDatabase Component on GameObject: {navaidDatabaseObject.name}", navaidDatabaseObject);
                
            return navaidDatabase;
        }
    }
}
