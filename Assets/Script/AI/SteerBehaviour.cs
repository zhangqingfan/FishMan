using System;
using System.Collections;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[RequireComponent(typeof(CollisionSensor))]
[RequireComponent(typeof(Rigidbody))]
public class SteerBehaviour : MonoBehaviour
{
    [Header("Move")]
    [Range(2, 10)]
    public float maxSpeed = 10.0f;
    [Range(30, 90)]
    public float turnAngle = 30f;
    readonly float stopDistance = 0.1f;

    [Header("Wander")]
    public Vector2 wanderTimeRange = new Vector2(2, 10);
    public float wanderRange = 30f;

    [Header("Pursue")]
    public float maxPursueLength = 30f;

    [HideInInspector]
    public Transform targetTrans;
    
    Rigidbody rb;
    Vector3 steerVelocity;

    CollisionSensor collisionSensor;
    Coroutine wanderCoroutine = null;
    Vector3 originalPos;

    private void Start()
    {
        originalPos = transform.position;
        rb = GetComponent<Rigidbody>();
        collisionSensor = GetComponent<CollisionSensor>();
        targetTrans = new GameObject("SharkTarget").transform;
    }

    private void Update()
    {
         var result = collisionSensor.AvoidCollision(rb.velocity, out var newVelocity);
         if(result == true)
          {
            rb.velocity = newVelocity.normalized * maxSpeed * 0.5f;
            //rb.velocity = Vector3.Lerp(rb.velocity, newVelocity.normalized * maxSpeed, Time.deltaTime);
        }

        ApplySteering();
        FaceTarget();
        //Debug.DrawLine(transform.position, transform.position + velocity, Color.green);
    }
    
    public void Stop()
    {
        steerVelocity = Vector3.zero;
        rb.velocity = Vector3.zero;
    }

    public void Seek(Vector3 target)
    {
        var v3 = target - transform.position;
        steerVelocity = v3.magnitude < stopDistance ? Vector3.zero : v3.normalized * maxSpeed;
    }

    void ApplySteering()
    {
        if (steerVelocity.magnitude > maxSpeed)
            steerVelocity = steerVelocity.normalized * maxSpeed;

        rb.velocity = Vector3.Lerp(rb.velocity, steerVelocity, Time.deltaTime);
    }

    public void FaceTarget()
    {
        if (rb.velocity.magnitude <= 0.06f)
            return;

        var rotation = Quaternion.LookRotation(steerVelocity);
        rb.rotation = Quaternion.RotateTowards(rb.rotation, rotation, turnAngle * Time.deltaTime);
    }

    public void Arrive(Vector3 target)
    {
        var dir = (target - transform.position);
        var distance = (target - transform.position).magnitude;
        var slowRadius = 8f;

        if (distance > slowRadius)
        {
            Seek(target);
            return;
        } 

        var speed = maxSpeed * distance / slowRadius;
        var desireVelocity = dir.normalized * speed;
        desireVelocity -= rb.velocity;
        steerVelocity = desireVelocity;
    }

    public void Wander(bool bo)
    {
        if(bo == false && wanderCoroutine != null)
        {
            StopCoroutine(wanderCoroutine);
            wanderCoroutine = null;
            return;
        }

        else if(bo == true && wanderCoroutine == null)
        {
            wanderCoroutine = StartCoroutine(WanderCoroutine());
            return;
        }        
    }

    IEnumerator WanderCoroutine()
    {
        var targetPos = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)) * wanderRange;
        targetTrans.position = originalPos + targetPos;
        var y = targetTrans.position.y > WorldManager.fishHeight ? WorldManager.fishHeight : targetTrans.position.y;
        targetTrans.position = new Vector3(targetTrans.position.x, y, targetTrans.position.z);

        var wanderTime = UnityEngine.Random.Range(wanderTimeRange.x, wanderTimeRange.y);
        var passTime = 0f;

        while (true)
        {
            yield return null;

            passTime += Time.deltaTime;
            if (passTime >= wanderTime)
                break;

            Arrive(targetTrans.position);
        }
        wanderCoroutine = null;
    }

    public void Pursue(Vector3 target, Vector3 targetVelocity)
    {
        var dir = target - transform.position;
        var distance = dir.magnitude;

        if (distance > maxPursueLength)
        { 
            steerVelocity = Vector3.zero;
            return;
        } 

        float predictionTime = distance / rb.velocity.magnitude;
        Vector3 predictedPosition = target + targetVelocity * predictionTime;

        Seek(predictedPosition);
    }
}
