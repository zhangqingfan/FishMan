using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public partial class Water
{
    public float GetHeight(Vector3 worldPos)
    {
        var grid = FindGrid(worldPos);
        if (grid == null)
        {
            Debug.Log("Can not find proper grid, bug!!!");
            return 0;
        }

        var localPos = grid.surface.transform.InverseTransformPoint(worldPos);
        var newPos = SamplePosition(localPos, Time.time); 
        var newWorldPos = grid.surface.transform.TransformPoint(newPos);
        Debug.Log(newWorldPos);
        return newWorldPos.y;
    }

    Vector3 SamplePosition(Vector3 pos, float time)
    {
        var waveData = setting.GetWaveData();
        var dataCount = waveData.Length;

        float epsilon = 0.0001f;
        if (Mathf.Abs(pos.x) >= length / 2 - epsilon || Mathf.Abs(pos.z) >= length / 2 - epsilon)
            return pos;

        float3 newPos = pos;
        for (int i = 0; i < dataCount; i++)
        {
            float amplitude = waveData[i].x;
            float length = waveData[i].y;
            float speed = waveData[i].z;
            float x = Mathf.Cos(waveData[i].w);
            float z = Mathf.Sin(waveData[i].w);
            Vector3 direction = new float3(x, 0, z);

            float k = 2.0f * 3.14f / length;
            float omega = speed * k;
            direction = direction.normalized;

            float phase = Vector3.Dot(pos, direction) * k - omega * time;
            float displacement = amplitude * Mathf.Sin(phase);

            newPos.y += displacement;
        }
        //return newPos;
        Debug.Log(newPos);


        float3 newPos_new = pos;
        for (int i = 0; i < setting.input.Count; i++)
        {
            float amplitude = setting.input[i].amplitude;
            float length = setting.input[i].length;
            float speed = setting.input[i].speed;
            float x = Mathf.Cos(setting.input[i].angle);
            float z = Mathf.Sin(setting.input[i].angle);
            Vector3 direction = new Vector3(x, 0, z);

            float k = (float)(2.0 * 3.14f / length);
            float omega = speed * k;
            direction.Normalize();

            float phase = Vector3.Dot(pos, direction) * k - omega * time;
            float displacement = amplitude * Mathf.Sin(phase);

            newPos_new.y += displacement;
        }
        Debug.Log(newPos_new);

        return newPos;

    }
}
