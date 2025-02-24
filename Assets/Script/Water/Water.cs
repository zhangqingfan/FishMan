using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static Water;

public partial class Water : MonoBehaviour
{
    public WaveSetting setting;
    public static Water Instance;
    List<ParticleSystem> psList = new List<ParticleSystem>();
    
    private void Awake()
    {
        Instance = this;
    }

    public void AddParticleSystem(ParticleSystem ps)
    {
        for(int i = 0; i < psList.Count; i++)
        {
            if (psList[i] == ps)
                return;
        }
        psList.Add(ps);
    }

    public void RemoveParticleSystem(ParticleSystem ps) 
    {
        for (int i = 0; i < psList.Count; i++)
        {
            if (psList[i] == ps)
            {
                psList.RemoveAt(i);
                return;
            }   
        }
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

    IEnumerator RenderTrackRT()
    {
        HashSet<Grid> gridSet = new HashSet<Grid>();
        var timeStep = new WaitForSeconds(0.02f);

        while(true)
        {
            yield return timeStep;

            foreach (var grid in gridSet)
            {
                //bug!!!!!
                //ClearRenderTarget(grid.trackRT); 
            }

            gridSet.Clear();
            
            foreach (var ps in psList)
            {
                var grids = FindGridMarginList(ps.gameObject.transform.position);
                for(int i = 0; i < grids.Count; i++)
                {
                    //Debug.Log(grids[i].trackRT.name);
                    gridSet.Add(grids[i]);
                }
            }

            foreach (var grid in gridSet)
            {
                UpdateTrackRT(grid, psList);
            }
        }
    }
}
