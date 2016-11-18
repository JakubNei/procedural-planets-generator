#pragma once

#include "stdafx.h"

using namespace std;

class Random
{
public:

	Random();
	Random(ulong seed);
	int GetNext();
	int GetNext(int min, int max);	

	Random* Reset();
	Random* SetSeed(ulong seed);

private:
	ulong seed;
	mt19937 generator;
};

