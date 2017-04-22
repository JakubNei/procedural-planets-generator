// PERLIN NOISE








// taken from: https://github.com/ashima/webgl-noise/blob/master/src/noise3D.glsl

//
// Description : Array and textureless GLSL 2D/3D/4D simplex 
//               noise functions.
//      Author : Ian McEwan, Ashima Arts.
//  Maintainer : stegu
//     Lastmod : 20110822 (ijm)
//     License : Copyright (C) 2011 Ashima Arts. All rights reserved.
//               Distributed under the MIT License. See LICENSE file.
//               https://github.com/ashima/webgl-noise
//               https://github.com/stegu/webgl-noise









vec2 perlinNoise_mod289(vec2 x) {
  return x - floor(x * (1.0 / 289.0)) * 289.0;
}
vec3 perlinNoise_mod289(vec3 x) {
  return x - floor(x * (1.0 / 289.0)) * 289.0;
}
vec4 perlinNoise_mod289(vec4 x) {
  return x - floor(x * (1.0 / 289.0)) * 289.0;
}
vec3 perlinNoise_permute(vec3 x) {
  return perlinNoise_mod289(((x*34.0)+1.0)*x);
}
vec4 perlinNoise_permute(vec4 x) {
     return perlinNoise_mod289(((x*34.0)+1.0)*x);
}
vec4 perlinNoise_taylorInvSqrt(vec4 r)
{
  return 1.79284291400159 - 0.85373472095314 * r;
}
float perlinNoise(vec3 v)
{ 
  const vec2  C = vec2(1.0/6.0, 1.0/3.0) ;
  const vec4  D = vec4(0.0, 0.5, 1.0, 2.0);

// First corner
  vec3 i  = floor(v + dot(v, C.yyy) );
  vec3 x0 =   v - i + dot(i, C.xxx) ;

// Other corners
  vec3 g = step(x0.yzx, x0.xyz);
  vec3 l = 1.0 - g;
  vec3 i1 = min( g.xyz, l.zxy );
  vec3 i2 = max( g.xyz, l.zxy );

  //   x0 = x0 - 0.0 + 0.0 * C.xxx;
  //   x1 = x0 - i1  + 1.0 * C.xxx;
  //   x2 = x0 - i2  + 2.0 * C.xxx;
  //   x3 = x0 - 1.0 + 3.0 * C.xxx;
  vec3 x1 = x0 - i1 + C.xxx;
  vec3 x2 = x0 - i2 + C.yyy; // 2.0*C.x = 1/3 = C.y
  vec3 x3 = x0 - D.yyy;      // -1.0+3.0*C.x = -0.5 = -D.y

// Permutations
  i = perlinNoise_mod289(i); 
  vec4 p = perlinNoise_permute( perlinNoise_permute( perlinNoise_permute( 
             i.z + vec4(0.0, i1.z, i2.z, 1.0 ))
           + i.y + vec4(0.0, i1.y, i2.y, 1.0 )) 
           + i.x + vec4(0.0, i1.x, i2.x, 1.0 ));

// Gradients: 7x7 points over a square, mapped onto an octahedron.
// The ring size 17*17 = 289 is close to a multiple of 49 (49*6 = 294)
  float n_ = 0.142857142857; // 1.0/7.0
  vec3  ns = n_ * D.wyz - D.xzx;

  vec4 j = p - 49.0 * floor(p * ns.z * ns.z);  //  mod(p,7*7)

  vec4 x_ = floor(j * ns.z);
  vec4 y_ = floor(j - 7.0 * x_ );    // mod(j,N)

  vec4 x = x_ *ns.x + ns.yyyy;
  vec4 y = y_ *ns.x + ns.yyyy;
  vec4 h = 1.0 - abs(x) - abs(y);

  vec4 b0 = vec4( x.xy, y.xy );
  vec4 b1 = vec4( x.zw, y.zw );

  //vec4 s0 = vec4(lessThan(b0,0.0))*2.0 - 1.0;
  //vec4 s1 = vec4(lessThan(b1,0.0))*2.0 - 1.0;
  vec4 s0 = floor(b0)*2.0 + 1.0;
  vec4 s1 = floor(b1)*2.0 + 1.0;
  vec4 sh = -step(h, vec4(0.0));

  vec4 a0 = b0.xzyw + s0.xzyw*sh.xxyy ;
  vec4 a1 = b1.xzyw + s1.xzyw*sh.zzww ;

  vec3 p0 = vec3(a0.xy,h.x);
  vec3 p1 = vec3(a0.zw,h.y);
  vec3 p2 = vec3(a1.xy,h.z);
  vec3 p3 = vec3(a1.zw,h.w);

//Normalise gradients
  vec4 norm = perlinNoise_taylorInvSqrt(vec4(dot(p0,p0), dot(p1,p1), dot(p2, p2), dot(p3,p3)));
  p0 *= norm.x;
  p1 *= norm.y;
  p2 *= norm.z;
  p3 *= norm.w;

// Mix final noise value
  vec4 m = max(0.6 - vec4(dot(x0,x0), dot(x1,x1), dot(x2,x2), dot(x3,x3)), 0.0);
  m = m * m;
  return 42.0 * dot( m*m, vec4( dot(p0,x0), dot(p1,x1), 
                                dot(p2,x2), dot(p3,x3) ) );
  }


