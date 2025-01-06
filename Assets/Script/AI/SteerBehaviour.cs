using System.Collections;
using UnityEngine;

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

    [Header("Target")]
    public Transform targetTrans;

    Rigidbody rb;
    CollisionSensor collisionSensor;
    Coroutine wanderCoroutine = null;
    Vector3 originalPos;

    private void Start()
    {
        originalPos = transform.position;
        rb = GetComponent<Rigidbody>();
        collisionSensor = GetComponent<CollisionSensor>();
        //Wander();
    }

    private void Update()
    {
        var velocity = Seek(targetTrans.position);

         var result = collisionSensor.AvoidCollision(rb.velocity, out var newVelocity);
         if(result == true)
          {
                rb.velocity = newVelocity.normalized * maxSpeed;
          }

        ApplySteering(velocity);
        FaceTarget(velocity);
        //Debug.DrawLine(transform.position, transform.position + velocity, Color.green);
    }
    
    public Vector3 Seek(Vector3 target)
    {
        var v3 = target - transform.position;
        return v3.magnitude < stopDistance ? Vector3.zero : v3.normalized * maxSpeed;
    }

    void ApplySteering(Vector3 velocity)
    {
        if (velocity.magnitude > maxSpeed)
            velocity = velocity.normalized * maxSpeed;

        rb.velocity = Vector3.Lerp(rb.velocity, velocity, Time.deltaTime);
        //Debug.Log(rb.velocity.magnitude);
    }

    public void FaceTarget(Vector3 velocity)
    {
        if (rb.velocity.magnitude <= 0.06f)
            return;

        var rotation = Quaternion.LookRotation(velocity);
        rb.rotation = Quaternion.RotateTowards(rb.rotation, rotation, turnAngle * Time.deltaTime);
    }

    public Vector3 Arrive(Vector3 target)
    {
        var dir = (target - transform.position);
        var distance = (target - transform.position).magnitude;
        var slowRadius = 8f;

        if (distance > slowRadius)
            return Seek(target);

        var speed = maxSpeed * distance / slowRadius;
        var desireVelocity = dir.normalized * speed;
        desireVelocity -= rb.velocity;
        return desireVelocity;
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
        while (true)
        {
            var targetPos = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * wanderRange;
            targetTrans.position = originalPos + targetPos;
            var y = targetTrans.position.y > WorldManager.height ? WorldManager.height : targetTrans.position.y;
            targetTrans.position = new Vector3(targetTrans.position.x, y, targetTrans.position.z);
            yield return new WaitForSeconds(Random.Range(wanderTimeRange.x, wanderTimeRange.y));
        }
    }

    public Vector3 Pursue(Vector3 target, Vector3 targetVelocity)
    {
        var dir = target - transform.position;
        var distance = dir.magnitude;

        if (distance > maxPursueLength)
            return Vector3.zero;

        float predictionTime = distance / rb.velocity.magnitude;
        Vector3 predictedPosition = target + targetVelocity * predictionTime;

        return Seek(predictedPosition);
    }
}
