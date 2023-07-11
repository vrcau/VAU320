using UdonSharp;

namespace A320VAU.ECAM {
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ECAMPage : UdonSharpBehaviour {
        public virtual void OnPageInit(ECAMDisplay ecamDisplay) {}
        public virtual void OnPageUpdate() { }
    }
}