using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SteerBehaviour : MonoBehaviour
{
    [Header("Move")]
    public float maxSpeed = 10.0f;
    public float turnAngle = 0.5f;
    public float slowRadius = 1f;

    [Header("Wander")]
    public Vector2 wanderTimeRange = new Vector2(2, 10);
    public float wanderRange = 10f;

    [Header("Pursue")]
    public float maxPursueLength = 30f;

    public Transform targetTrans;

    Rigidbody rb;
    CollisionSensor collisionSensor;
    Coroutine wanderCoroutine;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        collisionSensor = GetComponent<CollisionSensor>();
        //Wander();
    }

    private void FixedUpdate()
    {
        var velocity = Seek(targetTrans.position);
        
        velocity = collisionSensor.AvoidCollision(velocity);
        if (velocity == Vector3.zero)
        {
            Debug.Log("I am stuck, help! " + gameObject.name);
            return;
        }

        ApplySteering(velocity);
        FaceTarget(velocity);

        Debug.DrawLine(transform.position, targetTrans.position, Color.red);
    }
    
    void ApplySteering(Vector3 acceleration)
    {
        rb.velocity += acceleration * Time.deltaTime;
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
    }

    public void  FaceTarget(Vector3 acceleration)
    {
        var rotation = Quaternion.LookRotation(acceleration);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, turnAngle * Time.deltaTime);
    }

    public Vector3 Arrive(Vector3 target)
    {
        var dir = (target - transform.position);
        var distance = (target - transform.position).magnitude;

        if (distance > slowRadius)
            return Seek(target);

        var speed = maxSpeed * distance / slowRadius;
        var desireVelocity = dir.normalized * speed;
        return desireVelocity;
    }

    public Vector3 Seek(Vector3 target)
    {
        Vector3 desiredVelocity = (target - transform.position).normalized * maxSpeed;
        return Vector3.ClampMagnitude(desiredVelocity, maxSpeed);
    }

    public void Wander()
    {
        wanderCoroutine = StartCoroutine(WanderCoroutine());
    }

    IEnumerator WanderCoroutine()
    {
        while(true)
        {
            var targetPos = new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1)) * wanderRange;
            targetTrans.position = transform.position + targetPos;
            var y = targetTrans.position.y > WorldManager.height ? WorldManager.height : targetTrans.position.y;
            targetTrans.position = new Vector3(targetTrans.position.x, y, targetTrans.position.z);
            yield return new WaitForSeconds(Random.Range(wanderTimeRange.x, wanderTimeRange.y));
        }
    }

    public Vector3 Pursue(Vector3 target, Vector3 targetVelocity)
    {
        var dir  = target - transform.position;
        var distance = dir.magnitude;

        if (distance > maxPursueLength)
            return Vector3.zero;

        float predictionTime = distance / rb.velocity.magnitude;
        Vector3 predictedPosition = target + targetVelocity * predictionTime;

        return Seek(predictedPosition);
    }
}
