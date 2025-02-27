using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class Ball : MonoBehaviour
{
    public Rigidbody rb;
    public ParticleSystem ps;

    public void AddVelocity(Vector3 velocity)
    {
        rb.velocity = velocity;
    }

    private void OnEnable()
    {
        StartCoroutine(SpawnSplash());
        var emissionModule = ps.emission;
        emissionModule.rateOverTime = 120;
    }

    private void OnDisable()
    {
        var emissionModule = ps.emission;
        emissionModule.rateOverTime = 0;
    }

    IEnumerator SpawnSplash()
    {
        while(true)
        {
            yield return null;
            if (transform.position.y < WorldManager.seaHeight)
            {
                WorldManager.Instance.CreateObject("Splash", transform.position);
                yield break;
            }
        }
    }
}
