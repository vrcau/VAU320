
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using YuxiFlightInstruments.BasicFlightData;

namespace A320VAU.AtmosphereModel
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class EarthAtmosphereModel : UdonSharpBehaviour
    {
        /*根据当前飞行高度，计算大气静参数
         * 根据当前飞行马赫数，计算大气总参数
         * 静温 总温
         * 静压 总压
         * 密度
         * 公式来自 Earth Atmosphere Model, Glenn Research Center, NASA
         * ...
         * 考虑之后使用其他飞行系统的可能，从YFI_FlightDataInterface里读数据
         * 这里实际存在一个逻辑错误，现实中的过程是->大气参数->飞行数据->飞机系统
         * 这里变为了 飞行数据->大气参数->飞机系统
         * 不过VRC里干嘛管那么多捏
         * 千万注意单位!
         */
        public YFI_FlightDataInterface flightData;
        //public float altitude {
        //    get => _altitude;
        //    set
        //    {
        //        Debug.Log(TemperatureStatic);
        //        Debug.Log(PressuerStatic);
        //        Debug.Log(Rho);
        //        Debug.Log(MachNumber);
        //        Debug.Log(TemperatureTotal);
        //        Debug.Log(PressureTotal);
        //        _altitude = value;
        //    }
        //}

        public float altitude => flightData.altitude;
        //大气静温度(摄氏度)
        public float TemperatureStatic => 
           ((altitude/ 3.28084f) <11000f)?(15.04f-0.00649f* (altitude / 3.28084f)) :
            ((altitude / 3.28084f) < 25000f)?(-56.46f):(-131.21f+0.00299f* (altitude / 3.28084f));
        //大气静压（千帕）
        public float PressuerStatic =>
            ((altitude / 3.28084f) < 11000f)?(101.29f*Mathf.Pow((TemperatureStatic+273.15f) /288.08f,5.256f)):
            ((altitude / 3.28084f) < 25000f) ?(22.65f*Mathf.Exp((1.73f-0.000157f* (altitude / 3.28084f)))):
            (2.488f*Mathf.Pow((TemperatureStatic+273.1f)/216.6f,-11.388f));

        //大气密度（公斤/立方米）
        public float Rho =>
            PressuerStatic / (0.2869f * (TemperatureStatic + 273.1f));

        //马赫数
        public float MachNumber => flightData.mach;

        //总参数计算：气体绝热常数取1.4
        //总温(K)
        public float TemperatureTotal =>
            (TemperatureStatic+273.15f) * (1f + (1.4f - 1f) / 2f * MachNumber * MachNumber)-273.15f;

        //总压(kPa)
        public float PressureTotal =>
            PressuerStatic * Mathf.Pow(1+(1.4f-1f)/2f* MachNumber* MachNumber, 1.4f / (1.4f - 1f));
    }
}
