using System.Collections.Generic;
using UnityEngine;

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

    public bool AvoidCollision(Vector3 velocity, out Vector3 newVelocity)
    {
        newVelocity = Vector3.zero;

        if (DetectCollision(velocity) == false)
        {
            newVelocity = velocity;
            return false;
        }

        var speed = velocity.magnitude;
        var loopCount =  360 / deltaSensorAngle;

        for (int i = 1; i <= loopCount / 2; i++) 
        {
            var curDir = Quaternion.Euler(0, i * deltaSensorAngle, 0) * velocity;
            curDir.y = 0;
            //Debug.DrawLine(transform.position, transform.position + curDir, Color.cyan);
            if (DetectCollision(curDir) == false)
            {
                newVelocity = curDir.normalized * speed;
                Debug.DrawLine(transform.position, transform.position + newVelocity * 2, Color.yellow, 10f);
                break;
            }

            curDir = Quaternion.Euler(0, -1 * i * deltaSensorAngle, 0) * velocity;
            curDir.y = 0;
            //Debug.DrawLine(transform.position, transform.position + curDir, Color.cyan);
            if (DetectCollision(curDir) == false)
            {
                newVelocity = curDir.normalized * speed;                
                Debug.DrawLine(transform.position, transform.position + newVelocity * 2, Color.yellow, 10f);
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
        return RayDetect(forward, startPoints, avoidDistance, collisionLayer);
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
