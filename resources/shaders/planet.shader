[include internal/include.all.shader]

uniform sampler2D param_biomesSplatMap;
uniform sampler2D param_rock;
uniform sampler2D param_snow;
uniform sampler2D param_perlinNoise;

uniform float param_planetRadius;
uniform float param_finalPosWeight;
uniform float param_debugWeight;

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

	vec3 normalModelSpace = in_normal;

	gl_Position = model.modelViewProjectionMatrix * vec4(modelPos,1);
	o.worldPos = worldPos3;
	o.modelPos = modelPos;
	o.uv = in_uv;
	o.normal = (model.modelMatrix * vec4(normalModelSpace,0)).xyz;
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

	float closestPowerOf2(float a) {
		//return pow(2, ceil(log(a)/log(2)));
		if(a>64) return 64;
		if(a>32) return 32;
		if(a>16) return 16;
		if(a>8) return 8;
		if(a>4) return 4;
		if(a>2) return 2;
		return 1;
	}
	float tessLevel(float d1, float d2) {		
		float d=(d1+d2)/2;
		//float r=clamp((1/d)*(keyIK*10), 1, 64);
		//float r=clamp((1/d)*(100.0), 1, 64);
		float r=clamp((1/d)*(100.0), 1, 64);
		r=closestPowerOf2(r);
		return r;		
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

	float PerlinAt(float x, float y) {		
		return texture2D(param_perlinNoise, vec2(x,y)).r;
	}
	float AdjustTerrainAt(float x, float y) {
		float result=0;
		int octaves=3;
		float frequency=1;
		float height=4;
		for(int i=0; i<octaves; i++) {
			result=PerlinAt(x*frequency, y*frequency) * height;
			height/=2;
			frequency*=2;
		}
		return result;
	}



	void main()
	{

		// INTERPOLATE NEW PARAMS
		o.worldPos	= interpolate3D(	i[0].worldPos,	i[1].worldPos,	i[2].worldPos	); 
		o.modelPos	= interpolate3D(	i[0].modelPos,	i[1].modelPos,	i[2].modelPos	); 
		o.normal	= interpolate3D(	i[0].normal,	i[1].normal,	i[2].normal		); 
		o.uv		= interpolate2D(	i[0].uv,		i[1].uv,		i[2].uv			); 
		o.tangent	= interpolate3D(	i[0].tangent,	i[1].tangent,	i[2].tangent	); 


		// APPLY TERRAIN MODIFIER
		vec2 xz=o.uv.xy * param_planetRadius * 50;
		float x=xz.x;
		float y=xz.y;

		o.worldPos += o.normal * AdjustTerrainAt(x, y);
		
		//o.worldPos+=o.normal*adjustedHeight;
		//>>>>//o.normal.y+=adjustedHeight*adjustedHeight;
		//vec3 n=o.normal;
		/*o.normal=mix(
			vec3(n.x)*i[0].normal + vec3(n.y)*i[1].normal + vec3(n.z)*i[2].normal,
			o.normal,
			t
		);*/

		o.normal=normalize(o.normal);
		gl_Position = engine.projectionMatrix * engine.viewMatrix * vec4(o.worldPos,1);

	}












[FragmentShader]

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

	vec3 blendWeights = abs(normal);
	blendWeights = clamp(pow(blendWeights, vec3(10)),0,1);
	blendWeights /= blendWeights.x + blendWeights.y + blendWeights.z;

	vec2 coord_x = position.yz * scale;
	vec2 coord_y = position.zx * scale;
	vec2 coord_z = position.xy * scale;


	vec3 result;

	#define TRILANAR_RESULT(A,B,C) \
		result = blendWeights.##A * texture2D(tex, coord_##A).xyz; \
		if(blendWeights.##B>0.05f) result += blendWeights.##B * texture2D(tex, coord_##B).xyz; \
		//if(blendWeights.##C>0.05f) result = vec3(1,0,0); \
		//if(blendWeights.##B>0.05f) result = vec3(1,0,0); \
		//if(blendWeights.##C>0.05f) result += blendWeights.##C * texture2D(tex, coord_##C).xyz; \		

	if(blendWeights.x > blendWeights.y) { // x>y
		if(blendWeights.x > blendWeights.z) { // x>y,x>z
			if(blendWeights.y > blendWeights.z) { // x>y,x>z,y>z  x>y>z					
				TRILANAR_RESULT(x,y,z)
			} else { // x>y,x>z,z>y  x>z>y
				TRILANAR_RESULT(x,z,y)
			}
		} else { // x>y,z>x  z>x>y 
			TRILANAR_RESULT(z,x,y)
		}
	} else { // y>x
		if(blendWeights.y > blendWeights.z) { // y>x,y>z 
			if(blendWeights.x > blendWeights.z) { // y>x,y>z,x>z  y>x>z
				TRILANAR_RESULT(y,x,z)
			} else { // y>x,y>z,z>x  y>z>x
				TRILANAR_RESULT(y,z,x)
			}
		} else { // y>x,z>y  z>y>x 
			TRILANAR_RESULT(z,y,x)
		}
	}

	return result;

}








float myLog(float base, float num) {
	return log2(num)/log2(base);
}


float rand(vec2 co){
    return fract(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
}


vec3 getColor() {

	float biomesSplatMap = texture2D(param_biomesSplatMap, i.uv).r;
	//return vec3(i.uv.y<-0.4);
	//return vec3(biomesSplatMap);

	vec3 pos = i.modelPos;
    //vec3 pos = engine.cameraPosition + i.worldPos;

	vec3 snow = 
		(
			triPlanar(param_snow, pos, i.normal, 0.005) +
			triPlanar(param_snow, pos, i.normal, 0.05) +
			triPlanar(param_snow, pos, i.normal, 0.5) 
		) / 3;

	vec3 rock = 
		(
			triPlanar(param_rock, pos, i.normal, 0.005) +
			triPlanar(param_rock, pos, i.normal, 0.05) +
			triPlanar(param_rock, pos, i.normal, 0.5) 
		) / 3;

	return rock;
	//return mix(rock, snow, biomesSplatMap);
}


void main()
{
	// if(param_visibility != 1)	{
	//	if(clamp(rand(gl_FragCoord.xy),0,1) > param_visibility) discard;
	// }	

	// BASE COLOR
	//float pixelDepth = gl_FragCoord.z/gl_FragCoord.w; //distance(EyePosition, i.worldPos);
	vec3 color = vec3(1,1,1);
	color = getColor();


	out_color = vec4(pow(color,vec3(engine.gammaCorrectionTextureRead)),1);
	//out_normal = normalize(i.normal);
	out_normal = i.normal;
	out_position = i.worldPos;
	out_data = vec4(0);

	//DEBUG
	//out_color = vec4(vec3(1,0,0),1);
	//out_color = vec4(vec3(param_finalPosWeight,0,0),1);
	//out_color = vec4(param_debugWeight,0,0,1);
}

	