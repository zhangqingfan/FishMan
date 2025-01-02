using System.Collections;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;
using System;
using Unity.Burst;
using System.Collections.Generic;
using static UnityEditor.PlayerSettings;

public class FishFlock : MonoBehaviour
{
    public int number;
    public int spawnRadius = 30;
    public float spawnHeightScale = 0.5f;

    public Vector3 flockPosition;
    public GameObject fishPrefab;
    public List<Transform> sharkTrans;
    
    public struct Fish
    {
        public Vector3 position;
        public Vector3 scale;
        public Quaternion rotation;
        public float speed;
        public Vector3 target;

        public float nextUpdateTime;
        public float passTime;
    }

    [BurstCompile]
    public struct JobFish : IJobParallelFor
    {
        public int spawnRadius;
        public float spawnHeightScale;
        public Vector3 flockPosition;
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

            fish.position += (fish.rotation * Vector3.forward).normalized * fish.speed * deltaTime;
            fish.position.y = fish.position.y > WorldManager.height ? WorldManager.height : fish.position.y;
            //Debug.Log(fish.position.y);
            var direction = Quaternion.LookRotation((fish.target - fish.position).normalized);
            fish.rotation = Quaternion.RotateTowards(fish.rotation, direction, 0.5f);

            fishArray[index] = fish;
        }

        void Update(int index)
        {
            var fish = fishArray[index];

            Vector3 escape = Vector3.zero;
            foreach (var shark in sharks)
            {
                var dir = fish.position - shark;
                var weight = dir.magnitude - dangerRadius;
                weight = weight >= 0 ? 0 : -weight;
                escape += 2 * weight * dir.normalized;
            }
            
            if(escape != Vector3.zero)
            {
                fish.target = fish.position + escape;
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

            var random = new Unity.Mathematics.Random((uint)DateTime.Now.Ticks + (uint)index);
            if (random.NextFloat() < 6.0f)
            {
                var pos = new Vector3(2 * random.NextFloat() - 1f, 2 * random.NextFloat() - 1f, 2 * random.NextFloat() - 1f).normalized;
                pos *= spawnRadius;
                pos.y *= spawnHeightScale;
                fish.target = pos + flockPosition;
                fish.speed = random.NextFloat(minSpeed, maxSpeed);
            }

            fish.passTime = 0;
            fish.nextUpdateTime = random.NextFloat(1.0f, 5.0f);
            fishArray[index] = fish;
        }
    }

    JobFish jobFish;

    private void OnDestroy()
    {
        Dispose();
    }

    public void CreateFishes()
    {
        Dispose();

        jobFish = new JobFish
        {
            fishArray = new NativeArray<Fish>(number, Allocator.Persistent),
            spawnRadius = spawnRadius,
            spawnHeightScale = spawnHeightScale,
            flockPosition = flockPosition,
            dangerRadius = 20f,
            minSpeed = 2f,
            maxSpeed = 10f,
            sharks = new NativeArray<Vector3>(sharkTrans.Count, Allocator.Persistent),
        };

        for (int i = 0; i < number; i++) 
        {
            var pos = UnityEngine.Random.onUnitSphere * spawnRadius;
            //Debug.Log(pos);
            pos.y *= spawnHeightScale;

            var fish = jobFish.fishArray[i];

            fish.position = pos + flockPosition;
            fish.position.y = fish.position.y > WorldManager.height ? WorldManager.height : fish.position.y;
            fish.rotation = Quaternion.identity;
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
            jobFish.sharks[i] = sharkTrans[i].position;
            //Debug.Log(jobFish.sharks[i]);
        }

        jobFish.deltaTime = Time.deltaTime;
        jobFish.flockPosition = this.flockPosition;
        JobHandle jobHandle = jobFish.Schedule(jobFish.fishArray.Length, 64 * 2);
        jobHandle.Complete();
    }

    IEnumerator Render()
    {
        var fishMesh = fishPrefab.GetComponent<MeshFilter>().sharedMesh;
        var renderParams = new RenderParams(fishPrefab.GetComponent<Renderer>().sharedMaterial);
        var fishInstanceMatrixs = new Matrix4x4[jobFish.fishArray.Length];
        var scale = new Vector3(1, 1, 1);

        Debug.Log("GPU Instancing state: " + fishPrefab.GetComponent<Renderer>().sharedMaterial.enableInstancing);

        while (true)
        { 
            yield return null;

            for (int i = 0; i < jobFish.fishArray.Length; i++)
            {
                var fish = jobFish.fishArray[i];
                fishInstanceMatrixs[i] = Matrix4x4.TRS(fish.position, fish.rotation, scale);
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
