using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Pipeline;
using UnityEngine;
using static Water;

partial class Water : MonoBehaviour 
{
    [Range(2, 30)]
    public int segmentsPerEdge;
    
    [Range(10, 100)]
    public int length;

    [Range(20, 60)]
    public int depth;
    
    public GameObject seaMeshPrefab;
    public Material surfaceMat;
    public Material bottomMat;
    
    public Transform goTrans;
    Grid oldGrid;
    Grid curGrid;

    List<Grid> gridList = new List<Grid>();
    List<Vector3> offsetList = new List<Vector3>();
    bool canOnValidate = false;

    public class Grid
    {
        public GameObject root;
        public Vector3 offset;
        public GameObject surface;
        public GameObject bottom;
    }

    private void OnValidate()
    {
        if (canOnValidate)
        {
            for (int i = 0; i < gridList.Count; i++)
            {
                Destroy(gridList[i].surface);
                Destroy(gridList[i].bottom);
            }
            
            gridList.Clear();
            CreatePlanes(offsetList);
            Shader.SetGlobalInt("_WaterDepth", depth);
        }
    }

    private void Start()
    {
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

        StartCoroutine(CentralizeGameObject(goTrans));
    }

    void CreatePlanes(List<Vector3> offsets)
    {
        for(int i = 0; i < offsets.Count; i++) 
        {
            gridList.Add(CreatePlane(offsets[i]));
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

    IEnumerator CentralizeGameObject(Transform trans)
    {
        if(trans == null)
            yield break;

        var grid = FindGrid(trans.position);
        oldGrid = grid;
        curGrid = grid;
        CentralizeGrid(grid);

        var delayTime = new WaitForSeconds(0.5f);
        while (true)
        {
            yield return delayTime;

            curGrid = FindGrid(trans.position);
            if (curGrid != oldGrid)
            {
                CentralizeGrid(curGrid);
                oldGrid = curGrid;
            }
        }
    }

    Grid FindGrid(Vector3 worldPos)
    {
        for (int i = 0; i < gridList.Count; i++)
        {
            var localPos = gridList[i].root.transform.InverseTransformPoint(worldPos);
            if(Mathf.Abs(localPos.x) <= length / 2 && Mathf.Abs(localPos.z) <= length / 2)
                return gridList[i];
        }
        return null;
    }

    //算法要好好想一想！！
    void CentralizeGrid(Grid grid)
    {
        var moveOffset = grid.offset - Vector3.zero;
        for(int i = 0; i < gridList.Count; i++)
        {
            gridList[i].root.transform.localPosition += moveOffset * length;
            gridList[i].offset += moveOffset;
        }
    }
}
