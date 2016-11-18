#pragma once

#include "stdafx.h"

namespace Math {


	template<typename T>
	inline T Max(T a, T b) {
		if (a>b) return a;
		return b;
	}
	template<typename T>
	inline T Max(T a, T b, T c) {
		return Max(Max(a, b), c);
	}
	template<typename T>
	inline T Max(T a, T b, T c, T d) {
		return Max(Max(Max(a, b), c), d);
	}

	template<typename T>
	inline T Min(T a, T b) {
		if (a<b) return a;
		return b;
	}
	template<typename T>
	inline T Min(T a, T b, T c) {
		return Min(Min(a, b), c);
	}
	template<typename T>
	inline T Min(T a, T b, T c, T d) {
		return Min(Min(Min(a, b), c), d);
	}

	inline dvec3 Vec3ToDVec3(vec3 a) {
		return dvec3(
			a.x,
			a.y,
			a.z
			);
	}
	inline vec3 DVec3ToVec3(dvec3 a) {
		return vec3(
			(float)a.x,
			(float)a.y,
			(float)a.z
			);
	}


	inline vec3 FloatToVec3(float f) {
		vec3 color;
		color.b = floor(f / 256.0f / 256.0f);
		color.g = floor((f - color.b * 256.0f * 256.0f) / 256.0f);
		color.r = floor(f - color.b * 256.0f * 256.0f - color.g * 256.0f);
		return color / 256.0f;
	}
	inline float Vec3ToFloat(vec3 color) {
		return color.r + color.g * 256.0f + color.b * 256.0f * 256.0f;
	}

	inline float Abs(float a) {
		return abs(a);
	}
	inline int Abs(int a) {
		return abs(a);
	}
	inline float Pow(float a, float power) {
		return pow(a, power);
	}
	inline float Pow(float a, int power) {
		return pow(a, power);
	}
	inline int Pow(int a, int power) {
		return (int)pow((float)a, power);
	}

	inline int Random() {
		return rand();
	}
	inline int Random(int min, int max) {
		//Debug::Log("\n\n%d\n\n",min+(rand()%(max-min)));
		return min + (Random() % (max - min));
	}

	inline float Sqrt(float a) {
		return sqrt(a);
	}

	inline float Distance(vec2& a, vec2& b) {
		return glm::distance(a, b);
	}
	inline float Distance(vec3& a, vec3& b) {
		return glm::distance(a, b);
	}
	inline float DistanceSqr(vec2& a, vec2& b) {
		float dx = a.x - b.x;
		float dy = a.y - b.y;
		return Abs(dx*dx + dy*dy);
	}
	inline float DistanceSqr(vec3& a, vec3& b) {
		float dx = a.x - b.x;
		float dy = a.y - b.y;
		float dz = a.z - b.z;
		return Abs(dx*dx + dy*dy + dz*dz);
	}



	inline float Floor(float val) {
		return floor(val);
	}

	inline int Clamp(int val, int min, int max) {
		if (val>max) val = max;
		if (val<min) val = min;
		return val;
	}
	inline float Clamp(float val, float min, float max) {
		if (val>max) val = max;
		if (val<min) val = min;
		return val;
	}
	inline double Clamp(double val, double min, double max) {
		if (val>max) val = max;
		if (val<min) val = min;
		return val;
	}
	inline vec3 Clamp(vec3 val, vec3& min, vec3& max) {
		val.x = Clamp(val.x, min.x, max.x);
		val.y = Clamp(val.y, min.y, max.y);
		val.z = Clamp(val.z, min.z, max.z);
		return val;
	}
	inline vec3 Clamp(vec3 val, float min, float max) {
		val.x = Clamp(val.x, min, max);
		val.y = Clamp(val.y, min, max);
		val.z = Clamp(val.z, min, max);
		return val;
	}
	inline dvec3 Clamp(dvec3 val, dvec3& min, dvec3& max) {
		val.x = Clamp(val.x, min.x, max.x);
		val.y = Clamp(val.y, min.y, max.y);
		val.z = Clamp(val.z, min.z, max.z);
		return val;
	}
	inline dvec3 Clamp(dvec3 val, double min, double max) {
		val.x = Clamp(val.x, min, max);
		val.y = Clamp(val.y, min, max);
		val.z = Clamp(val.z, min, max);
		return val;
	}


	inline float Lerp(float a, float b, float t) {
		return a + t*(b - a);
		//return a*(1-t)+b*t;		
	}
	inline vec3 Lerp(vec3& a, vec3& b, float t) {
		return vec3(
			a.x + t*(b.x - a.x),
			a.y + t*(b.y - a.y),
			a.z + t*(b.z - a.z)
			);
	}
	inline dvec3 Lerp(dvec3& a, dvec3& b, double t) {
		return dvec3(
			a.x + t*(b.x - a.x),
			a.y + t*(b.y - a.y),
			a.z + t*(b.z - a.z)
			);
	}




	inline float RadToDeg(float radians) {
		return radians * 180.0f / (float)M_PI;
	}
	inline float DegToRad(float degrees) {
		return degrees * (float)M_PI / 180.0f;
	}
	inline double RadToDeg(double radians) {
		return radians * 180.0f / (double)M_PI;
	}
	inline double DegToRad(double degrees) {
		return degrees * (double)M_PI / 180.0f;
	}

	/// <summary>
	/// Constant used in FNV hash function.
	/// FNV hash: http://isthe.com/chongo/tech/comp/fnv/#FNV-source
	/// </summary>
#define FNV_OFFSET_BASIS 2166136261
	/// <summary>
	/// Constant used in FNV hash function
	/// FNV hash: http://isthe.com/chongo/tech/comp/fnv/#FNV-source
	/// </summary>
#define FNV_PRIME 16777619
	/// <summary>
	/// Hashes three integers into a single integer using FNV hash.
	/// FNV hash: http://isthe.com/chongo/tech/comp/fnv/#FNV-source
	/// </summary>
	/// <returns>hash value</returns>
	inline uint FnvHash(uint x, uint y, uint z, uint w) { // derived by me
		return (uint)((((((((FNV_OFFSET_BASIS ^ (uint)x) * FNV_PRIME) ^ (uint)y) * FNV_PRIME) ^ (uint)z) * FNV_PRIME) ^ (uint)w) * FNV_PRIME);
	}
	inline uint FnvHash(uint x, uint y, uint z) { // original
		return (uint)((((((FNV_OFFSET_BASIS ^ (uint)x) * FNV_PRIME) ^ (uint)y) * FNV_PRIME) ^ (uint)z) * FNV_PRIME);
	}
	inline uint FnvHash(uint x, uint y) { // derived by me
		return (uint)((((FNV_OFFSET_BASIS ^ (uint)x) * FNV_PRIME) ^ (uint)y) * FNV_PRIME);
	}


	vec3 CubicBezier(float t, vec3 p0, vec3 p1, vec3 p2, vec3 p3);


	// stolen from http://nccastaff.bournemouth.ac.uk/jmacey/RobTheBloke/www/
	float CoxDeBoor(float u, int index, int k, const float* Knots);

	static float Pi = (float)M_PI;

}