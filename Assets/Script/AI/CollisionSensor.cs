using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class CollisionSensor : MonoBehaviour
{
    public LayerMask collisionLayer;
    public int sensorCount = 8;
    public int deltaSensorAngle = 30;

    BoxCollider boxCollider;

    void Start()
    { 
        boxCollider = GetComponent<BoxCollider>();
    }

    public Vector3 AvoidCollision(Vector3 velocity)
    {
        var loopCount = 180 / deltaSensorAngle;
        var localForward = Quaternion.LookRotation(transform.InverseTransformDirection(velocity)).eulerAngles;

        for (int i = 0; i <= loopCount / 2; i++) 
        {  
            localForward.y += i * deltaSensorAngle;
            var worldForward = transform.TransformDirection(Quaternion.Euler(localForward) * Vector3.forward).normalized;
            if(DetectCollision(worldForward) == false)
            {
                return worldForward;
            }

            localForward.y -= i * deltaSensorAngle;
            worldForward = transform.TransformDirection(Quaternion.Euler(localForward) * Vector3.forward).normalized;
            if (DetectCollision(worldForward) == false)
            {
                return worldForward;
            }
        }

        return Vector3.zero;
    }

    bool DetectCollision(Vector3 forward)
    {
        var minLocalX = -boxCollider.size.x;
        var deltaLocalX = 2 * boxCollider.size.x / sensorCount;

        for (int i = 0; i < sensorCount; i++)
        {
            var localPos = new Vector3(minLocalX + deltaLocalX * i, 0, 0);
            var worldPos = transform.TransformPoint(localPos);
            var bo = Physics.Raycast(worldPos, forward, 2 * boxCollider.size.z, collisionLayer);
            if (bo == true)
                return true;
        }

        return false;
    }
}
