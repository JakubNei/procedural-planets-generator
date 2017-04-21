[include internal/include.all.shader]

[include shaders/include.planet.glsl]

uniform sampler2D param_perlinNoise;
uniform dvec3 param_offsetFromPlanetCenter;
uniform vec3 param_remainderOffset;


[VertexShader]

layout(location = 0) in vec3 in_position;
layout(location = 1) in vec3 in_normal;
layout(location = 2) in vec3 in_tangent;
layout(location = 3) in vec2 in_uv;
layout(location = 4) in vec4 in_biomes1;
layout(location = 5) in vec4 in_biomes2;
// in mat4 in_modelMatrix; // instanced rendering

out data {
	vec3 worldPos;
	vec3 modelPos;
	vec3 normal; 
	vec2 uv; 
	vec3 tangent;
	vec4 biomes1;
	vec4 biomes2;
} o;

void main()
{
	vec3 modelPos = in_position;
	vec4 worldPos4 = (model.modelMatrix * vec4(modelPos, 1));	
	vec3 worldPos3 = worldPos4.xyz / worldPos4.w;

	gl_Position = model.modelViewProjectionMatrix * vec4(modelPos,1);
	o.worldPos = worldPos3;
	o.modelPos = modelPos;
	o.normal = (model.modelMatrix * vec4(in_normal,0)).xyz;
	o.tangent = (model.modelMatrix * vec4(in_tangent,0)).xyz;
	o.uv = in_uv;
	o.biomes1 = in_biomes1;
	o.biomes2 = in_biomes2;

	//DEBUG
	//o.biomes2 = vec4(1,0,0,0);
}








	

[TessControlShader]

layout(vertices = 3) out;	

in data {
	vec3 worldPos;
	vec3 modelPos;
	vec3 normal; 
	vec2 uv; 
	vec3 tangent;
	vec4 biomes1;
	vec4 biomes2;
} i[];

out data {
	vec3 worldPos;
	vec3 modelPos;
	vec3 normal; 
	vec2 uv; 
	vec3 tangent;
	vec4 biomes1;
	vec4 biomes2;		
} o[];

int closestPowerOf2(float a) {
	//return pow(2, ceil(log(a)/log(2)));
	//return 1;
	if(a>64) return 64;
	if(a>32) return 32;
	if(a>16) return 16;
	if(a>8) return 8;
	if(a>4) return 4;
	if(a>2) return 2;
	return 1;
}
int tessLevel(float d1, float d2) {		
	float d=(d1+d2)/2;
	//float r=clamp((1/d)*(keyIK*10), 1, 64);
	//float r=clamp((1/d)*(100.0), 1, 64);
	float r=clamp(100/d, 1, 64);
	return closestPowerOf2(r);
}

void main() {

	gl_out[gl_InvocationID].gl_Position = gl_in[gl_InvocationID].gl_Position;

	// COPY OVER PARAMS
	o[gl_InvocationID].worldPos = i[gl_InvocationID].worldPos;
	o[gl_InvocationID].modelPos = i[gl_InvocationID].modelPos;
	o[gl_InvocationID].normal 	= i[gl_InvocationID].normal;
	o[gl_InvocationID].uv 		= i[gl_InvocationID].uv;
	o[gl_InvocationID].tangent 	= i[gl_InvocationID].tangent;
	o[gl_InvocationID].biomes1 	= i[gl_InvocationID].biomes1;
	o[gl_InvocationID].biomes2 	= i[gl_InvocationID].biomes2;

	// TESS LEVEL BASED ON EYE DISTANCE
	float d0=distance(i[0].worldPos,vec3(0));
	float d1=distance(i[1].worldPos,vec3(0));
	float d2=distance(i[2].worldPos,vec3(0));	

	gl_TessLevelOuter[0] = tessLevel(d1,d2);
	gl_TessLevelOuter[1] = tessLevel(d0,d2);
	gl_TessLevelOuter[2] = tessLevel(d0,d1);
	//gl_TessLevelOuter[3] = tess;

	gl_TessLevelInner[0] = gl_TessLevelOuter[2];
	//gl_TessLevelInner[1] = tess;

	//DEBUG
	//gl_TessLevelInner[0] = gl_TessLevelOuter[0] = gl_TessLevelOuter[1] = gl_TessLevelOuter[2] = 2;
}





[TessEvaluationShader]

layout(triangles, equal_spacing, ccw) in;

in data {
	vec3 worldPos;
	vec3 modelPos;
	vec3 normal; 
	vec2 uv; 
	vec3 tangent;
	vec4 biomes1;
	vec4 biomes2;
} i[];

