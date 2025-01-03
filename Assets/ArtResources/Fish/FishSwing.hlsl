void FishSwing_float(float3 position, out float3 result)
{
    float offset = position.x + position.y + position.z;
    offset = offset / 3;

    half t = _Time.x * _Speed;
    position.x += sin(t + offset * 8) * 0.1;
	result = position;
}
