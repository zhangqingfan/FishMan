using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class CollisionSensor : MonoBehaviour
{
    public LayerMask collisionLayer;
    int sensorCount = 4;
    int deltaSensorAngle = 20;
    float detectLength = 0;

    BoxCollider boxCollider;

    void Start()
    { 
        boxCollider = GetComponent<BoxCollider>();
        detectLength = 4 * boxCollider.size.z;
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
            //Debug.DrawLine(transform.position, transform.position + curDir, Color.cyan);
            if (DetectCollision(curDir) == false)
            {
                newVelocity = curDir.normalized * speed;
                //Debug.DrawLine(transform.position, transform.position + newVelocity * 2, Color.yellow, 10f);
                break;
            }

            curDir = Quaternion.Euler(0, -1 * i * deltaSensorAngle, 0) * velocity;
            //Debug.DrawLine(transform.position, transform.position + curDir, Color.cyan);
            if (DetectCollision(curDir) == false)
            {
                newVelocity = curDir.normalized * speed;                
                //Debug.DrawLine(transform.position, transform.position + newVelocity * 2, Color.yellow, 10f);
                break;
            }
        }
        return true;
    }

    bool DetectCollision(Vector3 forward)
    {
        var deltaLocalX = 2 * boxCollider.size.x / sensorCount;
        var verticalDir = Quaternion.Euler(0, 90, 0) * forward;
        verticalDir.Normalize();

        var result = false;
        for (int i = 0; i < sensorCount / 2; i++)
        {
            var startPos = transform.position + deltaLocalX * i * verticalDir;
            var bo = Physics.Raycast(startPos, forward, out RaycastHit hitInfo, detectLength, collisionLayer);
            //Debug.DrawLine(startPos, startPos + forward * detectLength, Color.red);
            result = bo == true ? true : result;
             
            startPos = transform.position + deltaLocalX * -i * verticalDir;
            bo = Physics.Raycast(startPos, forward, out hitInfo, detectLength, collisionLayer);
            //Debug.DrawLine(startPos, startPos + forward * detectLength, Color.red);
            result = bo == true ? true : result;
        }
        return result;
    }
}
