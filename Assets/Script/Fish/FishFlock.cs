using System.Collections;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;
using System;

public class FishFlock : MonoBehaviour
{
    public int number;
    public int spawnRadius = 30;
    public float spawnHeightScale = 0.5f;

    public Vector3 flockPosition;
    public GameObject fishPrefab;
    
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

    public struct JobFish : IJobParallelFor
    {
        public int spawnRadius;
        public float spawnHeightScale;
        public Vector3 flockPosition;
        public float deltaTime;
        public Unity.Mathematics.Random random;
        public NativeArray<Fish> fishArray;

        public void Execute(int index)
        {
            Move(index);
            Update(index);
        }

        void Move(int index)
        {
            var fish = fishArray[index];

            fish.position += (fish.rotation * Vector3.forward).normalized * fish.speed * deltaTime;
            var direction = Quaternion.LookRotation((fish.target - fish.position).normalized);
            fish.rotation = Quaternion.RotateTowards(fish.rotation, direction, 0.5f);

            fishArray[index] = fish;
        }

        void Update(int index)
        {
            var fish = fishArray[index];

            fish.passTime += deltaTime;
            if (fish.passTime < fish.nextUpdateTime)
                return;

            if (random.NextFloat() < 60.6)
            {
                var f3 = random.NextFloat3() * spawnRadius;
                var pos = new Vector3(f3.x, f3.y, f3.z);
                pos.y *= spawnHeightScale;
                fish.target = pos + flockPosition;
                fish.speed = random.NextFloat(2, 10);
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

        jobFish = new JobFish();
        jobFish.fishArray = new NativeArray<Fish>(number, Allocator.Persistent);
        jobFish.spawnRadius = spawnRadius;
        jobFish.spawnHeightScale = spawnHeightScale;
        jobFish.flockPosition = flockPosition;
        jobFish.random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, 1000));

        for (int i = 0; i < number; i++) 
        {
            var pos = UnityEngine.Random.onUnitSphere * spawnRadius;
            pos.y *= spawnHeightScale;

            var fish = jobFish.fishArray[i];

            fish.position = pos + flockPosition;
            fish.rotation = Quaternion.identity;
            fish.nextUpdateTime = 0;
            fish.passTime = 0;

            jobFish.fishArray[i] = fish;
        }

        StartCoroutine(Render());
    }

    void Update()
    {
        jobFish.deltaTime = Time.deltaTime;
        jobFish.flockPosition = this.flockPosition;
        JobHandle jobHandle = jobFish.Schedule(jobFish.fishArray.Length, 64);
        jobHandle.Complete();
    }

    IEnumerator Render()
    {
        var fishMesh = fishPrefab.GetComponent<MeshFilter>().sharedMesh;
        var renderParams = new RenderParams(fishPrefab.GetComponent<Renderer>().sharedMaterial);
        var fishInstanceMatrixs = new Matrix4x4[jobFish.fishArray.Length];
        var scale = new Vector3(1, 1, 1);
        fishPrefab.GetComponent<Renderer>().sharedMaterial.enableInstancing = true;

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
    }
}
