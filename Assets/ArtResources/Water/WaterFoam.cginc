#ifndef WATER_FOAM  
#define WATER_FOAM

sampler2D _FoamMask;
float4 _FoamMask_ST;

float FoamRemap(float origFrom, float origTo, float targetFrom, float targetTo, float value)
{
    return lerp(targetFrom, targetTo, (value - origFrom) / (origTo - origFrom));
}

float3 GetFoamAlbedo(float2 uv, float coverage)
{
    uv = uv * 0.1;
    float foamTime = 1 - saturate(coverage);
    float4 foamMasks = tex2D(_FoamMask, uv);
	
    float microDistanceField = foamMasks.r;
    float temporalNoise = foamMasks.g;
    float foamNoise = foamMasks.b;
    float macroDistanceField = foamMasks.a;

    foamTime = saturate(foamTime);
    foamTime = pow(foamTime, 4.0);

	// Time offsets
    float microDistanceFieldInfluenceMin = 0.05;
    float microDistanceFieldInfluenceMax = 0.6;
    float MicroDistanceFieldInfluence = lerp(microDistanceFieldInfluenceMin, microDistanceFieldInfluenceMax, foamTime);
    foamTime += (2.0 * (1.0f - microDistanceField) - 1.0) * MicroDistanceFieldInfluence;

    float temporalNoiseInfluenceMin = 0.1;
    float temporalNoiseInfluenceMax = 0.2;
    float temporalNoiseInfluence = -lerp(temporalNoiseInfluenceMin, temporalNoiseInfluenceMax, foamTime);
    foamTime += (2.0 * temporalNoise - 1.0) * temporalNoiseInfluence;

    foamTime = saturate(foamTime);
    foamTime = FoamRemap(0.0, 1.0, 0.0, 2.2, foamTime); // easy way to make sure the erosion is over (there are many time offsets)
    foamTime = saturate(foamTime);

	// sharpness
    float sharpnessMin = 0.1;
    float sharpnessMax = 5.0;
    float sharpness = lerp(sharpnessMax, sharpnessMin, foamTime);
    sharpness = max(0.0f, sharpness);

    float alpha = FoamRemap(foamTime, 1.0f, 0.0f, 1.0f, macroDistanceField);
    alpha *= sharpness;
    alpha = saturate(alpha);

	// detail in alpha
    float distanceFieldInAlpha = lerp(macroDistanceField, microDistanceField, 0.5f) * 0.45f;
    distanceFieldInAlpha = 1.0f - distanceFieldInAlpha;
    float noiseInAlpha = pow(foamNoise, 0.3f);

	// fade
    float fadeOverTime = pow(1.0 - foamTime, 2);

    float3 albedo = alpha * distanceFieldInAlpha * noiseInAlpha * fadeOverTime /* + foamBubble.r * 0.8 * data.coverage.x*/;

    return albedo;
}
#endif