float perlinNoise(vec2 v)
{
  const vec4 C = vec4(0.211324865405187,  // (3.0-sqrt(3.0))/6.0
                      0.366025403784439,  // 0.5*(sqrt(3.0)-1.0)
                     -0.577350269189626,  // -1.0 + 2.0 * C.x
                      0.024390243902439); // 1.0 / 41.0
// First corner
  vec2 i  = floor(v + dot(v, C.yy) );
  vec2 x0 = v -   i + dot(i, C.xx);

// Other corners
  vec2 i1;
  //i1.x = step( x0.y, x0.x ); // x0.x > x0.y ? 1.0 : 0.0
  //i1.y = 1.0 - i1.x;
  i1 = (x0.x > x0.y) ? vec2(1.0, 0.0) : vec2(0.0, 1.0);
  // x0 = x0 - 0.0 + 0.0 * C.xx ;
  // x1 = x0 - i1 + 1.0 * C.xx ;
  // x2 = x0 - 1.0 + 2.0 * C.xx ;
  vec4 x12 = x0.xyxy + C.xxzz;
  x12.xy -= i1;

// Permutations
  i = perlinNoise_mod289(i); // Avoid truncation effects in permutation
  vec3 p = perlinNoise_permute( perlinNoise_permute( i.y + vec3(0.0, i1.y, 1.0 ))
    + i.x + vec3(0.0, i1.x, 1.0 ));

  vec3 m = max(0.5 - vec3(dot(x0,x0), dot(x12.xy,x12.xy), dot(x12.zw,x12.zw)), 0.0);
  m = m*m ;
  m = m*m ;

// Gradients: 41 points uniformly over a line, mapped onto a diamond.
// The ring size 17*17 = 289 is close to a multiple of 41 (41*7 = 287)

  vec3 x = 2.0 * fract(p * C.www) - 1.0;
  vec3 h = abs(x) - 0.5;
  vec3 ox = floor(x + 0.5);
  vec3 a0 = x - ox;

// Normalise gradients implicitly by scaling m
// Approximation of: m *= inversesqrt( a0*a0 + h*h );
  m *= 1.79284291400159 - 0.85373472095314 * ( a0*a0 + h*h );

// Compute final noise value at P
  vec3 g;
  g.x  = a0.x  * x0.x  + h.x  * x0.y;
  g.yz = a0.yz * x12.xz + h.yz * x12.yw;
  return 130.0 * dot(m, g);
}





































