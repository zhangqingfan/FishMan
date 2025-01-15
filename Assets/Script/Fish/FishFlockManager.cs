using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FishFlockManager : MonoBehaviour
{
    [Range(1, 100)]
    public int flockCount;

    [Range(1, 3000)]
    public int fishNumber = 5;

    [Range(10, 100)]
    public int spawnRadius = 60;

    [Range(0.1f, 1.5f)]
    public float spawnHeightScale = 0.5f;

    [Serialize]
    public List<Transform> sharks = new List<Transform>();

    public GameObject fishPrefab;
    FishFlock[] fishFlocks;
    Vector3[] fishFlockTargets;
    
    private void Start()
    {
        fishFlocks = new FishFlock[flockCount];
        fishFlockTargets = new Vector3[flockCount];

        for (int i = 0; i < flockCount; i++) 
        {
            fishFlocks[i] = CreateFlocks(fishPrefab);
            fishFlockTargets[i] = fishFlocks[i].flockPosition;
        }

        StartCoroutine(SearchFlockTargets());
        StartCoroutine(FlockMove());
    }

    FishFlock CreateFlocks(GameObject fishPrefab)
    {
        var go = new GameObject("flock");
        go.transform.parent = transform;
        
        var flock = go.AddComponent<FishFlock>();
        flock.number = fishNumber;
        flock.spawnRadius = spawnRadius;
        flock.spawnHeightScale = spawnHeightScale;
        flock.fishPrefab = fishPrefab; 

        var pos = UnityEngine.Random.onUnitSphere * spawnRadius;
        pos.y *= spawnHeightScale;
        flock.flockPosition = pos + gameObject.transform.position;
        flock.sharkTrans = sharks;
        flock.CreateFishes();

        return flock;
    }

    IEnumerator SearchFlockTargets()
    {
        while (true)
        {
            yield return new WaitForSeconds(UnityEngine.Random.Range(1.0f, 5.0f));

            for (int i = 0; i < flockCount; i++)
            {
                if (UnityEngine.Random.value > 0.6)
                    continue;

                var pos = UnityEngine.Random.onUnitSphere * spawnRadius;
                pos.y = WorldManager.fishHeight - spawnRadius;
                pos.y *= spawnHeightScale;
                fishFlockTargets[i] = pos + gameObject.transform.position;
            }
        }
    }  

    IEnumerator FlockMove()
    {
        while (true)
        {
            yield return null;

            var v = Vector3.zero;
            for (int i = 0; i < fishFlocks.Length; i++)
            {
                fishFlocks[i].flockPosition = Vector3.SmoothDamp(fishFlocks[i].flockPosition, fishFlockTargets[i], ref v, 0.5f);
                //Debug.Log(fishFlocks[i].flockPosition);
            }
        }
    }
}
