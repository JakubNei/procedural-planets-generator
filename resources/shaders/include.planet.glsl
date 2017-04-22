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

// rock
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



// https://gamedev.stackexchange.com/questions/116205/terracing-mountain-features
float terrace(float h, float bandHeight) {
    float W = bandHeight; // width of terracing bands
    float k = floor(h / W);
    float f = (h - k*W) / W;
    float s = min(2 * f, 1.0);
    return (k+s) * W;
}


float perlinNoise(vec3 pos, int octaves, float modifier)
{
	float result = 0;
	float amp = 1;
	for (int i = 0; i < octaves; i++)
	{
		result += perlinNoise(pos) * amp;
		pos *= modifier;
		amp /= modifier;
	}
	return result;
}


// vec2 worley(vec3 P, float jitter, bool manhattanDistance)
// float perlinNoise(vec3 p)

float GetProceduralHeight01(vec3 dir)
{
	float result = 0;

	vec2 w;
	float x;

	/*
	{ // terraces
		vec3 pos = dir * 10;
		int octaves = 2;
		float freqModifier = 3;
		float ampModifier = 1/freqModifier;
		float amp = 1;
		for (int i = 0; i < octaves; i++)
		{
			float p = perlinNoise(pos, 4, 10);
			result += terrace(p, 0.5) * amp;
			pos *= freqModifier;
			amp *= ampModifier;
		}
	}
	*/
	// small noise

	

	{ //big detail
		//continents
		result += abs(perlinNoise(dir*0.5, 5, 4));
		w = worleyNoise(dir*2);
		result += (w.x - w.y) * 2;
		//oceans
		result -= abs(perlinNoise(dir*2.2, 4, 4));
		//big rivers
		x = perlinNoise(dir * 3, 3, 2);
 		result += -exp(-pow(x*55,2)) * 0.2;
 		//craters
		w = worleyNoise(dir);
		result += smoothstep(0.0, 0.1, w.x);
	}
	

	{ //small detail
		float p = perlinNoise(dir*10, 5, 10) * 100;
		result += terrace(p, 0.3)*0.005;
		result += p*0.005;
		//small rivers
		float x = perlinNoise(dir * 3);
 		//result += -exp(-pow(x*55,2)); 
	}


	{
		float p = perlinNoise(dir*10, 5, 10);
		//result += terrace(p, 0.15)*10;
		//result += p * 0.1;
	}

	{
		//float p = perlinNoise(dir*10, 5, 10);
		//result += terrace(p, 0.1)/1;
	}



	/*
	{ // hill tops
		float p = perlinNoise(dir * 10);
		if(p > 0) result -= p * 2;
	}
	*/

	/*
	{ // craters

		vec2 w = worleyNoise(dir*10, 1, false);
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

double GetProceduralHeight(dvec3 direction)
{
	return param_noiseMultiplier * GetProceduralHeight01(vec3(direction * param_radiusMin / 1000000));
}


double GetProceduralHeight(vec2 uv)
{
	vec3 direction = sphericalToCalestial(uv);
	return GetProceduralHeight(direction);
}
double GetProceduralHeight01(vec2 uv)
{
	return GetProceduralHeight(uv) / (param_baseHeightMapMultiplier + param_noiseMultiplier);
}

float GetHumidity(vec2 uvCenter)
{
	const float waterHeight = 0.5;

	vec2 uv = uvCenter;
	if(GetProceduralHeight01(uv) < waterHeight) return 1;

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
			if(GetProceduralHeight01(uv) < waterHeight) return 1 - distanceToWater / maxDistanceToWater;
		}
		
		distanceToWater += distanceToWaterIncrease;
		splits += 1;
	}

	return 0;

}

/*
vec3 GetProceduralAndBaseHeightMapNormal(vec2 uv, float eps) {
    vec3 normal;
    double z = GetProceduralHeight(uv); 
    normal.x = float(GetProceduralHeight(vec2(uv.x+eps,uv.y)) - z);
    normal.y = float(GetProceduralHeight(vec2(uv.x,uv.y+eps)) - z);
    normal.z = eps;
    return normalize(normal);
}
*/

vec3 GetProceduralAndBaseHeightMapNormal(vec2 spherical, float eps) {
    vec3 normal;
    double z = GetProceduralHeight(spherical); 
    normal.x = float(
    	(GetProceduralHeight(vec2(spherical.x+eps,spherical.y)) - z)
    	-(GetProceduralHeight(vec2(spherical.x-eps,spherical.y)) - z)
    ) / 2;
    normal.y = float(
    	(GetProceduralHeight(vec2(spherical.x,spherical.y+eps)) - z)
    	-(GetProceduralHeight(vec2(spherical.x,spherical.y-eps)) - z)
    ) / 2;
    normal.z = eps;
    return normalize(normal);
}



vec3 GetProceduralAndBaseHeightMapNormal(vec3 direction, vec3 normal, vec3 tangent, float eps) {


	vec3 N = normal;
	vec3 T = tangent;
	vec3 T2 = T - N * dot(N, T); // Gram-Schmidt orthogonalization of T
	vec3 B = normalize(cross(N,T2));

	vec3 result;
    double z = GetProceduralHeight(direction); 
    result.x = float(
    	(GetProceduralHeight(direction-T*eps) - z)
    	-(GetProceduralHeight(direction+T*eps) - z)
    ) / 2;
    result.y = float(
    	(GetProceduralHeight(direction-B*eps) - z)
    	-(GetProceduralHeight(direction+B*eps) - z)
    ) / 2;
    result.z = eps;
    return normalize(result);
}