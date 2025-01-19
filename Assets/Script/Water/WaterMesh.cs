using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

partial class Water : MonoBehaviour 
{
    [Range(2, 30)]
    public int segmentsPerEdge;
    
    [Range(10, 100)]
    public int length;
    
    public GameObject seaMeshPrefab;
    public Material mat;

    List<Grid> gridList = new List<Grid>();
    List<Vector3> offsetList = new List<Vector3>();
    bool canOnValidate = false;

    public class Grid
    {
        public GameObject mesh;
        public Vector3 offset;
    }

    private void OnValidate()
    {
        if (canOnValidate)
        {
            for (int i = 0; i < gridList.Count; i++)
            {
                Destroy(gridList[i].mesh);
            }
            
            gridList.Clear();
            CreatePlanes(offsetList);
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
        var mesh = Instantiate(seaMeshPrefab, transform);
        mesh.transform.localPosition = offset * length;
        mesh.GetComponent<SeaMesh>().CreatePlane(length, segmentsPerEdge, mat);

        var grid = new Grid();
        grid.offset = offset;
        grid.mesh = mesh;

        return grid;
    }
}
