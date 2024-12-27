using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class FishFlock : MonoBehaviour
{
    public int number;
    public int spawnRadius = 30;
    public float spawnHeightScale = 0.5f;

    public Vector3 flockPosition;
   
    NativeArray<Vector3> fishPositions;  //暂时没用到太多！！！
    NativeArray<Vector3> fishTargets;
    NativeArray<float> fishSpeeds;   
    Transform[] fishTransforms;
    
    private void OnDestroy()
    {
        Dispose();
    }

    public void CreateFishes()
    {
        Dispose();

        fishPositions = new NativeArray<Vector3>(number, Allocator.Persistent);
        fishTargets = new NativeArray<Vector3>(number, Allocator.Persistent);
        fishSpeeds = new NativeArray<float>(number, Allocator.Persistent);
        fishTransforms = new Transform[number];

        for(int i = 0; i < number; i++) 
        {
            var pos = Random.onUnitSphere * spawnRadius;
            pos.y *= spawnHeightScale;
            fishPositions[i] = pos;

            var fish = GameObject.CreatePrimitive(PrimitiveType.Cube);
            fish.transform.localRotation = Random.rotation;
            fish.transform.parent = this.gameObject.transform;
            fish.transform.position = fishPositions[i] + flockPosition;
            fishTransforms[i] = fish.transform;
        }

        StartCoroutine(SearchTargets());
        StartCoroutine(ChangeSpeed());
        StartCoroutine(FishMove());
    }

    IEnumerator SearchTargets()
    {
        while(true)
        {
            yield return new WaitForSeconds(Random.Range(1.0f, 5.0f));

            for(int i = 0; i < number; i++) 
            {
                if (Random.value > 0.6)
                    continue;

                var pos = Random.onUnitSphere * spawnRadius;
                pos.y *= spawnHeightScale;
                fishTargets[i] = pos + flockPosition;
            }
        }
    }

    IEnumerator ChangeSpeed()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(1.0f, 5.0f));

            for (int i = 0; i < number; i++)
            {
                if (Random.value > 0.6)
                    continue;

                fishSpeeds[i] = Random.Range(2f, 9f);
            }
        }
    }

    IEnumerator FishMove() 
    {
        while(true)
        {
            yield return null;

            for (int i = 0; i < number; i++)
            {
                var fishTrans = fishTransforms[i];
                fishTrans.position += fishTrans.forward * Time.deltaTime * fishSpeeds[i];

                var direction = Quaternion.LookRotation((fishTargets[i] - fishTrans.position).normalized);
                fishTrans.rotation = Quaternion.RotateTowards(fishTrans.rotation, direction, 0.5f);
            }
        }
    }

    void Dispose()
    {
        if(fishPositions != null) { fishPositions.Dispose(); }
        if(fishTargets != null) { fishTargets.Dispose();}
        if(fishSpeeds != null) { fishSpeeds.Dispose();}
    }
}