out data {
	vec3 worldPos;
	vec3 modelPos;
	vec3 normal; 
	vec2 uv; 
	vec3 tangent;
	vec4 biomes1;
	vec4 biomes2;
} o;

vec2 interpolate2D(vec2 v0, vec2 v1, vec2 v2) {
	return vec2(gl_TessCoord.x)*v0 + vec2(gl_TessCoord.y)*v1 + vec2(gl_TessCoord.z)*v2;
}
vec2 interpolate2D(vec2 v0, vec2 v1, vec2 v2, vec2 v3) {
	return mix(  mix(v0,v1,gl_TessCoord.x),  mix(v2,v3,gl_TessCoord.x),  gl_TessCoord.y);
}
vec3 interpolate3D(vec3 v0, vec3 v1, vec3 v2) {
	return vec3(gl_TessCoord.x)*v0 + vec3(gl_TessCoord.y)*v1 + vec3(gl_TessCoord.z)*v2;
}
vec3 interpolate3D(vec3 v0, vec3 v1, vec3 v2, vec3 v3) {
	return mix(  mix(v0,v1,gl_TessCoord.x),  mix(v2,v3,gl_TessCoord.x),  gl_TessCoord.y);
}
vec4 interpolate4D(vec4 v0, vec4 v1, vec4 v2) {
	return vec4(gl_TessCoord.x)*v0 + vec4(gl_TessCoord.y)*v1 + vec4(gl_TessCoord.z)*v2;
}
vec4 interpolate4D(vec4 v0, vec4 v1, vec4 v2, vec4 v3) {
	return mix(  mix(v0,v1,gl_TessCoord.x),  mix(v2,v3,gl_TessCoord.x),  gl_TessCoord.y);
}

//float PerlinAt(vec2 uv) {		
	//return texture2D(param_perlinNoise, uv).r;
	//return perlinNoise(uv);
//}
float AdjustTerrainAt(vec3 pos) {
	int octaves=3;
	float frequency=1;
	float height=1;
	float result=-height/2;
	for(int i=0; i<octaves; i++) {
		result += perlinNoise(pos*frequency) * height;
		height /= 2;
		frequency *= 2;
	}

	return result;

	// DEBUG
	//const float c = 20;
	//return mod(x, c) > c/2  ? 0.2 : 0;
	
}



void main()
{

	// INTERPOLATE NEW PARAMS
	o.worldPos	= interpolate3D(	i[0].worldPos,	i[1].worldPos,	i[2].worldPos	); 
	o.modelPos	= interpolate3D(	i[0].modelPos,	i[1].modelPos,	i[2].modelPos	); 
	o.normal	= interpolate3D(	i[0].normal,	i[1].normal,	i[2].normal		); 
	o.uv		= interpolate2D(	i[0].uv,		i[1].uv,		i[2].uv			); 
	o.tangent	= interpolate3D(	i[0].tangent,	i[1].tangent,	i[2].tangent	); 
	o.biomes1	= interpolate4D(	i[0].biomes1,	i[1].biomes1,	i[2].biomes1	); 
	o.biomes2	= interpolate4D(	i[0].biomes2,	i[1].biomes2,	i[2].biomes2	); 

	vec3 pos = vec3(o.modelPos + param_offsetFromPlanetCenter);

	/*{
		// add some random variaton to biomes boundaries
		vec3 pp = o.modelPos;
		const float cw = 0.5;
		const float pw = 1;

	    o.biomes1.x *= cw + perlinNoise(pp / 200) * pw;
	    o.biomes1.y *= cw + perlinNoise(pp / 225) * pw;
	    o.biomes1.z *= cw + perlinNoise(pp / 250) * pw;
	    o.biomes1.w *= cw + perlinNoise(pp / 275) * pw;

	    o.biomes2.x *= cw + perlinNoise(pp / 300) * pw;
	    o.biomes2.y *= cw + perlinNoise(pp / 325) * pw;
	    o.biomes2.z *= cw + perlinNoise(pp / 350) * pw;
	    o.biomes2.w *= cw + perlinNoise(pp / 375) * pw;

	    o.biomes1 = clamp(o.biomes1, 0, 1);
	    o.biomes2 = clamp(o.biomes2, 0, 1);

		float sum = 
			o.biomes1.x + o.biomes1.y + o.biomes1.z + o.biomes1.w +
			o.biomes2.x + o.biomes2.y + o.biomes2.z + o.biomes2.w;

		o.biomes1 /= sum;
		o.biomes2 /= sum;
	}*/


	//o.uv = calestialToSpherical(o.modelPos + param_offsetFromPlanetCenter).xy;

	// APPLY TERRAIN MODIFIER
	
	// make it uniform across different planet sizes
	//pos *= float(param_radiusMin) / 20000;
	float amount = AdjustTerrainAt(pos) * 0.1;
	o.worldPos += o.normal * amount;

	//o.normal *= 1 + amount;
	//o.normal = normalize(o.normal);

	//o.tangent *= 1 + amount;
	//o.tangent = normalize(o.tangent);

	gl_Position = engine.projectionMatrix * engine.viewMatrix * vec4(o.worldPos,1);

}












