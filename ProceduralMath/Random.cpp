#include "stdafx.h"

#include "Random.h"

//#include <chrono>

Random::Random() {	
	seed = 0; //(ulong)chrono::system_clock::now().time_since_epoch().count();
	SetSeed(seed);
}
Random::Random(ulong seed)
{
	SetSeed(seed);
}

int Random::GetNext() {
	return (int)generator();
}
int Random::GetNext(int min, int max) {
	return min + (generator() % (max - min));
}

Random* Random::Reset() {
	SetSeed(seed);
	return this;
}
Random* Random::SetSeed(ulong seed) {
	this->seed = seed;
	generator.seed(seed);
	return this;
}
