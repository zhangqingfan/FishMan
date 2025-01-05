using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CollisionSensor))]
[RequireComponent(typeof(Rigidbody))]
public class SteerBehaviour : MonoBehaviour
{
    [Header("Move")]
    [Range(2, 10)]
    public float maxSpeed = 10.0f;
    readonly float turnAngle = 30f;
    readonly float stopDistance = 0.1f;

    [Header("Wander")]
    public Vector2 wanderTimeRange = new Vector2(2, 10);
    public float wanderRange = 10f;

    [Header("Pursue")]
    public float maxPursueLength = 30f;

    [Header("Target")]
    public Transform targetTrans;

    Rigidbody rb;
    CollisionSensor collisionSensor;
    Coroutine wanderCoroutine;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        collisionSensor = GetComponent<CollisionSensor>();
        //wanderCoroutine = Wander();
    }

    private void Update()
    {
        var velocity = Seek(targetTrans.position);

        //var result = collisionSensor.AvoidCollision(velocity, out var newVelocity);
        //if(result == true)
        {
            var result = collisionSensor.AvoidCollision(rb.velocity, out var newVelocity);
            if(result == true)
            {
                rb.velocity = newVelocity.normalized * maxSpeed;
                Debug.Log("11");
            }

            /*if (newVelocity == Vector3.zero)
            {
                Debug.Log("I am stuck, help! " + gameObject.name);
                return;
            }
            Debug.DrawLine(transform.position, transform.position + newVelocity.normalized * maxSpeed, Color.red);
            rb.velocity = newVelocity.normalized * maxSpeed;
            */
        }

        ApplySteering(velocity);
        FaceTarget(velocity);

        Debug.DrawLine(transform.position, transform.position + velocity, Color.green);
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

    public void Wander()
    {
        wanderCoroutine = StartCoroutine(WanderCoroutine());
    }

    IEnumerator WanderCoroutine()
    {
        while (true)
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
        var dir = target - transform.position;
        var distance = dir.magnitude;

        if (distance > maxPursueLength)
            return Vector3.zero;

        float predictionTime = distance / rb.velocity.magnitude;
        Vector3 predictedPosition = target + targetVelocity * predictionTime;

        return Seek(predictedPosition);
    }
}
