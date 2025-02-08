using UnityEngine;
using UnityEngine.UIElements;

public class Ball : MonoBehaviour
{
    public Rigidbody rb;
    
    public void AddVelocity(Vector3 velocity)
    {
        rb.velocity = velocity;
    }

    void Update()
    {
        //todo...????????
        if(transform.position.y < WorldManager.seaHeight) 
        {
            WorldManager.Instance.CreateObject("Splash", transform.position);
            WorldManager.Instance.ReleaseObject("Ball", gameObject);           
        }
    }
}
