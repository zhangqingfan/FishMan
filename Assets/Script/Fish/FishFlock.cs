using System.Collections;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;
using System;
using Unity.Burst;
using System.Collections.Generic;

public struct Fish
{
    public Vector3 localPosition;
    public Quaternion localRotation;
    public float speed;
    public Vector3 localTarget;

    public float nextUpdateTime;
    public float passTime;
}

[BurstCompile]
public struct JobFish : IJobParallelFor
{
    public int spawnRadius;
    public Vector3 localFlockPosition;
    public float deltaTime;
    public NativeArray<Fish> fishArray;
    public NativeArray<Vector3> sharks;

    public float dangerRadius;
    public float minSpeed;
    public float maxSpeed;

    public void Execute(int index)
    {
        Move(index);
        Update(index);
    }

    void Move(int index)
    {
        var fish = fishArray[index];

        fish.localPosition += (fish.localRotation * Vector3.forward).normalized * fish.speed * deltaTime;
        fish.localPosition.y = fish.localPosition.y > WorldManager.fishHeight ? WorldManager.fishHeight : fish.localPosition.y;
        //Debug.Log(fish.position.y);
        var direction = Quaternion.LookRotation((fish.localTarget - fish.localPosition).normalized);
        fish.localRotation = Quaternion.RotateTowards(fish.localRotation, direction, 0.5f);

        fishArray[index] = fish;
    }

    void Update(int index)
    {
        var fish = fishArray[index];

        Vector3 escape = Vector3.zero;
        foreach (var shark in sharks)
        {
            var dir = fish.localPosition - shark;
            var weight = dir.magnitude - dangerRadius;
            weight = weight >= 0 ? 0 : -weight;
            escape += 2 * weight * dir.normalized;
        }

        if (escape != Vector3.zero)
        {
            fish.localTarget = fish.localPosition + escape;
            var speed = escape.magnitude / dangerRadius;
            var factor = Mathf.Clamp01((float)speed);
            fish.speed = minSpeed + (maxSpeed - minSpeed) * factor;
            fishArray[index] = fish;
            return;
        }

        fish.passTime += deltaTime;
        if (fish.passTime < fish.nextUpdateTime)
        {
            fishArray[index] = fish;
            return;
        }

        var random = new Unity.Mathematics.Random((uint)(math.cos(index * 50) * 100) + (uint)index);
        if (random.NextFloat() < 6.0f)
        {
            var pos = new Vector3(2 * random.NextFloat() - 1f, 2 * random.NextFloat() - 1f, 2 * random.NextFloat() - 1f).normalized;
            pos *= spawnRadius;
            fish.localTarget = pos + localFlockPosition;
            fish.speed = random.NextFloat(minSpeed, maxSpeed);
        }

        fish.passTime = 0;
        fish.nextUpdateTime = random.NextFloat(1.0f, 5.0f);
        fishArray[index] = fish;
    }
}

public class FishFlock : MonoBehaviour
{
    public Vector3 localPosition;
    public GameObject fishPrefab;
    public List<Transform> sharkTrans;
    
    JobFish jobFish;

    private void OnDestroy()
    {
        Dispose();
    }

    public void CreateFishes(int number, int spawnRadius)
    {
        Dispose();

        jobFish = new JobFish
        {
            fishArray = new NativeArray<Fish>(number, Allocator.Persistent),
            spawnRadius = spawnRadius,
            localFlockPosition = localPosition,
            dangerRadius = 15f,
            minSpeed = 2f,
            maxSpeed = 8f,
            sharks = new NativeArray<Vector3>(sharkTrans.Count, Allocator.Persistent),
        };

        for (int i = 0; i < number; i++) 
        {
            var pos = UnityEngine.Random.onUnitSphere * spawnRadius;
            //Debug.Log(pos);
            var fish = jobFish.fishArray[i];

            fish.localPosition = pos + localPosition;
            fish.localPosition.y = Mathf.Clamp(fish.localPosition.y, WorldManager.fishHeight, WorldManager.fishHeight - 15f);
            fish.localRotation = Quaternion.identity;
            fish.speed = jobFish.minSpeed;
            fish.nextUpdateTime = 0;
            fish.passTime = 0;

            jobFish.fishArray[i] = fish;
        }

        StartCoroutine(Render());
    }

    void Update()
    {
        for(int i = 0; i < sharkTrans.Count; i++) 
        {
            //bug...这里必须要修改！！！转成局部坐标！！
            jobFish.sharks[i] = sharkTrans[i].position;
            //Debug.Log(jobFish.sharks[i]);
        }

        jobFish.deltaTime = Time.deltaTime;
        jobFish.localFlockPosition = this.localPosition;
        JobHandle jobHandle = jobFish.Schedule(jobFish.fishArray.Length, 64 * 2);
        jobHandle.Complete();
    }

    IEnumerator Render()
    {
        var fishMesh = fishPrefab.GetComponent<MeshFilter>().sharedMesh;
        var renderParams = new RenderParams(fishPrefab.GetComponent<Renderer>().sharedMaterial);
        var fishInstanceMatrixs = new Matrix4x4[jobFish.fishArray.Length];
        var Scale = new Vector3(1.4f, 1.4f, 1.4f);
        Debug.Log("GPU Instancing state: " + fishPrefab.GetComponent<Renderer>().sharedMaterial.enableInstancing);

        while (true)
        { 
            yield return null;

            for (int i = 0; i < jobFish.fishArray.Length; i++)
            {
                var fish = jobFish.fishArray[i];
                var T = gameObject.transform.TransformPoint(fish.localPosition);
                var R = gameObject.transform.rotation * fish.localRotation;
                var S = Scale;
                fishInstanceMatrixs[i] = Matrix4x4.TRS(T, R, S);
            }

            Graphics.RenderMeshInstanced(renderParams, fishMesh, 0, fishInstanceMatrixs);
        }
    }

    void Dispose()
    {
        if(jobFish.fishArray != null) {  jobFish.fishArray.Dispose(); }
        if(jobFish.sharks != null) { jobFish.sharks.Dispose(); }
    }
}
