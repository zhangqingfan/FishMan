using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class CanonTrace : MonoBehaviour
{
    public float timeStep = 0.05f;
    
    float initialSpeed;
    LineRenderer lineRenderer;
    float g = -Physics.gravity.y;
    Camera renderCamera;
    Vector3 hitPoint;

    void Start()
    {
        lineRenderer = transform.GetComponent<LineRenderer>();
        if (lineRenderer == null)
            lineRenderer = transform.AddComponent<LineRenderer>();

        CaculateInitialSpeed(12.3f);
        renderCamera = Camera.main;
    }

    private void Update()
    {
        var showRing = PlayerController.Instance.showRing;
        GetComponent<MeshRenderer>().enabled = showRing;
        lineRenderer.enabled = showRing;

        if (showRing == true)
        {
            var screenPosition = Input.mousePosition;
            screenPosition.z = 1f;
            var startPosition = renderCamera.ScreenToWorldPoint(screenPosition);

            screenPosition.z = 1.5f;
            var endPosition = renderCamera.ScreenToWorldPoint(screenPosition);

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(startPosition, endPosition - startPosition, out RaycastHit hitInfo, Mathf.Infinity, 1 << LayerMask.NameToLayer("Sea")))
            {
                hitPoint = hitInfo.point;
                var startPoint = gameObject.transform.position;
                var offset = hitPoint - startPoint;
                offset.y = 0;

                var rotation = Quaternion.LookRotation(offset);
                var euler = rotation.eulerAngles;
                euler.x = 0;
                euler.z = 0;
                gameObject.transform.rotation = Quaternion.Euler(euler);

                var eulerX = CalculateLaunchAngle(offset.magnitude);                
                DrawTrajectory(eulerX);
                //Debug.Log(offset.magnitude + " / " + eulerX );
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(hitPoint, 0.1f);  
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
            var worldPos = transform.TransformPoint(new Vector3(0, y, z / 2));  //Î´½âÖ®ÃÕ£¿£¿£¿£¿
            positionList.Add(worldPos);
        }

        lineRenderer.positionCount = positionList.Count;
        lineRenderer.SetPositions(positionList.ToArray());
        lineRenderer.startWidth = 0.1f;  
        lineRenderer.endWidth = 0.1f;    
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
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
