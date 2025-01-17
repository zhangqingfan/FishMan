using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WaterMesh))]
public class Water : MonoBehaviour
{
    public WaveSetting setting;

    void WaterUpdate(WaveSetting setting)
    {
        var count = setting.input.Count;
        if(count > 0 ) 
        {
            Shader.SetGlobalInt("dataCount", count);
            Shader.SetGlobalVectorArray("waveData", setting.GetWaveData());
        }
    }

    void Update()
    {
        WaterUpdate(setting);
    }
}
