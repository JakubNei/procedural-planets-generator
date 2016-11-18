#pragma once

#include "stdafx.h"

using namespace glm;
using namespace std;

class Worley
{
public:
	enum DistanceFunction {
		Euclidian,
		Manhattan,
		Chebyshev
	};
	Worley(int seed, DistanceFunction distanceFunction);
	~Worley();
	float* GetAt(vec2& input, uint returnArrayLen = 3);
	float* GetAt(vec3& input, uint returnArrayLen = 3);

private:
	int seed;
	DistanceFunction distanceFunction;
	vector<vec2*> points;
};

