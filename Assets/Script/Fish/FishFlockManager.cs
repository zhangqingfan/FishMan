using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishFlockManager : MonoBehaviour
{
    [Range(1, 100)]
    public int flockCount;

    [Range(1, 1000)]
    public int fishNumber = 5;

    [Range(10, 100)]
    public int spawnRadius = 30;

    [Range(0.1f, 1.5f)]
    public float spawnHeightScale = 0.5f;

    public GameObject fishPrefab;

    private void Start()
    {
        for(int i = 0; i < flockCount; i++) 
        {
            CreateFlocks();
        }
    }

    void CreateFlocks()
    {
        var go = new GameObject();
        go.transform.parent = transform;
        
        var flock = go.AddComponent<FishFlock>();
        flock.number = fishNumber;
        flock.spawnRadius = spawnRadius;
        flock.spawnHeightScale = spawnHeightScale;
        flock.CreateFishes();
    }

    void FishMove1()
    {
        var v3 = Vector3.zero;
        for (int i = 0; i < fishTransforms.Length; i++)
        {
            fishTransforms[i].localPosition = Vector3.SmoothDamp(fishTransforms[i].position, targets[i], ref v3, 0.5f);
        }
    }
}
