using UdonSharp;

namespace A320VAU.MCDU
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class MCDUPage : UdonSharpBehaviour
    {
        public virtual void L1() { }
        public virtual void L2() { }
        public virtual void L3() { }
        public virtual void L4() { }
        public virtual void L5() { }
        public virtual void L6() { }
        public virtual void R1() { }
        public virtual void R2() { }
        public virtual void R3() { }
        public virtual void R4() { }
        public virtual void R5() { }
        public virtual void R6() { }
        public virtual void Up() { }
        public virtual void Down() { }
        public virtual void PrevPage() { }
        public virtual void NextPage() { }
        public virtual void OnPageInit(MCDU mcdu) { }
    }
}