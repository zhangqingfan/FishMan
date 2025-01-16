using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPoint : MonoBehaviour
{
    public Vector3 Position => gameObject.transform.position;
    public HashSet<WayPoint> neighborPointSet = new HashSet<WayPoint>();
    public WayPoint parentWayPoint;
    public void ResetCost()
    {
        g = 0;
        h = 0;
        parentWayPoint = null;
    }

    public float GetCost()
    {
        return g + h;
    }

    public float g;
    public float h;
}
