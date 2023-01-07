
using Assets.YuxiFlightInstruments.ECAM;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using YuxiFlightInstruments.ECAM;
public class Test : UdonSharpBehaviour
{
    public ECAMController ECAMController;

    void Start()
    {
        Debug.Log(ECAMController);
        ECAMController.IsCabinReady = true;
    }
}
