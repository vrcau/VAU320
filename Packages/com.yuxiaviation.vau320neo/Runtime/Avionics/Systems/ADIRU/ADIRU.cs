using UdonSharp;

namespace A320VAU.ADIRU {
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ADIRU : UdonSharpBehaviour {
        public ADR adr;
        public IRS irs;

        public InertialReferenceMode inertialReferenceMode { get; private set; } = InertialReferenceMode.Navigation;
        public AlignState alignState { get; private set; } = AlignState.Aligned;

        private void Start() {
            adr = GetComponentInChildren<ADR>(true);
            irs = GetComponentInChildren<IRS>(true);
        }
    }

    public enum InertialReferenceMode  {
        Off,
        Navigation,
        Attitude
    }

    public enum AlignState {
        Off,
        Aligning,
        Aligned,
    }
}