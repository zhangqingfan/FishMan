using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public partial class Water : MonoBehaviour
{
    public WaveSetting setting;
    public static Water Instance;

    Dictionary<Transform, ParticleSystem> psDict = new Dictionary<Transform, ParticleSystem>();

    private void Awake()
    {
        Instance = this;
    }

    public void AddParticleSystem(Transform goTrans)
    {
        var ps = goTrans.GetComponent<ParticleSystem>();
        if (ps != null) 
        {
            psDict[goTrans] = ps;
        }
    }

    public void RemoveParticleSystem(Transform goTrans) 
    {
        psDict.Remove(goTrans);
    }

    void WaterUpdate(WaveSetting setting)
    {
        var count = setting.input.Count;
        if(count > 0) 
        {
            Shader.SetGlobalInt("dataCount", count);
            Shader.SetGlobalVectorArray("waveData", setting.GetWaveData());
        }
    }

    void Update()
    {
        if(setting.isChanged == true)
        {
            WaterUpdate(setting);
            setting.isChanged = false;
        }

        //实际没有用到
        var vpMatrix = Camera.main.projectionMatrix * Camera.main.worldToCameraMatrix;
        Shader.SetGlobalMatrix("_InverseVP", vpMatrix.inverse);
    }
}
