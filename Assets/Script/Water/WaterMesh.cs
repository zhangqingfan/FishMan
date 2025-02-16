using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Pipeline;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using static Water;

partial class Water : MonoBehaviour 
{
    [Range(2, 30)]
    public int segmentsPerEdge;
    
    [Range(10, 100)]
    public int length;

    [Range(20, 60)]
    public int depth;

    [Range(0, 10)]
    private int rtMargin = 0;

    public GameObject seaMeshPrefab;
    public Material surfaceMat;
    public Material bottomMat;

    public Transform playerTrans;
    Grid oldGrid;
    Grid curGrid;

    List<Grid> gridList = new List<Grid>();
    List<Vector3> offsetList = new List<Vector3>();

    bool canOnValidate = false;

    public List<RenderTexture> trackRTList = new List<RenderTexture>();

    public class Grid
    {
        public GameObject root;
        public Vector3 offset;
        public GameObject surface;
        public GameObject bottom;
        public RenderTexture trackRT;
    }

    private void OnValidate()
    {
        if (canOnValidate)
        {
            for (int i = 0; i < gridList.Count; i++)
            {
                Destroy(gridList[i].surface);
                Destroy(gridList[i].bottom);
                Destroy(gridList[i].root);
                gridList[i].trackRT.Release();
            }

            gridList.Clear();
            CreatePlanes(offsetList);

            Shader.SetGlobalInt("_WaterDepth", depth);
            Shader.SetGlobalFloat("_GridLength", length);
            Shader.SetGlobalVectorArray("GridWorldPosArray", GetGridWorldPos());
        }
    }

    private void Start()
    {
        for(int i = 0; i < trackRTList.Count; i++) 
        {
            Assert.IsTrue(trackRTList[i].width == trackRTList[i].height);
        }

        canOnValidate = true;

        offsetList.Add(new Vector3(-1, 0, -1));
        offsetList.Add(new Vector3(-1, 0, 0));
        offsetList.Add(new Vector3(-1, 0, 1));
        offsetList.Add(new Vector3(0, 0, -1));
        offsetList.Add(new Vector3(0, 0, 0));
        offsetList.Add(new Vector3(0, 0, 1));
        offsetList.Add(new Vector3(1, 0, -1));
        offsetList.Add(new Vector3(1, 0, 0));
        offsetList.Add(new Vector3(1, 0, 1));

        CreatePlanes(offsetList);
        Shader.SetGlobalInt("_WaterDepth", depth);
        Shader.SetGlobalFloat("_GridLength", length);
        
        //先注释掉试试看？？
        //Shader.SetGlobalVectorArray("GridWorldPosArray", GetGridWorldPos());

        StartCoroutine(CentralizeGameObject(playerTrans));
        StartCoroutine(RenderTrackRT());
    }

    void CreatePlanes(List<Vector3> offsets)
    {
        for(int i = 0; i < offsets.Count; i++)  
        {
            gridList.Add(CreatePlane(offsets[i]));
        }

        for(int i = 0; i < gridList.Count; i++) 
        {
            gridList[i].trackRT = trackRTList[i];
            ClearRenderTarget(gridList[i].trackRT);
            string rtName = $"_RT{i}";
            Shader.SetGlobalTexture(rtName, gridList[i].trackRT);
            Debug.Log(rtName);
        }
    }

    Grid CreatePlane(Vector3 offset)
    {
        var root = new GameObject("Root");
        root.transform.parent = transform;
        root.transform.localPosition = offset * length;
        root.transform.rotation = Quaternion.identity;

        var surface = Instantiate(seaMeshPrefab, root.transform);
        surface.transform.localPosition = Vector3.zero;
        surface.GetComponent<SeaMesh>().CreatePlane(length, segmentsPerEdge, surfaceMat);

        var bottom = Instantiate(seaMeshPrefab, root.transform);
        bottom.transform.localPosition = Vector3.zero - transform.up * depth;
        bottom.GetComponent<SeaMesh>().CreatePlane(length, 2, bottomMat);

        var grid = new Grid();
        grid.root = root;
        grid.offset = offset;
        grid.surface = surface;
        grid.bottom = bottom;

        return grid;
    }


    List<Vector4> worldPos = new List<Vector4>();
    List<Vector4> GetGridWorldPos()
    {
        worldPos.Clear();
        for (int i = 0; i < gridList.Count; i++)
        {
            var gridPos = gridList[i].surface.transform.position;
            var pos = new Vector4(gridPos.x, gridPos.y, gridPos.z, 1);
            worldPos.Add(gridPos);
        }
        return worldPos;
    }

