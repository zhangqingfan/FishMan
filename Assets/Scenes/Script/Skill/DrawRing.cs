using UnityEngine;

public class DrawRing : MonoBehaviour
{
    [Range(5f, 20f)]
    public float scale = 1;

    [Range(0.01f, 0.9f)]
    public float width = 0.2f;

    [Range(1, 100)]
    public int segment = 40;

    public Material mat;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    bool canOnValidate = false;

    private void Awake()
    {
        meshFilter = gameObject.GetComponent<MeshFilter>();
        if(meshFilter == null)
            meshFilter = gameObject.AddComponent<MeshFilter>();

        meshRenderer = gameObject.GetComponent<MeshRenderer>();
        if(meshRenderer == null)
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        
        CreateCircle();
        meshRenderer.enabled = false;
        canOnValidate = true;
    }

    private void OnValidate()
    {
        if (canOnValidate == true)
            CreateCircle();
    }

    private void LateUpdate()
    {
        transform.rotation = Quaternion.identity;
        var showRing = PlayerController.Instance.showRing;
        GetComponent<MeshRenderer>().enabled = showRing;
    }

    void CreateCircle()
    {
        Mesh mesh = new Mesh();

        float deltaAngle = 360f / segment;
        var vertex = new Vector3[segment * 2];
        
        for(int i = 0, j = 0; i < vertex.Length; i += 2, j++)
        { 
            var curAngle = deltaAngle * j;
            var cos = Mathf.Cos(curAngle * Mathf.Deg2Rad) ;
            var sin = Mathf.Sin(curAngle * Mathf.Deg2Rad);

            vertex[i] = new Vector3(cos * (1 - width), 0, sin * (1 - width));
            vertex[i + 1] = new Vector3(cos, 0, sin);
        }

        var triangleCount = vertex.Length * 3;
        var triangles = new int[triangleCount];
        for(int i = 0, j = 0; i < vertex.Length - 2; i += 2, j += 6)
        {
            triangles[j] = i;
            triangles[j + 1] = i + 2;
            triangles[j + 2] = i + 1;

            triangles[j + 3] = i + 2;
            triangles[j + 4] = i + 3;
            triangles[j + 5] = i + 1;
        }
        triangles[triangleCount - 6] = 0;
        triangles[triangleCount - 5] = 1;
        triangles[triangleCount - 4] = vertex.Length - 1;
        triangles[triangleCount - 3] = 0;
        triangles[triangleCount - 2] = vertex.Length - 1;
        triangles[triangleCount - 1] = vertex.Length - 2;
        
        var uvs = new Vector2[vertex.Length];
        for (int i = 0; i < vertex.Length; i++)
        {
            uvs[i] = new Vector2(0.5f + vertex[i].x / 2, 0.5f + vertex[i].z / 2);
            //Debug.Log(uvs[i]);
        }

        for (int i = 0; i < vertex.Length; i++)
        {
            //Debug.Log(vertex[i]);
            vertex[i] = vertex[i] * scale;
        }

        mesh.vertices = vertex;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        meshFilter.mesh = mesh;
        meshRenderer.material = mat;
    }
}
