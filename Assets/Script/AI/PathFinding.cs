using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding : MonoBehaviour
{
    static public PathFinding instance { get; private set; }
    private void Awake()
    {
        instance = this;
    }

    public GameObject startGo;
    public GameObject endGo;
    public LayerMask layer;
    [Range(100, 200)]
    public int maxConnectLength = 100;
    [HideInInspector]
    public WayPoint[] wayPoints;
    HashSet<WayPoint> openSet = new HashSet<WayPoint>();
    HashSet<WayPoint> closeSet = new HashSet<WayPoint>();

    private void Start()
    {
        wayPoints = FindObjectsOfType<WayPoint>();
        BuildConnection();
        //OnButtonClicked();
    }
    public void BuildConnection()
    {
        for (int i = 0; i < wayPoints.Length; i++)
        {
            for (int j = i + 1; j < wayPoints.Length; j++)
            {
                var start = wayPoints[i];
                var end = wayPoints[j];
                var distance = Vector3.Distance(start.Position, end.Position);

                if (distance > maxConnectLength)
                    continue;

                if (Physics.Raycast(start.Position, (end.Position - start.Position), distance, layer) == true)
                {
                    //Debug.Log("true");
                    continue;
                }

                start.neighborPointSet.Add(end);
                end.neighborPointSet.Add(start);
            }
        }
    }
    WayPoint FindNearestWayPoint(Vector3 pos)
    {
        var maxLength = float.MaxValue;
        WayPoint point = null;
        for (int i = 0; i < wayPoints.Length; i++)
        {
            var length = Vector3.Distance(wayPoints[i].Position, pos);
            if (length < maxLength)
            {
                maxLength = length;
                point = wayPoints[i];
            }
        }
        return point;
    }

    public List<Vector3> FindPath(Vector3 startPos, Vector3 endPos)
    {
        var beginWayPoint = FindNearestWayPoint(startPos);
        var endWayPoint = FindNearestWayPoint(endPos);

        if (startPos == null || endPos == null)
            return null;

        for (int i = 0; i < wayPoints.Length; i++)
            wayPoints[i].ResetCost();

        openSet.Clear();
        closeSet.Clear();
        openSet.Add(beginWayPoint);

        int loopCount = 0;
        while (loopCount < 1000)
        {
            var curWayPoint = FindMinF(openSet);
            if (curWayPoint == null)
                return null;

            if (curWayPoint == endWayPoint)
                return BuildPath(endPos, endWayPoint);
                
            openSet.Remove(curWayPoint);
            closeSet.Add(curWayPoint );

            foreach(var wayPoint in curWayPoint.neighborPointSet)
            {
                if (closeSet.Contains(wayPoint) == true)
                    continue;

                if(openSet.Contains(wayPoint) == false)
                {
                    wayPoint.g = curWayPoint.g + Vector3.Distance(curWayPoint.Position, wayPoint.Position);
                    wayPoint.h = Mathf.Abs(wayPoint.Position.x - endWayPoint.Position.x) + Mathf.Abs(wayPoint.Position.y - endWayPoint.Position.y);
                    wayPoint.parentWayPoint = curWayPoint;
                    openSet.Add(wayPoint);
                    continue;
                }

                if (wayPoint.g > curWayPoint.g + Vector3.Distance(curWayPoint.Position, wayPoint.Position))
                {
                    wayPoint.g = curWayPoint.g + Vector3.Distance(curWayPoint.Position, wayPoint.Position);
                    wayPoint.parentWayPoint = curWayPoint;
                }
            }
        }
        Debug.Log("max loop");
        return null;
    }

    List<Vector3> BuildPath(Vector3 endPos, WayPoint endWayPoint)
    {
        if(endPos == null || endWayPoint == null || endWayPoint.Position == null)
            return null;

        var path = new List<Vector3>();

        do
        {
            path.Add(endWayPoint.Position);
            endWayPoint = endWayPoint.parentWayPoint;
        }
        while (endWayPoint != null);

        path.Reverse();
        path.Add(endPos);
        return path;
    }

    WayPoint FindMinF(HashSet<WayPoint> set)
    {
        float minF = float.MaxValue;
        WayPoint point = null;
        foreach(var wayPoint in set)
        {
            if(minF > (wayPoint.g + wayPoint.h))
            {
                minF = (wayPoint.g + wayPoint.h); 
                point = wayPoint;
            }
        }
        return point;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        //return;
        if (wayPoints == null)
            return;

        Gizmos.color = Color.red;

        for (int i = 0; i < wayPoints.Length; i++)
        {
            foreach(var w in wayPoints[i].neighborPointSet) 
            {
                Gizmos.DrawLine(wayPoints[i].Position, w.Position);
            } 
        }
    }

    public void OnButtonClicked()
    {
        var startPosition = startGo.transform.position;
        var endPosition = endGo.transform.position;

        var path = FindPath(startPosition, endPosition);
        for (int i = 0; i < path.Count - 1; i++)
        {
            Debug.DrawLine(path[i], path[i + 1], Color.green, 10f);
        }
    }
#endif
}
