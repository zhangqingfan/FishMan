#ifndef GERSTNER_WAVE 
#define GERSTNER_WAVE  

#include "WaterRT.cginc"

int dataCount;
half4 waveData[20];

struct Wave
{
    float3 pos;
    float3 normal;
};

float3 SamplePosition(float3 pos, float time)
{   
    //return pos;
    
    
    float epsilon = 0.001;
    if (abs(pos.x) >= _GridLength / 2 - epsilon || abs(pos.z) >= _GridLength / 2 - epsilon)
        return pos;
   
    float3 newPos = pos;
    
    for (int i = 0; i < dataCount; i++)
    {
        float amplitude = waveData[i].x;
        float length = waveData[i].y;
        float speed = waveData[i].z;
        float x = cos(waveData[i].w);
        float z = sin(waveData[i].w);               
        float3 direction = float3(x, 0, z);
        
        float k = 2.0 * 3.14 / length;
        float omega = speed * k;
        direction = normalize(direction);
        
        float phase = dot(newPos, direction) * k - omega * time;
        float displacement = amplitude * sin(phase);

        newPos.y += displacement;
        newPos.xz += amplitude * direction.xz * cos(phase);
    }
    
    return newPos;
}

float3 CalculateNormal(float3 pos, float time)
{
    float epsilon = 0.001;
    if (abs(pos.x) >= _GridLength / 2 - epsilon || abs(pos.z) >= _GridLength / 2 - epsilon)
        return float3(0, 1, 0);
    
    float3 newPos = SamplePosition(pos, time);
    
    epsilon = 0.05f;
    float3 posX = float3(pos.x + epsilon, 0, pos.z);
    float3 posZ = float3(pos.x, 0, pos.z + epsilon);

    float3 newPosX = SamplePosition(posX, time);
    float3 newPosZ = SamplePosition(posZ, time);

    float3 tangentX = newPosX - newPos;
    float3 tangentZ = newPosZ - newPos;

    float3 normal = cross(tangentX, tangentZ);
    normal.y = abs(normal.y); //unity axis is different from right-hand axis!!!
    
    return normalize(normal);
}

Wave SampleWave(float3 pos, float time)
{
    Wave wave;
    wave.pos = SamplePosition(pos, time);
    wave.normal = CalculateNormal(pos, time);
    return wave;
}

float3 ComputeWorldSpacePosition_1(float2 positionNDC, float deviceDepth, float4x4 invViewProjMatrix)
{
    float4 positionCS = float4(positionNDC * 2.0 - 1.0, deviceDepth, 1.0);

    #if UNITY_UV_STARTS_AT_TOP
    // Our world space, view space, screen space and NDC space are Y-up.
    // Our clip space is flipped upside-down due to poor legacy Unity design.
    // The flip is baked into the projection matrix, so we only have to flip
    // manually when going from CS to NDC and back.
    positionCS.y = -positionCS.y;
    #endif
   
    float4 hpositionWS = mul(invViewProjMatrix, positionCS);
    return hpositionWS.xyz / hpositionWS.w;
}

#endif  