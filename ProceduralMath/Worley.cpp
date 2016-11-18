#include "stdafx.h"

#include "Worley.h"
#include "Random.h"
#include "Math.h"



/// <summary>
/// Given a uniformly distributed random number this function returns the number of feature points in a given cube.
/// </summary>
/// <param name="value">a uniformly distributed random number</param>
/// <returns>The number of feature points in a cube.</returns>
// Generated using mathmatica with "AccountingForm[N[Table[CDF[PoissonDistribution[4], i], {i, 1, 9}], 20]*2^32]"
uint probLookup(uint value)
{
    if (value < 393325350) return 1;
    if (value < 1022645910) return 2;
    if (value < 1861739990) return 3;
    if (value < 2700834071) return 4;
    if (value < 3372109335) return 5;
    if (value < 3819626178) return 6;
    if (value < 4075350088) return 7;
    if (value < 4203212043) return 8;
    return 9;
}


/// <summary>
/// Inserts value into array using insertion sort. If the value is greater than the largest value in the array
/// it will not be added to the array.
/// </summary>
/// <param name="arr">The array to insert the value into.</param>
/// <param name="value">The value to insert into the array.</param>
inline void insert(float* arr, uint arrSize, float value)
{
    float temp;
	for (uint i = arrSize - 1; i >= 0; i--)
    {
        if (value > arr[i]) break;
        temp = arr[i];
        arr[i] = value;
		if (i + 1 < arrSize) arr[i + 1] = temp;
    }
}




inline float Distance(vec2& p1, vec2& p2, Worley::DistanceFunction distanceFunction) {
	if(distanceFunction == Worley::DistanceFunction::Euclidian) {
		return (p1.x - p2.x) * (p1.x - p2.x) + (p1.y - p2.y) * (p1.y - p2.y);
	}
	if(distanceFunction == Worley::DistanceFunction::Manhattan) {
		return Math::Abs(p1.x - p2.x) + Math::Abs(p1.y - p2.y);
	}
	if(distanceFunction == Worley::DistanceFunction::Chebyshev) {
		vec2 diff = p1 - p2;
        return Math::Max(Math::Abs(diff.x), Math::Abs(diff.y));
	}
	return 0.0f;
}



inline float Distance(vec3& p1, vec3& p2, Worley::DistanceFunction distanceFunction) {
	if (distanceFunction == Worley::DistanceFunction::Euclidian) {
		return (p1.x - p2.x) * (p1.x - p2.x) + (p1.y - p2.y) * (p1.y - p2.y) + (p1.z - p2.z) * (p1.z - p2.z);
	}
	if (distanceFunction == Worley::DistanceFunction::Manhattan) {
		return Math::Abs(p1.x - p2.x) + Math::Abs(p1.y - p2.y) + Math::Abs(p1.z - p2.z);
	}
	if (distanceFunction == Worley::DistanceFunction::Chebyshev) {
		vec3 diff = p1 - p2;
		return Math::Max(Math::Abs(diff.x), Math::Max(Math::Abs(diff.y), Math::Abs(diff.z)));
	}
	return 0.0f;
}




Worley::Worley(int seed, DistanceFunction distanceFunction)
{
	this->seed = seed;
	this->distanceFunction = distanceFunction;
}

Worley::~Worley() {
}

//Sample single octave of 2D noise
float* Worley::GetAt(vec2& input, uint returnArrayLen)
{
	//Declare some values for later use
	uint numberFeaturePoints;
	vec2 randomDiff, featurePoint;
	int cubeX, cubeY;

	float* distanceArray = new float[returnArrayLen];

	//Initialize values in distance array to large values
	for (uint i = 0; i < returnArrayLen; i++)
		distanceArray[i] = 6666;

	//1. Determine which cube the evaluation point is in
	int evalCubeX = (int)Math::Floor(input.x);
	int evalCubeY = (int)Math::Floor(input.y);

	for (int i = -1; i < 2; ++i)
	{
		for (int j = -1; j < 2; ++j)
		{
			cubeX = evalCubeX + i;
			cubeY = evalCubeY + j;

			//2. Generate a reproducible random number generator for the cube
			Random random(Math::FnvHash((uint)(cubeX), (uint)(cubeY), seed));
			
			//3. Determine how many feature points are in the cube
			numberFeaturePoints = probLookup(random.GetNext());
			//4. Randomly place the feature points in the cube
			for (uint l = 0; l < numberFeaturePoints; ++l)
			{
				randomDiff.x = (float)random.GetNext() / 0x100000000;
				randomDiff.y = (float)random.GetNext() / 0x100000000;
				featurePoint = vec2(randomDiff.x + (float)cubeX, randomDiff.y + (float)cubeY);

				//5. Find the feature point closest to the evaluation point. 
				//This is done by inserting the distances to the feature points into a sorted list
				insert(distanceArray, returnArrayLen, Distance(input, featurePoint, distanceFunction));
			}
			//6. Check the neighboring cubes to ensure their are no closer evaluation points.
			// This is done by repeating steps 1 through 5 above for each neighboring cube
		}
	}

	return distanceArray;
}
	


//Sample single octave of 2D noise
float* Worley::GetAt(vec3& input, uint returnArrayLen)
{
	//Declare some values for later use
	uint numberFeaturePoints;
	vec3 randomDiff, featurePoint;
	int cubeX, cubeY, cubeZ;

	float* distanceArray = new float[returnArrayLen];

	//Initialize values in distance array to large values
	for (uint i = 0; i < returnArrayLen; i++)
		distanceArray[i] = 6666;

	//1. Determine which cube the evaluation point is in
	int evalCubeX = (int)Math::Floor(input.x);
	int evalCubeY = (int)Math::Floor(input.y);
	int evalCubeZ = (int)Math::Floor(input.z);

	for (int i = -1; i < 2; ++i)
	{
		for (int j = -1; j < 2; ++j)
		{
			for (int k = -1; k < 2; ++k) {

				cubeX = evalCubeX + i;
				cubeY = evalCubeY + j;
				cubeZ = evalCubeZ + k;

				//2. Generate a reproducible random number generator for the cube
				Random random(Math::FnvHash((uint)(cubeX), (uint)(cubeY), (uint)(cubeZ), seed));

				//3. Determine how many feature points are in the cube
				numberFeaturePoints = probLookup(random.GetNext());
				//4. Randomly place the feature points in the cube
				for (uint l = 0; l < numberFeaturePoints; ++l)
				{
					randomDiff.x = (float)random.GetNext() / 0x100000000;
					randomDiff.y = (float)random.GetNext() / 0x100000000;
					randomDiff.z = (float)random.GetNext() / 0x100000000;
					featurePoint = vec3(
						randomDiff.x + (float)cubeX,
						randomDiff.y + (float)cubeY,
						randomDiff.z + (float)cubeZ
					);

					//5. Find the feature point closest to the evaluation point. 
					//This is done by inserting the distances to the feature points into a sorted list
					insert(distanceArray, returnArrayLen, Distance(input, featurePoint, distanceFunction));
				}
				//6. Check the neighboring cubes to ensure their are no closer evaluation points.
				// This is done by repeating steps 1 through 5 above for each neighboring cube
			}
		}
	}

	return distanceArray;
}