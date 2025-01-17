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

float3 SampleNormal(float3 originalPos, float3 newPos, float time)
{
    float3 dx = SamplePosition((originalPos + float3(0.2, 0, 0)), time) - newPos;
    float3 dy = SamplePosition((originalPos + float3(0, 0, 0.2)), time) - newPos;
    return normalize(cross(dx, dy));
}

Wave SampleWave(float3 pos, float time)
{
    Wave wave;
    wave.pos = SamplePosition(pos, time);
    wave.normal = SampleNormal(pos, wave.pos, time);
    return wave;
}

#endif  