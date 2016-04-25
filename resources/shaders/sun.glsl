// http://www.mathematik.uni-marburg.de/~menzel/index.php?seite=tutorials&id=1

[include internal/prependAll.shader]

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
	
	gl_Position = model.modelViewProjectionMatrix * vec4(in_position,1);

	vec4 p = (model.modelMatrix * vec4(in_position, 1));
	o.worldPos = p.xyz / p.w;
	//o.position = in_position;

	o.uv = in_uv;

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




void main()
{
	


	vec4 c = texture2D(param_surfaceDiffuse, i.uv + engine.totalElapsedSecondsSinceEngineStart*0.003);
	float g;

	g = texture2D(param_turbulenceMap, i.uv + engine.totalElapsedSecondsSinceEngineStart*0.005).x;
	c += 0.5 * texture2D(param_turbulenceColorGradient, vec2( 0.01+g*0.98, 0));
	g = texture2D(param_turbulenceMap, i.uv.yx + engine.totalElapsedSecondsSinceEngineStart*0.01).x;
	c += 0.5 * texture2D(param_turbulenceColorGradient, vec2( 0.01+g*0.98, 0));

	vec3 color = c.rgb;

	out_color = vec4(pow(color,vec3(engine.gammaCorrectionTextureRead)),1);
	out_normal = i.normal;
	out_position = i.worldPos;
	GBufferPackData_Emission(out_data, 1);
	
}

	