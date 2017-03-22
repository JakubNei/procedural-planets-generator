

[include internal/include.perlinNoise.glsl]
[include internal/include.worleyNoise.glsl]

uniform double param_radiusMin;
uniform sampler2D param_baseHeightMap;
uniform sampler2D param_biomesSplatMap;
uniform double param_baseHeightMapMultiplier;
uniform double param_noiseMultiplier;


uniform vec3 param_biome0_biomesSplatMapColor;
uniform sampler2D param_biome0_diffuseMap;
uniform sampler2D param_biome0_normalMap;

uniform vec3 param_biome1_biomesSplatMapColor;
uniform sampler2D param_biome1_diffuseMap;
uniform sampler2D param_biome1_normalMap;

uniform vec3 param_biome2_biomesSplatMapColor;
uniform sampler2D param_biome2_diffuseMap;
uniform sampler2D param_biome2_normalMap;

uniform vec3 param_biome3_biomesSplatMapColor;
uniform sampler2D param_biome3_diffuseMap;
uniform sampler2D param_biome3_normalMap;

uniform vec3 param_biome4_biomesSplatMapColor;
uniform sampler2D param_biome4_diffuseMap;
uniform sampler2D param_biome4_normalMap;

uniform vec3 param_biome5_biomesSplatMapColor;
uniform sampler2D param_biome5_diffuseMap;
uniform sampler2D param_biome5_normalMap;

uniform vec3 param_biome6_biomesSplatMapColor;
uniform sampler2D param_biome6_diffuseMap;
uniform sampler2D param_biome6_normalMap;

uniform vec3 param_biome7_biomesSplatMapColor;
uniform sampler2D param_biome7_diffuseMap;
uniform sampler2D param_biome7_normalMap;

uniform vec3 param_biome8_biomesSplatMapColor;
uniform sampler2D param_biome8_diffuseMap;
uniform sampler2D param_biome8_normalMap;

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