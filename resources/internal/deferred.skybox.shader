

[VertexShader]

layout(location = 0) in vec3 in_position;
layout(location = 1) in vec3 in_normal;
layout(location = 2) in vec3 in_tangent;
layout(location = 3) in vec2 in_uv;

out data {	
	vec3 uv; 
} o;

void main()
{
	gl_Position = engine.viewProjectionMatrix * vec4(in_position + engine.cameraPosition , 1);
		
	o.uv = in_position;
}


[FragmentShader]

uniform samplerCube skyboxCubeMap;

in data {	
	vec3 uv; 
} i;

layout(location = 0) out vec4 out_color;
layout(location = 1) out vec3 out_position;
layout(location = 2) out vec3 out_normal;
layout(location = 3) out vec4 out_uv;

void main()
{

	//out_color = vec4(i.uv/2+0.5, 1);
	out_color = texture(skyboxCubeMap, i.uv);

	out_position = vec3(0);
	out_normal = vec3(0);
	out_uv = vec4(0);

}

	









