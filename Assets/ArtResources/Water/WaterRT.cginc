#ifndef WATER_RT
#define WATER_RT  

half4 GridWorldPosArray[9];
float _GridLength;
float _TrackRTScale;

sampler2D _RT0;
float4 _RT0_ST;

sampler2D _RT1;
float4 _RT1_ST;

sampler2D _RT2;
float4 _RT2_ST;

sampler2D _RT3;
float4 _RT3_ST;

sampler2D _RT4;
float4 _RT4_ST;

sampler2D _RT5;
float4 _RT5_ST;

sampler2D _RT6;
float4 _RT6_ST;

sampler2D _RT7;
float4 _RT7_ST;

sampler2D _RT8;
float4 _RT8_ST;


int FindSelfGridIndex(float3 worldPos)
{
    for (int i = 0; i < 9; i++)
    {
        half disX = abs(worldPos.x - GridWorldPosArray[i].x);
        half disZ = abs(worldPos.z - GridWorldPosArray[i].z);
        
        float epsilon = 0.001;
        float halfLength = _GridLength * _TrackRTScale * 0.5;
        if (disX <= halfLength + epsilon && disZ <= halfLength + epsilon)
            return i;
    }
    return -1;
}

sampler2D GetTrackRenderTexture(int i)
{
    if(i == 0)
        return _RT0;
    
    else if (i == 1)
        return _RT1;
    
    else if (i == 2)
        return _RT2;
    
    else if (i == 3)
        return _RT3;
    
    else if (i == 4)
        return _RT4;
    
    else if (i == 5)
        return _RT5;
    
    else if (i == 6)
        return _RT6;
    
    else if (i == 7)
        return _RT7;
    
    else if (i == 8)
        return _RT8;
}

float3 SampleTrackRT(float3 worldPos, float3 localPos)
{
    float halfLength = _GridLength * 0.5;
    float2 uv = ((localPos / (halfLength * _TrackRTScale)) + 1) * 0.5;
    
    int index = FindSelfGridIndex(worldPos);
    sampler2D trackRT = GetTrackRenderTexture(index);
    
    return float3(0, tex2D(trackRT, uv).y, 0);
}

float3 CalculateTrackRTNormal(float3 pos)
{
    
    
    
    return float3(0, 0, 0);
}

#endif  