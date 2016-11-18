#pragma once
#include "stdafx.h"

#define PERLIN_SAMPLE_SIZE 2048

using namespace glm;
using namespace std;

class Perlin
{
public:

  Perlin(int seed, int octaves=1, float freq=1.0f, float amp=1.0f, float persistance=2.0f);


  float Get2DComposeOctaves(vec2 pos);

  float Get1D(float& arg);
  float Get2D(vec2& pos);
  float Get3D(vec3& pos);
  

private:

  void Normalize2DVec(float v[2]);
  void Normalize3DVec(float v[3]);
  void Init(void);

  int   mOctaves;
  float mFrequency;
  float mAmplitude;
  int   mSeed;
  float	mPersistance;

  int p[PERLIN_SAMPLE_SIZE + PERLIN_SAMPLE_SIZE + 2];
  float g3[PERLIN_SAMPLE_SIZE + PERLIN_SAMPLE_SIZE + 2][3];
  float g2[PERLIN_SAMPLE_SIZE + PERLIN_SAMPLE_SIZE + 2][2];
  float g1[PERLIN_SAMPLE_SIZE + PERLIN_SAMPLE_SIZE + 2];
  bool  mStart;

};
