[include internal/prependAll.shader]

uniform sampler2D param_biomesSplatMap;
uniform sampler2D param_rock;
uniform sampler2D param_snow;

uniform float param_finalPosWeight;

[VertexShader]

layout(location = 0) in vec3 in_position;
layout(location = 1) in vec3 in_normal;
layout(location = 2) in vec3 in_tangent;
layout(location = 3) in vec2 in_uv;
layout(location = 4) in vec3 in_positionInitial;
layout(location = 5) in vec3 in_normalInitial;
// in mat4 in_modelMatrix; // instanced rendering

out data {
	vec3 worldPos;
	vec3 normal; 
	vec2 uv; 
	vec3 tangent;
} o;

void main()
{
	vec3 modelPosition = mix(in_positionInitial, in_position, param_finalPosWeight);
	vec4 worldPos4 = (model.modelMatrix * vec4(modelPosition, 1));	
	vec3 worldPos3 = worldPos4.xyz / worldPos4.w;

	vec3 normalModelSpace = mix(in_normalInitial, in_normal, param_finalPosWeight);

	gl_Position = model.modelViewProjectionMatrix * vec4(modelPosition,1);
	o.worldPos = worldPos3;
	o.uv = in_uv;
	o.normal = (model.modelMatrix * vec4(normalModelSpace,0)).xyz;
	o.tangent = (model.modelMatrix * vec4(in_tangent,0)).xyz;
}


[FragmentShader]

in data {
	vec3 worldPos;
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
	blendWeights = clamp(pow(blendWeights, vec3(5)),0,1);
	blendWeights /= blendWeights.x + blendWeights.y + blendWeights.z;

	vec2 coord_x = position.yz * scale;
	vec2 coord_y = position.zx * scale;
	vec2 coord_z = position.xy * scale;


	vec3 result;

	#define TRILANAR_RESULT(A,B,C) \
		result = blendWeights.##A * texture2D(tex, coord_##A).xyz; \
		if(blendWeights.##B>0.05f) result += blendWeights.##B * texture2D(tex, coord_##B).xyz; \
		// if(blendWeights.##B>0.05f) result = vec3(1,0,0); \

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
	vec3 pos = engine.cameraPosition + i.worldPos;

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

	return mix(rock, snow, biomesSplatMap);
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
	//out_color = vec4(vec3(0,1,0),1);
	//out_color = vec4(vec3(param_finalPosWeight,0,0),1);
	
}

	