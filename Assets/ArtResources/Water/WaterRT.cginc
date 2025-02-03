#ifndef WATER_RT
#define WATER_RT  

half4 GridWorldPosArray[9];
float gridLength;

int FindSelfGridIndex(float3 worldPos)
{
    for (int i = 0; i < 9; i++)
    {
        half disX = abs(worldPos.x - GridWorldPosArray[i].x);
        half disZ = abs(worldPos.z - GridWorldPosArray[i].z);
        
        float epsilon = 0.0001;
        if (disX <= gridLength / 2 + epsilon && disZ <= gridLength / 2 + epsilon)
            return i;
    }
    return -1;
}

//todo...

#endif  