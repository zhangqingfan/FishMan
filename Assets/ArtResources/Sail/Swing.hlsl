void Swing_float(float3 position, float strength, out float3 result)
{
    float offset = position.x + position.y + position.z;
    offset = offset / 3;

    half t = _Time.x * _SwingSpeed;
    position.z += sin(t + offset * 5)  * strength * 0.2;
	result = position;
}

void Strength_float(float2 uv, out float strength)
{
    float2 dist = uv - float2(0.5, 0.5);
    float length = sqrt(dist.x * dist.x + dist.y * dist.y);
    length = length / 0.7;
    strength = saturate(1 - length);
}