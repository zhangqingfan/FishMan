using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawCircle : MonoBehaviour
{
    [Range(0.1f, 10)]
    public float scale = 1;

    //[Range(40, 100)]
    public int segment = 4;

    public Material mat;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;

    private void Start()
    {
        meshFilter = gameObject.GetComponent<MeshFilter>();
        if(meshFilter == null )
            meshFilter = gameObject.AddComponent<MeshFilter>();

        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        if(meshRenderer == null )
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        
        CreateCircle();
    }

    void CreateCircle()
    {
        Mesh mesh = new Mesh();

        float deltaAngle = 360f / segment;
        var vertex = new Vector3[segment + 1];
        vertex[0] = Vector3.zero;
        
        for(int i = 1; i < vertex.Length; i++)
        { 
            var curAngle = deltaAngle * (i - 1);
            var cos = Mathf.Cos(curAngle * Mathf.Deg2Rad) ;
            var sin = Mathf.Sin(curAngle * Mathf.Deg2Rad);
            vertex[i] = new Vector3(cos, 0, sin);

            Debug.Log(vertex[i]);
        }

        var triangleCount = segment * 3;
        var triangles = new int[triangleCount];
        var j = 1;
        for(int i = 0; i < segment - 3; i += 3)
        {
            triangles[i] = 0;
            triangles[i + 1] = j;
            triangles[i + 2] = j + 1;
            j++;
        }
        triangles[triangleCount - 3] = 0;
        triangles[triangleCount - 2] = j;
        triangles[triangleCount - 1] = 1;

        var uvs = new Vector2[vertex.Length];
        for (int i = 1; i < vertex.Length; i++)
        {

        }

        for (int i = 1; i < vertex.Length; i++)
        {
            vertex[i] = vertex[i] * scale;
        }

        mesh.vertices = vertex;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        meshFilter.mesh = mesh;
    }
}