    List<Matrix4x4> world2Local = new List<Matrix4x4>();
    List<Matrix4x4> GetGridWorld2Local()
    {
        world2Local.Clear();
        for (int i = 0; i < gridList.Count; i++)
        {
            var trans = gridList[i].surface.transform;
            world2Local.Add(trans.worldToLocalMatrix);
        }
        return world2Local;
    }

    IEnumerator CentralizeGameObject(Transform trans)
    {
        oldGrid = null;
        var timeStep = new WaitForSeconds(0.5f);
        
        while (true)
        {
            curGrid = FindGrid(trans.position);
            if (curGrid != oldGrid)
            {
                Debug.Log("Change Grid!!!"); 
                CentralizeGrid(curGrid);
                oldGrid = curGrid;
                Shader.SetGlobalMatrixArray("GridWorldToLocal", GetGridWorld2Local());
                Shader.SetGlobalVectorArray("GridWorldPosArray", GetGridWorldPos());
            }

            yield return timeStep;
        }
    }

    Grid FindGrid(Vector3 worldPos)
    {
        var halfLength = length * 0.5f;
        for (int i = 0; i < gridList.Count; i++)
        {
            var localPos = gridList[i].root.transform.InverseTransformPoint(worldPos);
            if(Mathf.Abs(localPos.x) <= halfLength && Mathf.Abs(localPos.z) <= halfLength)
                return gridList[i];
        }
        return null;
    }

    List<Grid> grids = new List<Grid>();
    List<Grid> FindGridMarginList(Vector3 worldPos)
    {
        grids.Clear();
        var halfLength = rtMargin + length * 0.5f;
        for (int i = 0; i < gridList.Count; i++)
        {
            var localPos = gridList[i].root.transform.InverseTransformPoint(worldPos);
            if (Mathf.Abs(localPos.x) <= halfLength && Mathf.Abs(localPos.z) <= halfLength)
            {
                grids.Add(gridList[i]);
            }
        }
        return grids;
    }


    void CentralizeGrid(Grid grid)
    {
        for(int i = 0; i < gridList.Count; i++)
        {
            var offset = gridList[i].offset - grid.offset;
            var dis = Mathf.Max(Mathf.Abs(offset.x), Mathf.Abs(offset.z));
            
            //same grid
            if(dis == 0)
            {
                continue;
            }

            //adjacent grid
            else if (dis == 1)
            {
                gridList[i].offset = Vector3.zero + offset;
            }

            //far grid
            else if(dis >= 2)
            {
                gridList[i].offset *= -1;
                gridList[i].root.transform.localPosition = grid.root.transform.localPosition + gridList[i].offset * length;
                //Debug.Log(gridList[i].offset);
            }
        }
        grid.offset = Vector3.zero;
    }

    Matrix4x4 GetViewMatrix(Grid grid)
    {
        var V = Matrix4x4.Scale(new Vector3(1, 1, -1)) * 
                Matrix4x4.TRS(grid.surface.transform.position + grid.surface.transform.up * 100f, 
                              Quaternion.AngleAxis(90f, grid.root.transform.right), 
                              Vector3.one).inverse;
        return V;
    }

    Matrix4x4 GetProjectMatrix()
    {
        var halfLength = length * 0.5f;
        var P = Matrix4x4.Ortho(-halfLength, halfLength, -halfLength, halfLength, 1f, 200f);
        return P;
    }

    void ClearRenderTarget(RenderTexture rt)
    {
        var cmd = CommandBufferPool.Get("clear cmd");

        cmd.SetRenderTarget(rt);
        cmd.ClearRenderTarget(false, true, new Color(0f, 0f, 0f));
        
        Graphics.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    void UpdateTrackRT(Grid grid, List<ParticleSystem> psList)
    {
        var cmd = CommandBufferPool.Get("my cmd");
        
        cmd.SetRenderTarget(grid.trackRT);
        //cmd.ClearRenderTarget(false, true, new Color(0f, 0f, 0f));
        cmd.SetViewProjectionMatrices(GetViewMatrix(grid), GetProjectMatrix());

        foreach(var ps in psList) 
        {
            cmd.DrawRenderer(ps.GetComponent<ParticleSystemRenderer>(), ps.GetComponent<ParticleSystemRenderer>().sharedMaterial, 0, 0);
        }
        
        Graphics.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}
