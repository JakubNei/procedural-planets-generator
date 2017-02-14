[include internal/include.all.shader]


[VertexShader]

layout(location = 0) in vec3 in_position;
layout(location = 1) in vec3 in_normal;
layout(location = 2) in vec3 in_tangent;
layout(location = 3) in vec2 in_uv;

out data {
	vec2 uv; 
} o;

void main()
{
	o.uv = in_uv;
	gl_Position = vec4(in_position,1);
}


[FragmentShader]

uniform sampler2D finalDrawTexture;

in data {
	vec2 uv; 
} i;

layout(location = 0) out vec4 out_color;

void main()
{
	out_color = pow(texture2D(finalDrawTexture, i.uv), vec4(engine.gammaCorrectionFinalColor));
}