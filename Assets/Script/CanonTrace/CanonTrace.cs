using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CanonTrace : MonoBehaviour
{
    [Range(0.1f, 1f)]
    public float timeStep = 0.1f;
    float initialSpeed;
    LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = transform.GetComponent<LineRenderer>();
        if (lineRenderer == null)
            lineRenderer = transform.AddComponent<LineRenderer>();
    }

    void CaculateInitialSpeed(float maxRange)
    {
        initialSpeed = Mathf.Sqrt(maxRange * Physics.gravity.y);
    }

    void DrawTrajectory(float angle)
    {
        float g = Physics.gravity.y;

        float angleRad = angle * Mathf.Deg2Rad;
        float velocityX = initialSpeed * Mathf.Cos(angleRad);
        float velocityY = initialSpeed * Mathf.Sin(angleRad);
        float flightTime = (2 * velocityY) / g;

        var positionList = new List<Vector3>();
        int pointsCount = (int)(flightTime / timeStep);
        
        for (int i = 0; i < pointsCount; i++)
        {
            float z = velocityX * timeStep;
            float y = velocityY * timeStep - 0.5f * g * timeStep * timeStep;
            positionList.Add(new Vector3(0, y, z));
        }

        lineRenderer.positionCount = pointsCount;
        lineRenderer.SetPositions(positionList.ToArray());
    }

    float CalculateLaunchAngle(float targetDistance, float initialSpeed)
    {
        float g = Physics.gravity.y;

        float sin2theta = (g * targetDistance) / (initialSpeed * initialSpeed);
        if (sin2theta > 1 || sin2theta < -1)
        {
            return 45f;
        }

        float angle2 = Mathf.Asin(sin2theta);
        float angle = angle2 / 2f;
        return angle * Mathf.Rad2Deg;
    }
}
