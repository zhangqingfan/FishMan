using UnityEngine;

public class ShipController : MonoBehaviour
{
    public int momentum = 1500;
    [Range(0.1f, 1f)]
    public float backwardFactor = 0.5f;
    public float turnAngle = 0.5f;
    [Range(10f, 50f)]
    public float mouseSpeed = 25f;

    public Transform ring;
    public CanonTrace cannonTrace;

    Camera renderCamera;
    Vector3 cameraEuler = new Vector3(300f, 180f, 0f);
    float cameraDistance = 20f;
    float scrollSpeed = 200f;

    public Transform wheelTrans;
    public float wheelSpeed = 0.01f;
    float rotateY = 0f;

    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        renderCamera = Camera.main;
        var rotation = Quaternion.Euler(cameraEuler);
        var direction = (rotation * Vector3.forward).normalized;
        renderCamera.transform.position = transform.position + direction * cameraDistance;

        cannonTrace.CaculateInitialSpeed(ring.GetComponent<DrawRing>().scale);
    }

    private void LateUpdate()
    {
        if (PlayerController.Instance.isHold == true)
        {
            var mouseOffset = PlayerController.Instance.GetMouseOffset();
            if (mouseOffset != Vector2.zero)
            {
                cameraEuler.x += mouseOffset.y * mouseSpeed * Time.deltaTime;
                cameraEuler.y += mouseOffset.x * mouseSpeed * Time.deltaTime;
                cameraEuler.x = Mathf.Clamp(cameraEuler.x, 290f, 340f);
            }
        }

        var scroll = PlayerController.Instance.GetMouseScroll();
        if (scroll.y != 0)
        {
            cameraDistance += (scrollSpeed * Time.deltaTime * scroll.y) / -120f;
            cameraDistance = Mathf.Clamp(cameraDistance, 20, 80);
        }

        var rotation = Quaternion.Euler(cameraEuler);
        var direction = (rotation * Vector3.forward);
        renderCamera.transform.position = transform.position + direction * cameraDistance;
        renderCamera.transform.LookAt(transform.position);
    }

    void Update()
    {
        var move = PlayerController.Instance.GetMovement();
        if(move.y < 0) 
        {
            move.y *= backwardFactor;
            move.x *= -1;
        }

        rb.AddRelativeForce(momentum * move.y * Vector3.forward);

        var rotationY = Quaternion.AngleAxis(turnAngle * move.x, Vector3.up);
        transform.localRotation *= rotationY;

        if(move.x != 0)
        {
            rotateY += move.x * wheelSpeed;
            rotateY = Mathf.Clamp(rotateY, -60f, 60f);
            var newEuler = new Vector3(wheelTrans.localEulerAngles.x, rotateY, wheelTrans.localEulerAngles.z);
            wheelTrans.localEulerAngles = newEuler;
        }

        else if (move.x == 0 && rotateY != 0)
        {
            rotateY = rotateY > 0 ? rotateY - wheelSpeed / 3 : rotateY + wheelSpeed / 3;
            var newEuler = new Vector3(wheelTrans.localEulerAngles.x, rotateY, wheelTrans.localEulerAngles.z);
            wheelTrans.localEulerAngles = newEuler;
        }

        var showRing = PlayerController.Instance.showRing;
        ring.GetComponent<MeshRenderer>().enabled = showRing;

        if (showRing == true)
        {
            var screenPosition = Input.mousePosition;
            screenPosition.z = 10f;  
            var startPosition = renderCamera.ScreenToWorldPoint(screenPosition);

            screenPosition.z = 20f;
            var endPosition = renderCamera.ScreenToWorldPoint(screenPosition);

            if (Physics.Raycast(startPosition, endPosition - startPosition, out RaycastHit hitInfo, Mathf.Infinity, 1 << LayerMask.NameToLayer("Sea")))
            {
                var hitPoint = hitInfo.point;
                var startPoint = cannonTrace.gameObject.transform.position;
                var offset = hitPoint - startPoint;
                offset.y = 0;

                var roataion = Quaternion.LookRotation(offset);
                var eulerY = cannonTrace.CalculateLaunchAngle(offset.magnitude);
                cannonTrace.gameObject.transform.rotation = roataion * Quaternion.Euler(0, eulerY, 0);
                cannonTrace.DrawTrajectory(eulerY);
            }
        }
    }
}
