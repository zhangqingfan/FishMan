void Swing_float(float3 position, out float3 result)
{
	half t = _Time.x * 1;
    position += sin(t) * 1;
	result = position;
}

