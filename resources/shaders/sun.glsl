// http://www.mathematik.uni-marburg.de/~menzel/index.php?seite=tutorials&id=1

[include internal/include.all.shader]

uniform sampler2D param_turbulenceColorGradient;
uniform sampler2D param_turbulenceMap;
uniform sampler2D param_surfaceDiffuse;





[VertexShader]

layout(location = 0) in vec3 in_position;
layout(location = 1) in vec3 in_normal;
layout(location = 2) in vec3 in_tangent;
layout(location = 3) in vec2 in_uv;

out data {
	vec3 worldPos;
	vec3 normal; 
	vec2 uv; 
	vec3 tangent;
} o;

void main()
{
	o.uv = in_uv;

	float t = texture2D(param_turbulenceMap, in_normal.xy + in_normal.yz + in_normal.zx + engine.totalElapsedSecondsSinceEngineStart*0.002).r;

	vec3 modelPos = in_position;
	modelPos *= 1-t*0.03;

	gl_Position = model.modelViewProjectionMatrix * vec4(modelPos,1);
	vec4 p = (model.modelMatrix * vec4(modelPos, 1));
	o.worldPos = p.xyz / p.w;



	o.normal = normalize((model.modelMatrix * vec4(in_normal,0)).xyz);
	//o.normal = in_normal;

	o.tangent = normalize((model.modelMatrix * vec4(in_tangent,0)).xyz);
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
	blendWeights = clamp(pow(blendWeights, vec3(1)),0,1);
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



void main()
{
		
	vec3 pos = engine.cameraPosition + i.worldPos;

	//vec4 c = texture2D(param_surfaceDiffuse, i.uv + engine.totalElapsedSecondsSinceEngineStart*0.003);
	vec3 c = triPlanar(param_surfaceDiffuse, pos + engine.totalElapsedSecondsSinceEngineStart * 1, i.normal, 0.001);
	float g;

	//g = texture2D(param_turbulenceMap, i.uv + engine.totalElapsedSecondsSinceEngineStart*0.005).r;
	g = triPlanar(param_turbulenceMap, pos + engine.totalElapsedSecondsSinceEngineStart*12, i.normal, 0.001).r;
	c += 0.5 * texture2D(param_turbulenceColorGradient, vec2( 0.01+g*0.98, 0)).rgb;

	//g = texture2D(param_turbulenceMap, i.uv.yx + engine.totalElapsedSecondsSinceEngineStart*0.01).r;
	g = triPlanar(param_turbulenceMap, pos + engine.totalElapsedSecondsSinceEngineStart*15, i.normal, 0.001).r;
	c += 0.5 * texture2D(param_turbulenceColorGradient, vec2( 0.01+g*0.98, 0)).rgb;

	vec3 color = c.rgb;

	out_color = vec4(pow(color,vec3(engine.gammaCorrectionTextureRead)),1);
	out_normal = i.normal;
	out_position = i.worldPos;
	GBufferPackData_Emission(out_data, 1);
	
}

	