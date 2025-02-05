using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatController : MonoBehaviour
{
    Rigidbody rb;
    BoxCollider boxCol;
    
    public float voxelUnit = 0.4f;
    readonly float transformDensity = 500f;
    readonly float waterDensity = 1000f;
    
    List<Vector3> voxelList = new List<Vector3>();
    float voxelFloatForce;

    List<Vector3> forceList = new List<Vector3>();
    List<Vector3> forcePoint = new List<Vector3>();

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        boxCol = GetComponent<BoxCollider>();

        var bound = boxCol.bounds;
        var X = (int)(bound.size.x / voxelUnit);
        var Y = (int)(bound.size.y / voxelUnit);
        var Z = (int)(bound.size.z / voxelUnit);

        for(int i = 0; i < X; i++)
        {
            for(int j = 0; j < Y; j++)
            {
                for (int k = 0; k < Z; k++)
                {
                    var _x = bound.min.x + i * voxelUnit + voxelUnit / 2;
                    var _y = bound.min.y + j * voxelUnit + voxelUnit / 2;
                    var _z = bound.min.z + k * voxelUnit + voxelUnit / 2;
                    var pos = new Vector3(_x, _y, _z);
                    voxelList.Add(transform.InverseTransformPoint(pos));
                }
            }
        }

        var volume = rb.mass / transformDensity;
        voxelFloatForce = waterDensity * Mathf.Abs(Physics.gravity.y) * (volume / voxelList.Count);
        Debug.Log(voxelFloatForce);
        /*
        var totalG = rb.mass * Mathf.Abs(Physics.gravity.y);
        Debug.Log(totalG);
        Debug.Log(voxelFloatForce * voxelList.Count);
        */
    }

    void FixedUpdate()
    {
        AppleFloatForce();
    }

    void AppleFloatForce()
    {
        for(int i = 0; i < voxelList.Count; i++)
        {
            var voxelWorldPos = transform.TransformPoint(voxelList[i]);
            var voxelBottom = voxelWorldPos.y - voxelUnit / 2;
            var waterWorldHeight = Water.Instance.GetHeight(voxelWorldPos);

            if (voxelBottom < waterWorldHeight)
            {
                var floatforce = Vector3.zero;
                floatforce.y += voxelFloatForce * Mathf.Clamp01((waterWorldHeight - voxelBottom) / voxelUnit);
                rb.AddForceAtPosition(floatforce, voxelWorldPos);
            }
        }
    }
}
