[include internal/include.perlinNoise.glsl]
[include internal/include.worleyNoise.glsl]
#line 4

uniform double param_radiusMin;
uniform sampler2D param_baseHeightMap;
uniform double param_baseHeightMapMultiplier;
uniform double param_noiseMultiplier;



uniform sampler2D param_biomesControlMap;



// water
uniform sampler2D 	param_biome1r_diffuseMap;
uniform sampler2D 	param_biome1r_normalMap;
uniform vec3 		param_biome1r_color;

uniform sampler2D 	param_biome1g_diffuseMap;
uniform sampler2D 	param_biome1g_normalMap;
uniform vec3 		param_biome1g_color;

uniform sampler2D 	param_biome1b_diffuseMap;
uniform sampler2D 	param_biome1b_normalMap;
uniform vec3 		param_biome1b_color;

uniform sampler2D 	param_biome1a_diffuseMap;
uniform sampler2D 	param_biome1a_normalMap;
uniform vec3 		param_biome1a_color;



uniform sampler2D 	param_biome2r_diffuseMap;
uniform sampler2D 	param_biome2r_normalMap;
uniform vec3 		param_biome2r_color;

uniform sampler2D 	param_biome2g_diffuseMap;
uniform sampler2D 	param_biome2g_normalMap;
uniform vec3 		param_biome2g_color;

uniform sampler2D 	param_biome2b_diffuseMap;
uniform sampler2D 	param_biome2b_normalMap;
uniform vec3 		param_biome2b_color;

uniform sampler2D 	param_biome2a_diffuseMap;
uniform sampler2D 	param_biome2a_normalMap;
uniform vec3 		param_biome2a_color;



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

vec3 sphericalToCalestial(vec2 c /*spherical*/)
{
	// denormalize from 0..1
	c.x = (c.x - 0.5) * (2 * M_PI);
	c.y = (c.y - 0.5) * M_PI;

	// calculate
	vec3 p = vec3(	
		cos(c.y) * cos(c.x),
		sin(c.y),
		cos(c.y) * sin(c.x)
	);

	return p;
}








// vec2 worley(vec3 P, float jitter, bool manhattanDistance)
// float perlinNoise(vec3 p)

float GetProceduralHeight(vec3 dirFromPlanetCenter)
{
	float result = 0;
	
	{ // base noise
		float freq = 10;
		vec3 pos = dirFromPlanetCenter * freq;
		int octaves = 10;
		float ampModifier = 0.15;
		float freqModifier = 5;
		float amp = 1;
		for (int i = 0; i < octaves; i++)
		{
			result += perlinNoise(pos) * amp;
			pos *= freqModifier;
			amp *= ampModifier;
		}
	}
	
	/*
	{ // hill tops
		float p = perlinNoise(dirFromPlanetCenter * 10);
		if(p > 0) result -= p * 2;
	}
	*/

	/*
	{ // craters

		vec2 w = worleyNoise(dirFromPlanetCenter*10, 1, false);
		result += smoothstep(0.0, 0.4, w.x) * 100;
	}
	*/

	return result;

}

float HideTextureSamplingNoise(vec3 dirFromPlanetCenter)
{	
	float result = 0;
	float freq = 100;
	vec3 pos = dirFromPlanetCenter * freq;
	int octaves = 2;
	float ampModifier = 1/5;
	float freqModifier = 5;
	float amp = 1;
	for (int i = 0; i < octaves; i++)
	{
		result += perlinNoise(pos) * amp;
		pos *= freqModifier;
		amp *= ampModifier;
	}
	return result;
}

double GetProceduralAndBaseHeightMapHeight(dvec3 direction, vec2 uv)
{
	double height = 0;	

	if(param_baseHeightMapMultiplier > 0) {
		double h = texture2D(param_baseHeightMap, uv).r;
		height += param_baseHeightMapMultiplier * h * (0.5 + 0.5 * HideTextureSamplingNoise(vec3(direction))); 
	}
	if(param_noiseMultiplier > 0)			
		height += param_noiseMultiplier * GetProceduralHeight(vec3(direction * param_radiusMin / 1000000));

	return height;
}


double GetProceduralAndBaseHeightMapHeight(vec2 uv)
{
	vec3 direction = sphericalToCalestial(uv);
	return GetProceduralAndBaseHeightMapHeight(direction, uv);
}
double GetProceduralAndBaseHeightMapHeight01(vec2 uv)
{
	return GetProceduralAndBaseHeightMapHeight(uv) / param_baseHeightMapMultiplier;
}

float GetHumidity(vec2 uvCenter)
{
	const float waterHeight = 0.5;

	vec2 uv = uvCenter;
	if(GetProceduralAndBaseHeightMapHeight01(uv) < waterHeight) return 1;

	const float maxDistanceToWater = 0.05;
	const float distanceToWaterIncrease = 0.001;
	float distanceToWater = 0;	

	float splits = 3;
	while(distanceToWater < maxDistanceToWater) {

		float angleDelta = 2*M_PI / splits;
		for(float angle = 0; angle < 2*M_PI; angle += angleDelta)
		{
			uv = uvCenter + vec2(
				cos(angle) * distanceToWater,
				sin(angle) * distanceToWater
			);
			if(GetProceduralAndBaseHeightMapHeight01(uv) < waterHeight) return 1 - distanceToWater / maxDistanceToWater;
		}
		
		distanceToWater += distanceToWaterIncrease;
		splits += 1;
	}

	return 0;

}


vec3 GetProceduralAndBaseHeightMapNormal(vec2 uv, float eps) {
    vec3 normal;
    double z = GetProceduralAndBaseHeightMapHeight(uv); 
    normal.x = float(GetProceduralAndBaseHeightMapHeight(vec2(uv.x+eps,uv.y)) - z);
    normal.y = float(GetProceduralAndBaseHeightMapHeight(vec2(uv.x,uv.y+eps)) - z);
    normal.z = eps;
    return normalize(normal);
}