using UnityEngine;

public partial class Water : MonoBehaviour
{
    public WaveSetting setting;

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

        var vpMatrix = Camera.main.projectionMatrix * Camera.main.worldToCameraMatrix;
        Shader.SetGlobalMatrix("_InverseVP", vpMatrix.inverse);
    }
}
