using System.Collections.Generic;
using UnityEngine;

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

    List<Grid> gridList = new List<Grid>();
    List<Vector3> offsetList = new List<Vector3>();
    bool canOnValidate = false;

    public class Grid
    {
        public GameObject surface;
        public GameObject bottom;
        public Vector3 offset;
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

        //offsetList.Add(new Vector3(-1, 0, -1));
        //offsetList.Add(new Vector3(-1, 0, 0));
        //offsetList.Add(new Vector3(-1, 0, 1));
        //offsetList.Add(new Vector3(0, 0, -1));
        offsetList.Add(new Vector3(0, 0, 0));
        //offsetList.Add(new Vector3(0, 0, 1));
        //offsetList.Add(new Vector3(1, 0, -1));
        //offsetList.Add(new Vector3(1, 0, 0));
        //offsetList.Add(new Vector3(1, 0, 1));

        CreatePlanes(offsetList);
        Shader.SetGlobalInt("_WaterDepth", depth);
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
        var surface = Instantiate(seaMeshPrefab, transform);
        surface.transform.localPosition = offset * length;
        surface.GetComponent<SeaMesh>().CreatePlane(length, segmentsPerEdge, surfaceMat);

        var bottom = Instantiate(seaMeshPrefab, transform);
        bottom.transform.localPosition = offset * length - transform.up * depth;
        bottom.GetComponent<SeaMesh>().CreatePlane(length, 2, bottomMat);

        var grid = new Grid();
        grid.offset = offset;
        grid.surface = surface;
        grid.bottom = bottom;

        return grid;
    }
}
