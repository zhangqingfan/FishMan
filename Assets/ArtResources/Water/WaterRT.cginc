#ifndef WATER_RT
#define WATER_RT  


half4 GridWorldPosArray[9];
float _GridLength;

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


float GridHalfLength()
{
    return _GridLength * 0.5;
}

int FindSelfGridIndex(float3 worldPos)
{
    for (int i = 0; i < 9; i++)
    {
        half disX = abs(worldPos.x - GridWorldPosArray[i].x);
        half disZ = abs(worldPos.z - GridWorldPosArray[i].z);
        
        if (disX <= GridHalfLength() && disZ <= GridHalfLength())
            return i;
    }
    return -1;
}

float SampleTrackRT(int gridIndex, float3 localPos)
{
    float2 uv = ((localPos.xz / GridHalfLength()) + 1) * 0.5;        
    float result = 0;

    if(gridIndex == 0)
        result = tex2Dlod(_RT0, float4(uv, 0, 0)).y;
    else if (gridIndex == 1)
        result = tex2Dlod(_RT1, float4(uv, 0, 0)).y;
    else if (gridIndex == 2)
        result = tex2Dlod(_RT2, float4(uv, 0, 0)).y;
    else if (gridIndex == 3)
        result = tex2Dlod(_RT3, float4(uv, 0, 0)).y;
    else if (gridIndex == 4)
        result = tex2Dlod(_RT4, float4(uv, 0, 0)).y;
    else if (gridIndex == 5)
        result = tex2Dlod(_RT5, float4(uv, 0, 0)).y;
    else if (gridIndex == 6)
        result = tex2Dlod(_RT6, float4(uv, 0, 0)).y;
    else if (gridIndex == 7)
        result = tex2Dlod(_RT7, float4(uv, 0, 0)).y;
    else if (gridIndex == 8)
        result = tex2Dlod(_RT8, float4(uv, 0, 0)).y;
    
    return result * 1;
}

//todo...这里要调整一下！
float3 CalculateTrackRTNormal(int gridIndex, float3 localPos)
{
    float epsilon = 2 * _GridLength / 512;
    if (abs(localPos.x) >= _GridLength / 2 - epsilon || abs(localPos.z) >= _GridLength / 2 - epsilon)
        return float3(0, 1, 0);
    
    float3 offsetX = localPos + float3(epsilon, 0, 0);
    float3 offsetZ = localPos + float3(0, 0, epsilon);
    
    offsetX.y += SampleTrackRT(gridIndex, offsetX);
    offsetZ.y += SampleTrackRT(gridIndex, offsetZ);
    
    if (offsetX.y == offsetZ.y && offsetX.y == localPos.y)
        return float3(0, 0, 0);
    
    float3 tangentX = offsetX - localPos;
    float3 tangentZ = offsetZ - localPos;
        
    float3 normal = cross(tangentX, tangentZ);
    normal.y = abs(normal.y);
    return normalize(normal);
}

#endif  