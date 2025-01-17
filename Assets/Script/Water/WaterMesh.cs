using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WaterMesh: MonoBehaviour 
{
    public Material mat;
    [Range(2, 100)]
    public int segmentsPerEdge = 2;
    [Range(10, 100)]
    public int length = 10;

    List<Grid> grids = new List<Grid>();
    bool canOnValidate = false;
    public class Grid
    {
        public GameObject centerPoint;
        public int lodLevel;
    }

    private void OnValidate()
    {
        if(canOnValidate)
            ModifyPlane(grids[0], length, segmentsPerEdge);
    }

    private void Start()
    {
        grids.Add(CreatePlane(this.transform, Vector3.zero, 10, 2));
        canOnValidate = true;
    }

    void ModifyPlane(Grid grid, float length, int segmentsPerEdge)
    {
        var centerpoint = grid.centerPoint.transform.localPosition;
        Destroy(grid.centerPoint);
        grids.Clear();
        grids.Add(CreatePlane(transform, centerpoint, length, segmentsPerEdge));
    }

    public Grid CreatePlane(Transform parent, Vector3 localCenterPos, float length, int segmentsPerEdge)
    {
        Mesh mesh = new Mesh();
        Mathf.Clamp(segmentsPerEdge, 2, segmentsPerEdge);
        segmentsPerEdge = (segmentsPerEdge % 2 == 0) ? segmentsPerEdge : segmentsPerEdge + 1;

        var vertex = new Vector3[(segmentsPerEdge + 1) * (segmentsPerEdge + 1)];
        var offset = length / segmentsPerEdge;
        vertex[0].x = localCenterPos.x - offset * segmentsPerEdge / 2;
        vertex[0].z = localCenterPos.y - offset * segmentsPerEdge / 2;
        vertex[0].y = 0;

        for (int i = 0; i <= segmentsPerEdge; i++)
        {
            for (int j = 0; j <= segmentsPerEdge; j++)
            {
                var index = i * (segmentsPerEdge + 1) + j;
                vertex[index].x = vertex[0].x + i * offset;
                vertex[index].z = vertex[0].z + j * offset;
                vertex[index].y = 0;
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

        return grid;
    }
}