dvec2 perlinNoise_mod289(dvec2 x) {
  return x - floor(x * (1.0 / 289.0)) * 289.0;
}
dvec3 perlinNoise_mod289(dvec3 x) {
  return x - floor(x * (1.0 / 289.0)) * 289.0;
}
dvec4 perlinNoise_mod289(dvec4 x) {
  return x - floor(x * (1.0 / 289.0)) * 289.0;
}
dvec3 perlinNoise_permute(dvec3 x) {
  return perlinNoise_mod289(((x*34.0)+1.0)*x);
}
dvec4 perlinNoise_permute(dvec4 x) {
     return perlinNoise_mod289(((x*34.0)+1.0)*x);
}
dvec4 perlinNoise_taylorInvSqrt(dvec4 r)
{
  return 1.79284291400159 - 0.85373472095314 * r;
}
double perlinNoise(dvec3 v)
{ 
  const dvec2  C = dvec2(1.0/6.0, 1.0/3.0) ;
  const dvec4  D = dvec4(0.0, 0.5, 1.0, 2.0);

// First corner
  dvec3 i  = floor(v + dot(v, C.yyy) );
  dvec3 x0 =   v - i + dot(i, C.xxx) ;

// Other corners
  dvec3 g = step(x0.yzx, x0.xyz);
  dvec3 l = 1.0 - g;
  dvec3 i1 = min( g.xyz, l.zxy );
  dvec3 i2 = max( g.xyz, l.zxy );

  //   x0 = x0 - 0.0 + 0.0 * C.xxx;
  //   x1 = x0 - i1  + 1.0 * C.xxx;
  //   x2 = x0 - i2  + 2.0 * C.xxx;
  //   x3 = x0 - 1.0 + 3.0 * C.xxx;
  dvec3 x1 = x0 - i1 + C.xxx;
  dvec3 x2 = x0 - i2 + C.yyy; // 2.0*C.x = 1/3 = C.y
  dvec3 x3 = x0 - D.yyy;      // -1.0+3.0*C.x = -0.5 = -D.y

// Permutations
  i = perlinNoise_mod289(i); 
  dvec4 p = perlinNoise_permute( perlinNoise_permute( perlinNoise_permute( 
             i.z + dvec4(0.0, i1.z, i2.z, 1.0 ))
           + i.y + dvec4(0.0, i1.y, i2.y, 1.0 )) 
           + i.x + dvec4(0.0, i1.x, i2.x, 1.0 ));

// Gradients: 7x7 points over a square, mapped onto an octahedron.
// The ring size 17*17 = 289 is close to a multiple of 49 (49*6 = 294)
  double n_ = 0.142857142857; // 1.0/7.0
  dvec3  ns = n_ * D.wyz - D.xzx;

  dvec4 j = p - 49.0 * floor(p * ns.z * ns.z);  //  mod(p,7*7)

  dvec4 x_ = floor(j * ns.z);
  dvec4 y_ = floor(j - 7.0 * x_ );    // mod(j,N)

  dvec4 x = x_ *ns.x + ns.yyyy;
  dvec4 y = y_ *ns.x + ns.yyyy;
  dvec4 h = 1.0 - abs(x) - abs(y);

  dvec4 b0 = dvec4( x.xy, y.xy );
  dvec4 b1 = dvec4( x.zw, y.zw );

  //dvec4 s0 = dvec4(lessThan(b0,0.0))*2.0 - 1.0;
  //dvec4 s1 = dvec4(lessThan(b1,0.0))*2.0 - 1.0;
  dvec4 s0 = floor(b0)*2.0 + 1.0;
  dvec4 s1 = floor(b1)*2.0 + 1.0;
  dvec4 sh = -step(h, dvec4(0.0));

  dvec4 a0 = b0.xzyw + s0.xzyw*sh.xxyy ;
  dvec4 a1 = b1.xzyw + s1.xzyw*sh.zzww ;

  dvec3 p0 = dvec3(a0.xy,h.x);
  dvec3 p1 = dvec3(a0.zw,h.y);
  dvec3 p2 = dvec3(a1.xy,h.z);
  dvec3 p3 = dvec3(a1.zw,h.w);

//Normalise gradients
  dvec4 norm = perlinNoise_taylorInvSqrt(dvec4(dot(p0,p0), dot(p1,p1), dot(p2, p2), dot(p3,p3)));
  p0 *= norm.x;
  p1 *= norm.y;
  p2 *= norm.z;
  p3 *= norm.w;

// Mix final noise value
  dvec4 m = max(0.6 - dvec4(dot(x0,x0), dot(x1,x1), dot(x2,x2), dot(x3,x3)), 0.0);
  m = m * m;
  return 42.0 * dot( m*m, dvec4( dot(p0,x0), dot(p1,x1), 
                                dot(p2,x2), dot(p3,x3) ) );
  }


double perlinNoise(dvec2 v)
{
  const dvec4 C = dvec4(0.211324865405187,  // (3.0-sqrt(3.0))/6.0
                      0.366025403784439,  // 0.5*(sqrt(3.0)-1.0)
                     -0.577350269189626,  // -1.0 + 2.0 * C.x
                      0.024390243902439); // 1.0 / 41.0
// First corner
  dvec2 i  = floor(v + dot(v, C.yy) );
  dvec2 x0 = v -   i + dot(i, C.xx);

// Other corners
  dvec2 i1;
  //i1.x = step( x0.y, x0.x ); // x0.x > x0.y ? 1.0 : 0.0
  //i1.y = 1.0 - i1.x;
  i1 = (x0.x > x0.y) ? dvec2(1.0, 0.0) : dvec2(0.0, 1.0);
  // x0 = x0 - 0.0 + 0.0 * C.xx ;
  // x1 = x0 - i1 + 1.0 * C.xx ;
  // x2 = x0 - 1.0 + 2.0 * C.xx ;
  dvec4 x12 = x0.xyxy + C.xxzz;
  x12.xy -= i1;

// Permutations
  i = perlinNoise_mod289(i); // Avoid truncation effects in permutation
  dvec3 p = perlinNoise_permute( perlinNoise_permute( i.y + dvec3(0.0, i1.y, 1.0 ))
    + i.x + dvec3(0.0, i1.x, 1.0 ));

  dvec3 m = max(0.5 - dvec3(dot(x0,x0), dot(x12.xy,x12.xy), dot(x12.zw,x12.zw)), 0.0);
  m = m*m ;
  m = m*m ;

// Gradients: 41 points uniformly over a line, mapped onto a diamond.
// The ring size 17*17 = 289 is close to a multiple of 41 (41*7 = 287)

  dvec3 x = 2.0 * fract(p * C.www) - 1.0;
  dvec3 h = abs(x) - 0.5;
  dvec3 ox = floor(x + 0.5);
  dvec3 a0 = x - ox;

// Normalise gradients implicitly by scaling m
// Approximation of: m *= inversesqrt( a0*a0 + h*h );
  m *= 1.79284291400159 - 0.85373472095314 * ( a0*a0 + h*h );

// Compute final noise value at P
  dvec3 g;
  g.x  = a0.x  * x0.x  + h.x  * x0.y;
  g.yz = a0.yz * x12.xz + h.yz * x12.yw;
  return 130.0 * dot(m, g);
}

