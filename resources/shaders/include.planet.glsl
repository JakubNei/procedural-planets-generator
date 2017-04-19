[include internal/include.perlinNoise.glsl]
[include internal/include.worleyNoise.glsl]



uniform double param_radiusMin;
uniform sampler2D param_baseHeightMap;
uniform double param_baseHeightMapMultiplier;
uniform double param_noiseMultiplier;



uniform sampler2D param_biomesControlMap;



// water
uniform sampler2D param_biome1r_diffuseMap;
uniform sampler2D param_biome1r_normalMap;

uniform sampler2D param_biome1g_diffuseMap;
uniform sampler2D param_biome1g_normalMap;

uniform sampler2D param_biome1b_diffuseMap;
uniform sampler2D param_biome1b_normalMap;

uniform sampler2D param_biome1a_diffuseMap;
uniform sampler2D param_biome1a_normalMap;



uniform sampler2D param_biome2r_diffuseMap;
uniform sampler2D param_biome2r_normalMap;

uniform sampler2D param_biome2g_diffuseMap;
uniform sampler2D param_biome2g_normalMap;

uniform sampler2D param_biome2b_diffuseMap;
uniform sampler2D param_biome2b_normalMap;

uniform sampler2D param_biome2a_diffuseMap;
uniform sampler2D param_biome2a_normalMap;



#define M_PI 3.1415926535897932384626433832795



vec3 calestialToSpherical(vec3 c /*calestial*/)
{
	float r = length(c);
	if (r == 0) return vec3(0);

	// calculate
	vec3 p = vec3(
		atan(c.z, c.x),  // longitude = x
		asin(c.y / r), // latitude = y
		r // altitude = z
	);

	// normalize to 0..1 range
	p.x = p.x / (2 * M_PI) + 0.5;;
	p.y = p.y / M_PI + 0.5;

	return p;
}

vec3 sphericalToCalestial(vec3 c /*spherical*/)
{
	// denormalize from 0..1
	c.x = (c.x - 0.5) * (2 * M_PI);
	c.y = (c.y - 0.5) * M_PI;

	// calculate
	vec3 p = vec3(	
		cos(c.y) * cos(c.x) * c.z,
		sin(c.y) * c.z,
		cos(c.y) * sin(c.x) * c.z
	);

	return p;
}