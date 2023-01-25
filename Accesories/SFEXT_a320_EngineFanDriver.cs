
using SaccFlightAndVehicles;
using UdonSharp;
using UnityEngine;

namespace A320VAU.SFEXT
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SFEXT_a320_EngineFanDriver : UdonSharpBehaviour
    {
        public Transform[] fanTransforms;
        public Vector3[] fanAxises = { Vector3.up };

        private SFEXT_a320_AdvancedEngine[] engines;
        private float[] fanAngles;
        private Vector3[] fanParentAxises;
        private Quaternion[] fanInitialRotations;
        private bool hasPilot;
        private void Start()
        {
            var entity = GetComponentInParent<SaccEntity>();
            engines = entity.gameObject.GetComponentsInChildren<SFEXT_a320_AdvancedEngine>(true);

            fanAngles = new float[engines.Length];
            fanParentAxises = new Vector3[engines.Length];
            fanInitialRotations = new Quaternion[engines.Length];

            for (var i = 0; i < engines.Length; i++)
            {
                var fan = fanTransforms[i];
                fanAngles[i] = 0;
                fanInitialRotations[i] = fan.localRotation;
                fanParentAxises[i] = fan.localRotation * fanAxises[i];
            }

            gameObject.SetActive(false);
        }

        public void SFEXT_G_PilotEnter()
        {
            hasPilot = true;
            gameObject.SetActive(true);
        }
        public void SFEXT_G_PilotExit() => hasPilot = false;

        private void Update()
        {
            var deltaTime = Time.deltaTime;
            var stopped = true;
            for (var i = 0; i < engines.Length; i++)
            {
                var engine = engines[i];
                var fan = fanTransforms[i];
                var fanAngle = fanAngles[i];
                var n1 = engine.n1;
                var fanParentAxis = fanParentAxises[i];
                var fanInitialRotation = fanInitialRotations[i];

                fanAngle += n1 * deltaTime * 360;
                fanAngles[i] = fanAngle % 360;
                fan.localRotation = Quaternion.AngleAxis(fanAngle, fanParentAxis) * fanInitialRotation;

                if (n1 > 0) stopped = false;
            }

            if (!hasPilot && stopped) gameObject.SetActive(false);
        }
    }
}