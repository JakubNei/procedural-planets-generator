

[include internal/include.perlinNoise.glsl]
[include internal/include.worleyNoise.glsl]

uniform double param_radiusMin;
uniform sampler2D param_baseHeightMap;
uniform double param_baseHeightMapMultiplier;
uniform double param_noiseMultiplier;


vec3 calestialToSpherical(vec3 c /*calestial*/)
{
	float r = length(c);
	if (r == 0) return vec3(0);
	return vec3(
		atan(c.z, c.x) / (2 * M_PI) + 0.5,
		asin(c.y / r) / M_PI + 0.5,
		r
	);
}