[FragmentShader]
#line 224

in data {
	vec3 worldPos;
	vec3 modelPos;
	vec3 normal; 
	vec2 uv; 
	vec3 tangent;
	vec4 biomes1;
	vec4 biomes2;
} i;

layout(location = 0) out vec4 out_color;
layout(location = 1) out vec3 out_position;
layout(location = 2) out vec3 out_normal;
layout(location = 3) out vec4 out_data;


// TRIPLANAR TEXTURE PROJECTION
vec3 triPlanar(sampler2D tex, vec3 position, vec3 normal, float scale) {

	//DEBUG
	//return vec3(0);

	vec3 blendWeights = pow(abs(normal), vec3(20));
	blendWeights /= blendWeights.x + blendWeights.y + blendWeights.z;

	vec2 coord_x = position.yz * scale;
	vec2 coord_y = position.zx * scale;
	vec2 coord_z = position.xy * scale;


	vec3 result = vec3(0);

#ifdef USE_NON_OPTIMIZED_TRIPLNAR
	
	result = 
		blendWeights.x * texture2D(tex, coord_x).xyz +
		blendWeights.y * texture2D(tex, coord_y).xyz +
		blendWeights.z * texture2D(tex, coord_z).xyz;

#else

	const float threshold = 0.05;

	vec3 finalWeights = vec3(0);

	if(blendWeights.x > threshold) finalWeights.x = blendWeights.x;
	if(blendWeights.y > threshold) finalWeights.y = blendWeights.y;
	if(blendWeights.z > threshold) finalWeights.z = blendWeights.z;

	finalWeights /= finalWeights.x + finalWeights.y + finalWeights.z;

	if(finalWeights.x > 0) result += finalWeights.x * texture2D(tex, position.yz * scale).xyz;
	if(finalWeights.y > 0) result += finalWeights.y * texture2D(tex, position.zx * scale).xyz;
	if(finalWeights.z > 0) result += finalWeights.z * texture2D(tex, position.xy * scale).xyz;

#endif

/*
// debug weights
int numOfReads = 0;
if(blendWeights.x > threshold) numOfReads++;
if(blendWeights.y > threshold) numOfReads++;
if(blendWeights.z > threshold) numOfReads++;
if(numOfReads == 1) result = vec3(0,1,0);
if(numOfReads == 2) result = vec3(0,0,1);
if(numOfReads == 3) result = vec3(1,0,0);
/**/

	return result;

}


float myLog(float base, float num) {
	return log2(num)/log2(base);
}


float rand(vec2 co){
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}


vec3 getBiomeColor(sampler2D diffuseMap)
{
	vec3 pos = vec3(i.modelPos + param_remainderOffset);	
	return triPlanar(diffuseMap, pos, i.normal, 0.5);
}
vec3 getBiomeNormal(sampler2D normalMap)
{
	vec3 pos = vec3(i.modelPos + param_remainderOffset);
	return triPlanar(normalMap, pos, i.normal, 0.5);
}

vec3 getBiomeColor(sampler2D diffuseMap, vec3 offset)
{
	vec3 pos = vec3(i.modelPos + param_remainderOffset + offset);	
	return triPlanar(diffuseMap, pos, i.normal, 0.5);
}
vec3 getBiomeNormal(sampler2D normalMap, vec3 offset)
{
	vec3 pos = vec3(i.modelPos + param_remainderOffset + offset);
	return triPlanar(normalMap, pos, i.normal, 0.5);
}



// #define USE_NON_OPTIMIZED_TRIPLNAR
#define NORMAL_MAPPING_DISTANCE 10000000.0
// 1000



