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

    [Serialize]
    public List<Transform> sharks = new List<Transform>();

    public GameObject fishPrefab;
    FishFlock[] fishFlocks;
    Vector3[] localFlockTargets;
    
    private void Start()
    {
        fishFlocks = new FishFlock[flockCount];
        localFlockTargets = new Vector3[flockCount];

        for (int i = 0; i < flockCount; i++) 
        {
            fishFlocks[i] = CreateFlocks(fishPrefab);
            localFlockTargets[i] = Vector3.zero;
        }

        StartCoroutine(SearchFlockTargets());
        StartCoroutine(FlockMove());
    }

    FishFlock CreateFlocks(GameObject fishPrefab)
    {
        var go = new GameObject("flock");
        go.transform.parent = transform;
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        
        var flock = go.AddComponent<FishFlock>();
        flock.fishPrefab = fishPrefab; 

        var pos = UnityEngine.Random.onUnitSphere * spawnRadius;
        pos.y = Mathf.Clamp(pos.y, WorldManager.fishHeight, WorldManager.fishHeight - Water.depth);
        flock.localPosition = pos;
        flock.sharkTrans = sharks;
        flock.CreateFishes(fishNumber, spawnRadius);

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
                pos.y = Mathf.Clamp(pos.y, WorldManager.fishHeight, WorldManager.fishHeight - Water.depth);
                localFlockTargets[i] = pos;
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
                fishFlocks[i].localPosition = Vector3.SmoothDamp(fishFlocks[i].localPosition, localFlockTargets[i], ref v, 0.5f);
                //Debug.Log(fishFlocks[i].flockPosition);
            }
        }
    }
}
