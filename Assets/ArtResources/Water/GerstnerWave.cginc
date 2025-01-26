#ifndef GERSTNER_WAVE 
#define GERSTNER_WAVE  

int dataCount;
half4 waveData[20];

struct Wave
{
    float3 pos;
    float3 normal;
};

float3 SamplePosition(float3 pos, float time)
{
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
        
        float phase = dot(pos, direction) * k - omega * time;
        float displacement = amplitude * sin(phase);

        newPos.y += displacement;
    }
    return newPos;
}

float3 GetOffsetPos(float radians, float3 originalPos, float time)
{
    float radius = 0.1;
    float x = originalPos.x + radius * cos(radians);
    float z = originalPos.z + radius * sin(radians);
    float3 offsetPos = originalPos + float3(x, 0, z);
    return SamplePosition(offsetPos, time);
}

float3 SampleNormal(float3 originalPos, float3 newPos, float time)
{
    float deltaAngle = 90;
    float3 normal = 0;
    
    for (int i = 0; i < 360 / deltaAngle - 1; i++)
    {
        float radians0 = i * deltaAngle * 3.14159 / 180.0;
        float3 dx = GetOffsetPos(radians0, originalPos, time) - newPos;
        
        float radians1 = (i + 1) * deltaAngle * 3.14159 / 180.0;
        float3 dy = GetOffsetPos(radians1, originalPos, time) - newPos;
        
        float3 crossN = cross(dx, dy);
        crossN.y = abs(crossN.y); //unity axis is different from right-hand axis!!!
        normal += crossN;
    }
    
    return normalize(normal);
}

Wave SampleWave(float3 pos, float time)
{
    Wave wave;
    wave.pos = SamplePosition(pos, time);
    wave.normal = SampleNormal(pos, wave.pos, time);
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