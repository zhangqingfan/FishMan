using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ShipController : MonoBehaviour
{
    public int momentum = 1500;
    [Range(0.1f, 1f)]
    public float backwardFactor = 0.5f;
    public float turnAngle = 0.5f;

    public Transform wheelTrans;
    public float wheelSpeed = 0.01f;

    float rotateY = 0f;
    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
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
    }
}
