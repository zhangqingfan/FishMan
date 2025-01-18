using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Water;

partial class Water : MonoBehaviour 
{
    public Material mat;
    [Range(1, 100)]
    public int segmentsPerEdge;
    [Range(10, 100)]
    public int length;

    List<Grid> grids = new List<Grid>();
    bool canOnValidate = false;

    public class Grid
    {
        public GameObject centerPoint;
        public Vector3 offset;
        public int lodLevel;
    }

    private void OnValidate()
    {
        if(canOnValidate)
        {
            for(int i = 0; i < grids.Count; i++)
            {
                Destroy(grids[i].centerPoint);
            }
            grids.Clear();
            CreatePlanes();
        }
    }

    private void Start()
    {
        canOnValidate = true;
        CreatePlanes();
    }

    void CreatePlanes()
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                var offset = new Vector3(i, 0, j);
                grids.Add(CreatePlane(this.transform, Vector3.zero + offset * length, length, segmentsPerEdge, offset));
            }
        }
    }

    public Grid CreatePlane(Transform parent, Vector3 localCenterPos, float length, int segmentsPerEdge, Vector3 gridOffset)
    {
        Mesh mesh = new Mesh();
        Mathf.Clamp(segmentsPerEdge, 2, segmentsPerEdge);
        segmentsPerEdge = (segmentsPerEdge % 2 == 0) ? segmentsPerEdge : segmentsPerEdge + 1;

        var vertex = new Vector3[(segmentsPerEdge + 1) * (segmentsPerEdge + 1)];
        var offset = length / segmentsPerEdge;
        vertex[0].x = -offset * segmentsPerEdge / 2;
        vertex[0].z = -offset * segmentsPerEdge / 2;
        vertex[0].y = 0;

        for (int i = 0; i <= segmentsPerEdge; i++)
        {
            for (int j = 0; j <= segmentsPerEdge; j++)
            {
                var index = i * (segmentsPerEdge + 1) + j;
                vertex[index].x = vertex[0].x + i * offset;
                vertex[index].z = vertex[0].z + j * offset;
                vertex[index].y = 0;
                Debug.Log(vertex[index]);
            }
        }

        var triangleCount = segmentsPerEdge * segmentsPerEdge * 2 * 3;
        var triangles = new int[triangleCount];
        var k = 0;
        for (int i = 0; i < segmentsPerEdge; i++)
        {
            for (int j = 0; j < segmentsPerEdge; j++)
            {
                triangles[k] = i * (segmentsPerEdge + 1) + j;
                triangles[k + 1] = i * (segmentsPerEdge + 1) + j + 1;
                triangles[k + 2] = (i + 1) * (segmentsPerEdge + 1) + j + 1;

                triangles[k + 3] = i * (segmentsPerEdge + 1) + j;
                triangles[k + 4] = (i + 1) * (segmentsPerEdge + 1) + j + 1;
                triangles[k + 5] = (i + 1) * (segmentsPerEdge + 1) + j;
                
                k += 6;
            }
        }

        var uvs = new Vector2[vertex.Length];
        for (int i = 0; i < vertex.Length; i++)
        {
            uvs[i] = new Vector2(0.5f, 0.5f);            
        }

        mesh.vertices = vertex;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        var grid = new Grid();
        grid.centerPoint = new GameObject("center");
        var mf = grid.centerPoint.AddComponent<MeshFilter>();
        mf.mesh = mesh;
        var mr = grid.centerPoint.AddComponent<MeshRenderer>();
        mr.material = mat;
        grid.lodLevel = 0;
        grid.centerPoint.transform.parent = parent;
        grid.centerPoint.transform.localPosition = localCenterPos;
        grid.offset = gridOffset;

        return grid;
    }
}
