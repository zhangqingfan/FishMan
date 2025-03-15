using System.Collections.Generic;
using UnityEngine;

public class CanonTrace : MonoBehaviour
{
    public float timeStep = 0.05f;

    float initialSpeed;
    float g = -Physics.gravity.y;
    LineRenderer lineRenderer;
    Camera renderCamera;
    Vector3 hitPoint;

    float fireCD = 2f;
    float passedFireTime = 0f;

    void Start()
    {
        lineRenderer = transform.GetComponent<LineRenderer>();
        renderCamera = Camera.main;
        var radius = transform.parent.GetComponentInChildren<DrawRing>().scale;
        CaculateInitialSpeed(radius);

        WorldManager.Instance.CreateObject("Grenade", gameObject.transform.position, 0f); // pre-warm memory pool;
    }

    void Update()
    {
        lineRenderer.enabled = PlayerController.Instance.showRing;
        float eulerX = 0f;

        if (lineRenderer.enabled == true)
        {
            var screenPosition = Input.mousePosition;
            screenPosition.z = 1f;
            var startPosition = renderCamera.ScreenToWorldPoint(screenPosition);

            screenPosition.z = 1.5f;
            var endPosition = renderCamera.ScreenToWorldPoint(screenPosition);

            //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(startPosition, endPosition - startPosition, out RaycastHit hitInfo, Mathf.Infinity, 1 << LayerMask.NameToLayer("MousePlane")))
            {
                hitPoint = hitInfo.point;
                var startPoint = gameObject.transform.position;
                var offset = hitPoint - startPoint;

                var rotation = Quaternion.LookRotation(offset);
                gameObject.transform.rotation = rotation;// Quaternion.Euler(euler);
                eulerX = CalculateLaunchAngle(offset.magnitude);
                DrawTrajectory(eulerX);
                //Debug.Log(offset.magnitude + " / " + eulerX );

                if (PlayerController.Instance.fire == true)
                {
                    PlayerController.Instance.fire = false;

                    if(passedFireTime >= fireCD)
                    {
                        var ball = WorldManager.Instance.CreateObject("Grenade", gameObject.transform.position, 10f);
                        var velocity = gameObject.transform.rotation * Quaternion.Euler(-eulerX, 0, 0) * Vector3.forward;
                        velocity = velocity.normalized * initialSpeed;
                        //velocity = transform.TransformDirection(velocity);
                        ball.GetComponent<Ball>().AddVelocity(velocity);

                        passedFireTime = 0f;
                    }
                }
            }
        }
        passedFireTime += Time.deltaTime;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(hitPoint, 0.5f);  
    }

    public void CaculateInitialSpeed(float maxRange)
    {
        initialSpeed = Mathf.Sqrt(maxRange * g);
        Debug.Log(initialSpeed);
    }

    public void DrawTrajectory(float angle)
    {
        float angleRad = angle * Mathf.Deg2Rad;
        float velocityX = initialSpeed * Mathf.Cos(angleRad);
        float velocityY = initialSpeed * Mathf.Sin(angleRad);
        float flightTime = (2 * velocityY) / g;

        var positionList = new List<Vector3>();
        int pointsCount = Mathf.FloorToInt(flightTime / timeStep) + 1;
        
        for (int i = 0; i <= pointsCount; i++)
        {
            var curTimeStep = timeStep * i;
            float z = velocityX * curTimeStep;
            float y = velocityY * curTimeStep - 0.5f * g * curTimeStep * curTimeStep;
            var worldPos = transform.TransformPoint(new Vector3(0, y, z));
            positionList.Add(worldPos);
        }

        lineRenderer.positionCount = positionList.Count;
        lineRenderer.SetPositions(positionList.ToArray());
        lineRenderer.startWidth = 0.1f;  
        lineRenderer.endWidth = 0.2f;
    }

    public float CalculateLaunchAngle(float targetDistance)
    {
        float sin2theta = (g * targetDistance) / (initialSpeed * initialSpeed);
        if (sin2theta > 1 || sin2theta < -1)
        {
            return 45f;
        }

        float angle2 = Mathf.Asin(sin2theta);
        return angle2 * Mathf.Rad2Deg / 2;
    }
}
