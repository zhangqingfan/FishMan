using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

[RequireComponent(typeof(BoxCollider))]
public class CollisionSensor : MonoBehaviour
{
    public LayerMask collisionLayer;
    int deltaSensorAngle = 20;
   
    BoxCollider boxCollider;
    float avoidDistance;

    void Start()
    { 
        boxCollider = GetComponent<BoxCollider>();
        avoidDistance = boxCollider.size.z;
    }

    public bool AvoidCollision(Vector3 velocity, out Vector3 newDir)
    {
        newDir = Vector3.zero;
        if (DetectCollision(velocity) == false)
        {
            return false;
        }

        var loopCount =  360 / deltaSensorAngle;

        for (int i = 1; i <= loopCount / 2; i++) 
        {
            var curDir = Quaternion.Euler(0, i * deltaSensorAngle, 0) * velocity;
            curDir.y = 0;
            if (DetectCollision(curDir) == false)
            {
                newDir = curDir.normalized;
                Debug.DrawLine(transform.position, transform.position + newDir * 2, Color.yellow, 10f);
                break;
            }

            curDir = Quaternion.Euler(0, -1 * i * deltaSensorAngle, 0) * velocity;
            curDir.y = 0;
            if (DetectCollision(curDir) == false)
            {
                newDir = curDir.normalized;                
                Debug.DrawLine(transform.position, transform.position + newDir * 2, Color.yellow, 10f);
                break;
            }
        }
        return true;
    }

    bool DetectCollision(Vector3 forward)
    {
        var centerGo = transform.Find("centerPoint");
        var offsetZ = centerGo == null ? 0 : centerGo.transform.localPosition.z;
        var centerPoint = centerGo == null ? transform.position : centerGo.transform.position;
        forward.Normalize();

        var startPoints = new List<Vector3>
        {
            boxCollider.transform.position,
            centerPoint,
            transform.TransformPoint(new Vector3(boxCollider.size.x / 2, boxCollider.size.y / 2, boxCollider.size.z / 2 + offsetZ)),
            transform.TransformPoint(new Vector3(boxCollider.size.x / 2, boxCollider.size.y / 2, -boxCollider.size.z / 2 + offsetZ)),
            transform.TransformPoint(new Vector3(-boxCollider.size.x / 2, boxCollider.size.y / 2, boxCollider.size.z / 2 + offsetZ)),
            transform.TransformPoint(new Vector3(-boxCollider.size.x / 2, boxCollider.size.y / 2, -boxCollider.size.z / 2 + offsetZ)),
            transform.TransformPoint(new Vector3(boxCollider.size.x / 2, -boxCollider.size.y / 2, boxCollider.size.z / 2 + offsetZ)),
            transform.TransformPoint(new Vector3(boxCollider.size.x / 2, -boxCollider.size.y / 2, -boxCollider.size.z / 2 + offsetZ)),
            transform.TransformPoint(new Vector3(-boxCollider.size.x / 2, -boxCollider.size.y / 2, boxCollider.size.z / 2 + offsetZ)),
            transform.TransformPoint(new Vector3(-boxCollider.size.x / 2, -boxCollider.size.y / 2, -boxCollider.size.z / 2 + offsetZ)),
        };

        for (int i = 0; i < startPoints.Count; i++)
        {
            var bo = Physics.Raycast(startPoints[i], forward, out RaycastHit hitInfo, avoidDistance, collisionLayer);
            Debug.DrawLine(startPoints[i], startPoints[i] + forward * avoidDistance, Color.red);
            if (bo == true)
                return true;
        }
        return false;
    }

    bool RayDetect(Vector3 forward, List<Vector3> startPoints, float detectLength, LayerMask collisionLayer)
    {
        var result = false;
        for (int i = 0; i < startPoints.Count; i++) 
        {
            var bo = Physics.Raycast(startPoints[i], forward, out RaycastHit hitInfo, detectLength, collisionLayer);
            Debug.DrawLine(startPoints[i], startPoints[i] + forward * detectLength, Color.red);
            result = bo == true ? true : result;
        }
        return result;
    }
}
