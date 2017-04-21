[include internal/include.doubleTrigonometry.glsl]
[include internal/include.perlinNoise.glsl]
[include internal/include.worleyNoise.glsl]










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
dvec3 calestialToSpherical(dvec3 c /*calestial*/)
{
	double r = length(c);
	if (r == 0) return dvec3(0);

	// calculate
	dvec3 p = dvec3(
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
dvec3 sphericalToCalestial(dvec3 c /*spherical*/)
{
	// denormalize from 0..1
	c.x = (c.x - 0.5) * (2 * M_PI);
	c.y = (c.y - 0.5) * M_PI;

	// calculate
	dvec3 p = dvec3(	
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
dvec3 sphericalToCalestial(dvec2 c /*spherical*/)
{
	// denormalize from 0..1
	c.x = (c.x - 0.5) * (2 * M_PI);
	c.y = (c.y - 0.5) * M_PI;

	// calculate
	dvec3 p = dvec3(	
		cos(c.y) * cos(c.x),
		sin(c.y),
		cos(c.y) * sin(c.x)
	);

	return p;
}







// https://gamedev.stackexchange.com/questions/116205/terracing-mountain-features
double terrace(double h, double bandHeight) {
    double W = bandHeight; // width of terracing bands
    double k = floor(h / W);
    double f = (h - k*W) / W;
    double s = min(2 * f, 1.0);
    return (k+s) * W;
}


double perlinNoise(dvec3 pos, int octaves, double modifier)
{
	double result = 0;
	double amp = 1;
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

double GetProceduralHeight(dvec3 dir)
{
	double result = 0;

	dvec2 w;
	double x;

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

	#define SQR(X) ((X)*(X))
	

	{ //big detail
		//continents
		result += abs(perlinNoise(dir*0.5, 5, 4));
		w = worleyNoise(dir*2);
		result += (w.x - w.y) * 2;
		//oceans
		result -= abs(perlinNoise(dir*2.2, 4, 4));
		//big rivers
		x = perlinNoise(dir * 3, 3, 2);
 		//result += -exp(-SQR(x*55)) * 0.2;
 		//craters
		w = worleyNoise(dir);
		result += smoothstep(0.0, 0.1, w.x);
	}
	

	{ //small detail
		double p = perlinNoise(dir*10, 5, 10) * 100;
		result += terrace(p, 0.3)*0.005;
		result += p*0.001;
		//small rivers
		double x = perlinNoise(dir * 3);
 		//result += -exp(-pow(x*55,2)); 
	}


	{
		double p = perlinNoise(dir*10, 5, 10);
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



double GetProceduralAndBaseHeightMapHeight(dvec3 direction, dvec2 uv)
{
	double height = 0;	
	
	height += param_noiseMultiplier * GetProceduralHeight(direction * param_radiusMin / 1000000);

	return height;
}



double GetProceduralAndBaseHeightMapHeight(dvec2 uv)
{
	dvec3 direction = sphericalToCalestial(uv);
	return GetProceduralAndBaseHeightMapHeight(direction, uv);
}
double GetProceduralAndBaseHeightMapHeight01(dvec2 uv)
{
	return GetProceduralAndBaseHeightMapHeight(uv) / (param_baseHeightMapMultiplier + param_noiseMultiplier);
}

float GetHumidity(dvec2 uvCenter)
{
	const double waterHeight = 0.5;

	dvec2 uv = uvCenter;
	if(GetProceduralAndBaseHeightMapHeight01(uv) < waterHeight) return 1;






	const float maxDistanceToWater = 0.05;
	const float distanceToWaterIncrease = 0.001;
	float distanceToWater = 0;	

	float splits = 3;
	while(distanceToWater < maxDistanceToWater) {

		float angleDelta = 2*M_PI / splits;
		for(float angle = 0; angle < 2*M_PI; angle += angleDelta)
		{
			uv = uvCenter + dvec2(
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


vec3 GetProceduralAndBaseHeightMapNormal(dvec2 spherical, double eps) {
    dvec3 normal;
    double z = GetProceduralAndBaseHeightMapHeight(spherical); 
    normal.x = float(
    	(GetProceduralAndBaseHeightMapHeight(dvec2(spherical.x+eps,spherical.y)) - z)
    	-(GetProceduralAndBaseHeightMapHeight(dvec2(spherical.x-eps,spherical.y)) - z)
    ) / 2;
    normal.y = float(
    	(GetProceduralAndBaseHeightMapHeight(dvec2(spherical.x,spherical.y+eps)) - z)
    	-(GetProceduralAndBaseHeightMapHeight(dvec2(spherical.x,spherical.y-eps)) - z)
    ) / 2;
    normal.z = eps;
    return vec3(normalize(normal));
}