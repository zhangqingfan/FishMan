using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public partial class Water
{
    public float GetHeight(Vector3 pos)
    {
        var newPos = SamplePosition(pos, Time.time);
        return newPos.y;
    }

    Vector3 SamplePosition(Vector3 pos, float time)
    {
        float3 newPos = pos;

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

            newPos.y += displacement;
        }
        return newPos;
    }
}
