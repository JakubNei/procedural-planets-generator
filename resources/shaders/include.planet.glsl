

[include internal/include.perlinNoise.glsl]
[include internal/include.worleyNoise.glsl]



uniform double param_radiusMin;
uniform sampler2D param_baseHeightMap;
uniform double param_baseHeightMapMultiplier;
uniform double param_noiseMultiplier;


uniform sampler2D param_humidityMap;


uniform sampler2D param_biomesSplatMap0;

uniform sampler2D param_biome00_diffuseMap;
uniform sampler2D param_biome00_normalMap;

uniform sampler2D param_biome01_diffuseMap;
uniform sampler2D param_biome01_normalMap;

uniform sampler2D param_biome02_diffuseMap;
uniform sampler2D param_biome02_normalMap;

uniform sampler2D param_biome03_diffuseMap;
uniform sampler2D param_biome03_normalMap;




uniform sampler2D param_biomesSplatMap1;

uniform sampler2D param_biome10_diffuseMap;
uniform sampler2D param_biome10_normalMap;

uniform sampler2D param_biome11_diffuseMap;
uniform sampler2D param_biome11_normalMap;

uniform sampler2D param_biome12_diffuseMap;
uniform sampler2D param_biome12_normalMap;

uniform sampler2D param_biome13_diffuseMap;
uniform sampler2D param_biome13_normalMap;




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