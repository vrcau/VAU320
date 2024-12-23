﻿using JetBrains.Annotations;
using UdonSharp;
using VirtualCNS;

namespace A320VAU.FMGC {
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class FMGCRadNav : UdonSharpBehaviour {
        public FMGC fmgc;

        public NavSelector VOR1;
        public NavSelector VOR2;

        // DME frequency is sync with VOR/ILS

        public NavSelector ILS;

        public NavSelector ADF;

        [PublicAPI]
        public bool SetVORByName(int index, string identity) {
            var navaidIndex = fmgc.navaidDatabase._FindIndexByIdentity(identity);
            return navaidIndex != -1 && SetVORByIndex(index, navaidIndex);
        }

        [PublicAPI]
        public bool SetVORByFrequency(int index, float frequency) {
            var navaidIndex = fmgc.navaidDatabase._FindIndexByFrequency(frequency);
            return navaidIndex != -1 && SetVORByIndex(index, navaidIndex);
        }

        [PublicAPI]
        public void SetVORCourse(int index, int course) {
            switch (index) {
                case 1:
                    VOR1.Course = course;
                    break;
                case 2:
                    VOR2.Course = course;
                    break;
            }
        }

        [PublicAPI]
        public bool SetVORByIndex(int index, int navaidIndex) {
            if (!fmgc.navaidDatabase._IsVOR(navaidIndex)) return false;

            switch (index) {
                case 1:
                    VOR1._SetIndex(navaidIndex);
                    break;
                case 2:
                    VOR2._SetIndex(navaidIndex);
                    break;
                default:
                    return false;
            }

            return true;
        }

        [PublicAPI]
        public bool SetILSByName(string identity) {
            var navaidIndex = fmgc.navaidDatabase._FindIndexByIdentity(identity);
            if (navaidIndex == -1 || !fmgc.navaidDatabase._IsILS(navaidIndex)) return false;

            ILS._SetIndex(navaidIndex);
            return true;
        }

        [PublicAPI]
        public bool SetILSByFrequency(float frequency) {
            var navaidIndex = fmgc.navaidDatabase._FindIndexByFrequency(frequency);
            if (!fmgc.navaidDatabase._IsILS(navaidIndex)) return false;

            ILS._SetIndex(navaidIndex);
            return true;
        }

        [PublicAPI]
        public void SetILSCourse(int course) {
            ILS.Course = course;
        }

        [PublicAPI]
        public bool SetADFByName(string identity) {
            var navaidIndex = fmgc.navaidDatabase._FindIndexByIdentity(identity);
            if (navaidIndex != -1 || !fmgc.navaidDatabase._IsNDB(navaidIndex)) return false;

            ADF._SetIndex(navaidIndex);
            return true;
        }

        [PublicAPI]
        public bool SetADFByFrequency(float frequency) {
            var navaidIndex = fmgc.navaidDatabase._FindIndexByFrequency(frequency);
            if (!fmgc.navaidDatabase._IsNDB(navaidIndex)) return false;

            ADF._SetIndex(navaidIndex);
            return true;
        }
    }
}