void getColor(vec2 uv, out vec3 color, out vec3 normal) {


	//DEBUG
	//color = vec3(i.uv.x, 0, 0); return;
	//color = texture2D(param_biomesSplatMap1, i.uv).xyz; return;
	//color = vec3(temperature); return;
	//color = i.biomes1.xyz; return;
		
	// must set default values, on some GPUs missing default values cause error in calculations, thus on some GPUs random old data is used
	color = vec3(0);

	bool shouldCalculateNormal = length(i.worldPos) < NORMAL_MAPPING_DISTANCE;
	if(shouldCalculateNormal) normal = vec3(0);
	else normal = vec3(0.5, 0.5, 1);


	float amount;

#define RENDER_BIOME(ID, CHANNEL) \
	amount = i.biomes##ID##.##CHANNEL; \
	if(amount > 0) { \
		color += amount * getBiomeColor(param_biome##ID##CHANNEL##_diffuseMap); \
		if(shouldCalculateNormal) \
			normal += amount * getBiomeNormal(param_biome##ID##CHANNEL##_normalMap); \
	}

    //RENDER_BIOME(1,r)
    //special handling for water
	amount = i.biomes1.r;
	if(amount > 0) {
		vec3 offset = vec3(engine.totalElapsedSecondsSinceEngineStart);
		color += amount * getBiomeColor(param_biome1r_diffuseMap, offset);
		if(shouldCalculateNormal)
			normal += amount * getBiomeNormal(param_biome1r_normalMap, offset);
	}

    RENDER_BIOME(1,g)
    RENDER_BIOME(1,b)
    RENDER_BIOME(1,a)
    RENDER_BIOME(2,r)
    RENDER_BIOME(2,g)
    RENDER_BIOME(2,b)
    RENDER_BIOME(2,a)

}



void main()
{

	// if(param_visibility != 1)	{
	//	if(clamp(rand(gl_FragCoord.xy),0,1) > param_visibility) discard;
	// }	

	// BASE COLOR
	//float pixelDepth = gl_FragCoord.z/gl_FragCoord.w; //distance(EyePosition, i.worldPos);
	
	float distToCamera = length(i.worldPos);	
	vec3 pos = vec3(i.modelPos + param_offsetFromPlanetCenter);	
	vec2 uv = calestialToSpherical(pos).xy;



	//uv.x += perlinNoise(pos / 10000) / 200;
	//uv.y += perlinNoise(pos.yxz / 10000) / 200;
	vec3 normal = vec3(0,0,1);

	normal = GetProceduralAndBaseHeightMapNormal(i.uv, 0.00001);


	vec3 color;
	vec3 normalColorFromTexture;
	getColor(uv, color, normalColorFromTexture);

	float defaultNormalWeight = smoothstep(NORMAL_MAPPING_DISTANCE * 0.9, NORMAL_MAPPING_DISTANCE, distToCamera);

	if(defaultNormalWeight < 1) {
		normalColorFromTexture = normalize(normalColorFromTexture.xyz*2.0-1.0);
		normal = normalize(mix(normalColorFromTexture, normal, 0.5));
	}

	{
		//normal.xy *= 10;
		normal = normalize(normal);

		vec3 N = i.normal;
		vec3 T = i.tangent;
		vec3 T2 = T - N * dot(N, T); // Gram-Schmidt orthogonalization of T
		vec3 B = normalize(cross(N,T2));
		mat3 normalMatrix = mat3(T,B,N); // column0, column1, column2

		out_normal = normalMatrix * normal;
	}

	out_color = vec4(pow(color,vec3(engine.gammaCorrectionTextureRead)),1);
	//out_color = vec4(color,1);

	//out_normal = i.normal;
	out_position = i.worldPos;
	out_data = vec4(0);

	//DEBUG
	//out_color = vec4(vec3(defaultNormalWeight,0,0),1);
	//out_color = vec4(i.uv,0,1);
	//out_color = vec4(vec3(texture2D(param_biomesSplatMap0, i.uv).x), 1);
	//out_color = vec4(vec3(texture2D(param_humidityMap, i.uv).r), 1);	
	//out_color = vec4(vec3(param_finalPosWeight,0,0),1);
	//out_color = vec4(param_debugWeight,0,0,1);
	//out_color = vec4(i.tangent,1);
	//out_color = vec4(i.normal,1);
	//out_color = vec4(vec3(i.biomes1.r),1);
	//out_color = vec4(vec3(i.biomes1.g),1);
	//out_color = vec4(vec3(i.biomes2),1);


	// FOG		
	//float z=distance(i.worldPos.xz, EyePosition.xz)/fogDistance;
	//float fogFactor = smoothstep(1.0-fogPower,1.0,z);
	//color = mix(color, vec4(color.rgb,0), fogFactor);

}

	