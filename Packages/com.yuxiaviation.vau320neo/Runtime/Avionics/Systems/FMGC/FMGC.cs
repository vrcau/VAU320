using UdonSharp;
using UnityEngine;
using VirtualAviationJapan;

namespace A320VAU.FMGC
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class FMGC : UdonSharpBehaviour
    {
        public NavaidDatabase navaidDatabase;

        public FMGCRadNav radNav;

        private void Start()
        {
            navaidDatabase = NavaidDatabase.GetInstance();
            if (!navaidDatabase) Debug.LogError("You don't have a NavaidDatabase in your scene, FMGC won't work.", this);

            radNav = GetComponentInChildren<FMGCRadNav>();
            radNav.fmgc = this;
        }
    }
}