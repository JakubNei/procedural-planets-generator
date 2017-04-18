[include internal/include.all.shader]

[include shaders/include.planet.glsl]

uniform sampler2D param_perlinNoise;
uniform vec3 param_offsetFromPlanetCenter;




// #define USE_NON_OPTIMIZED_TRIPLNAR
#define NORMAL_MAPPING_DISTANCE 1000.0

[VertexShader]

layout(location = 0) in vec3 in_position;
layout(location = 1) in vec3 in_normal;
layout(location = 2) in vec3 in_tangent;
layout(location = 3) in vec2 in_uv;
// in mat4 in_modelMatrix; // instanced rendering

out data {
	vec3 worldPos;
	vec3 modelPos;
	vec3 normal; 
	vec2 uv; 
	vec3 tangent;
} o;

void main()
{
	vec3 modelPos = in_position;
	vec4 worldPos4 = (model.modelMatrix * vec4(modelPos, 1));	
	vec3 worldPos3 = worldPos4.xyz / worldPos4.w;

	gl_Position = model.modelViewProjectionMatrix * vec4(modelPos,1);
	o.worldPos = worldPos3;
	o.modelPos = modelPos;
	o.uv = in_uv;
	o.normal = (model.modelMatrix * vec4(in_normal,0)).xyz;
	o.tangent = (model.modelMatrix * vec4(in_tangent,0)).xyz;
}








	

[TessControlShader]

layout(vertices = 3) out;	

in data {
	vec3 worldPos;
	vec3 modelPos;
  	vec3 normal; 
	vec2 uv; 
	vec3 tangent;
} i[];

out data {
	vec3 worldPos;
	vec3 modelPos;
  	vec3 normal; 
	vec2 uv; 
	vec3 tangent;
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
	o[gl_InvocationID].normal = i[gl_InvocationID].normal; 
	o[gl_InvocationID].uv = i[gl_InvocationID].uv; 
	o[gl_InvocationID].tangent = i[gl_InvocationID].tangent; 				

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
} i[];

out data {
	vec3 worldPos;
	vec3 modelPos;
  	vec3 normal; 
	vec2 uv; 
	vec3 tangent;
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

//float PerlinAt(vec2 uv) {		
	//return texture2D(param_perlinNoise, uv).r;
	//return perlinNoise(uv);
//}
float AdjustTerrainAt(vec3 pos) {
	int octaves=3;
	float frequency=1;
	float height=0.5;
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

	//o.uv = calestialToSpherical(o.modelPos + param_offsetFromPlanetCenter).xy;

	// APPLY TERRAIN MODIFIER
	vec3 pos = o.modelPos + param_offsetFromPlanetCenter;
	// make it uniform across different planet sizes
	pos *= 200000 / float(param_radiusMin);

	o.worldPos += o.normal * AdjustTerrainAt(pos);
	o.normal = normalize(o.normal);

	gl_Position = engine.projectionMatrix * engine.viewMatrix * vec4(o.worldPos,1);

}












[FragmentShader]
#line 227

in data {
	vec3 worldPos;
	vec3 modelPos;
	vec3 normal;
	vec2 uv;
	vec3 tangent;
} i;

layout(location = 0) out vec4 out_color;
layout(location = 1) out vec3 out_position;
layout(location = 2) out vec3 out_normal;
layout(location = 3) out vec4 out_data;


// TRIPLANAR TEXTURE PROJECTION
vec3 triPlanar(sampler2D tex, vec3 position, vec3 normal, float scale) {

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

	const float threshold = 0.01;
	if(blendWeights.x > threshold) result += blendWeights.x * texture2D(tex, position.yz * scale).xyz;
	if(blendWeights.y > threshold) result += blendWeights.y * texture2D(tex, position.zx * scale).xyz;
	if(blendWeights.z > threshold) result += blendWeights.z * texture2D(tex, position.xy * scale).xyz;

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
	vec3 pos = i.modelPos;
	return
		mix(
			triPlanar(diffuseMap, pos, i.normal, 0.05),
			triPlanar(diffuseMap, pos, i.normal, 0.5),
			1
		);
}
vec3 getBiomeNormal(sampler2D normalMap)
{
	vec3 pos = i.modelPos;
	return  
		mix(
			triPlanar(normalMap, pos, i.normal, 0.05),
			triPlanar(normalMap, pos, i.normal, 0.5),
			1
		);
}


float getChannel(vec4 color, int channel)
{
	if(channel == 0) return color.x;
	if(channel == 1) return color.y;
	if(channel == 2) return color.z;
	return color.w;
}



void getColor(vec2 uv, out vec3 color, out vec3 normal) {

	//DEBUG
	//color = vec3(i.uv.x, 0, 0); return;
	//color = texture2D(param_biomesSplatMap1, i.uv).xyz; return;
		
	// must set default values, on some GPUs missing default values cause error in calculations, thus on some GPUs random old data is used
	color = vec3(0);

	bool calculateNormal = length(i.worldPos) < NORMAL_MAPPING_DISTANCE;
	if(calculateNormal) normal = vec3(0);
	else normal = vec3(0.5, 0.5, 1);

	float amount;

#define ADD_BIOME(ID, CHANNEL) \
	amount = getChannel(texture2D(param_biomesSplatMap##ID##, uv), CHANNEL); \
	if(amount > 0) { \
		color += amount * getBiomeColor(param_biome##ID##CHANNEL##_diffuseMap); \
		if(calculateNormal) \
			normal += amount * getBiomeNormal(param_biome##ID##CHANNEL##_normalMap); \
	}

    ADD_BIOME(0,0)
    ADD_BIOME(0,1)
    ADD_BIOME(0,2)
    ADD_BIOME(0,3)
    ADD_BIOME(1,0)
    ADD_BIOME(1,1)
    ADD_BIOME(1,2)
    ADD_BIOME(1,3)

}


void main()
{
	// if(param_visibility != 1)	{
	//	if(clamp(rand(gl_FragCoord.xy),0,1) > param_visibility) discard;
	// }	

	// BASE COLOR
	//float pixelDepth = gl_FragCoord.z/gl_FragCoord.w; //distance(EyePosition, i.worldPos);
	vec2 uv = calestialToSpherical(i.modelPos + param_offsetFromPlanetCenter).xy;
	vec3 color;
	vec3 normalColorFromTexture;
	getColor(uv, color, normalColorFromTexture);

	float distToCamera = length(i.worldPos);
	float defaultNormalWeight = smoothstep(NORMAL_MAPPING_DISTANCE * 0.9, NORMAL_MAPPING_DISTANCE, distToCamera);

	if(defaultNormalWeight < 1) {
		vec3 N = i.normal;
		vec3 T = i.tangent;
		vec3 T2 = T - N * dot(N, T); // Gram-Schmidt orthogonalization of T
		vec3 B = normalize(cross(N,T2));
		//if (dot(B2, B) < 0) B2 *= -1;
		mat3 normalMatrix = mat3(T,B,N); // column0, column1, column2		
		vec3 normalFromTexture = normalize(normalColorFromTexture.xyz*2.0-1.0);
		out_normal = 
			normalize(mix(				
				normalMatrix * normalFromTexture,
				i.normal,
				defaultNormalWeight
			));
	} else {		
		out_normal = i.normal;
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
	//out_color = vec4(vec3(param_finalPosWeight,0,0),1);
	//out_color = vec4(param_debugWeight,0,0,1);
	//out_color = vec4(i.tangent,1);
	//out_color = vec4(i.normal,1);
}

	