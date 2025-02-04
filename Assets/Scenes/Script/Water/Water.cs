using Unity.VisualScripting;
using UnityEngine;

public partial class Water : MonoBehaviour
{
    public WaveSetting setting;
    public static Water Instance;

    private void Awake()
    {
        Instance = this;
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
