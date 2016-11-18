// ProceduralMath.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "ProceduralMath.h"


// This is an example of an exported variable
//PROCEDURALMATH_API int nProceduralMath=0;

inline void Clamp01(float& x)
{
	if (x > 1) x = 1;
	if (x < 0) x = 0;
}


inline float SmoothStep(float edge0, float edge1, float& x)
{
	var tmp = (x - edge0) / (edge1 - edge0);
	Clamp01(tmp);
	return tmp * tmp * (3.0f - 2.0f * tmp);
}


// This is an example of an exported function.
PROCEDURALMATH_API double GetHeight(MathInstance* instance, double x, double y, double z, int detailLevel)
{
	var initialPos = vec3((float)x, (float)y, (float)z);
	var pos = initialPos;

	int octaves = 2;
	double freq = 10;
	double ampModifier = .05f;
	double freqModifier = 15;
	double result = instance->radius;
	double amp = instance->radiusVariation;
	pos *= freq;

	
	for (int i = 0; i < octaves; i++)
	{
		result += instance->perlin.Get3D(pos) * amp;
		pos *= freqModifier;
		amp *= ampModifier;
	}

	{
		// hill tops
		var p = instance->perlin.Get3D(initialPos * 10.0f);
		if (p > 0) result -= p * instance->radiusVariation * 2;
	}


	{
		// craters
		var p = instance->worley.GetAt(initialPos * 2.0f, 1);
		result += SmoothStep(0.0f, 0.1f, p[0]) * instance->radiusVariation * 2;
	}

	return result;
}

// This is the constructor of a class that has been exported.
// see ProceduralMath.h for the class definition
/*CProceduralMath::CProceduralMath()
{
    return;
}*/
