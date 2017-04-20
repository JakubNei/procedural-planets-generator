[include internal/include.all.shader]

[include shaders/include.planet.glsl]


[VertexShader]

layout(location = 0) in vec3 in_position;
layout(location = 1) in vec3 in_normal;
layout(location = 2) in vec3 in_tangent;
layout(location = 3) in vec2 in_uv;

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
	o.normal = (model.modelMatrix * vec4(in_normal,0)).xyz;
	o.uv = in_uv;
	o.tangent = (model.modelMatrix * vec4(in_tangent,0)).xyz;
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


void main()
{

	vec3 color = vec3(0,0,1);	

	out_color = vec4(pow(color,vec3(engine.gammaCorrectionTextureRead)),0.3);

}

	