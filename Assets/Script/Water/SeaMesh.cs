using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeaMesh : MonoBehaviour
{
    public void CreatePlane(float length, int segmentsPerEdge, Material mat)
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

        var mf = gameObject.AddComponent<MeshFilter>();
        mf.mesh = mesh;
        var mr = gameObject.AddComponent<MeshRenderer>();
        mr.material = mat;
     }
}
