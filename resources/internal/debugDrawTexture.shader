[include internal/prependAll.shader]



uniform sampler2D debugDrawTexture;
uniform vec4 debugDrawTexturePositionScale;
uniform vec4 debugDrawTexturePositionOffset;
uniform float debugDrawTextureScale = 1;
uniform float debugDrawTextureOffset = 0;
uniform float debugDrawTextureGamma = 1;

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
	gl_Position = debugDrawTexturePositionOffset + vec4(in_position,1) * debugDrawTexturePositionScale;
	o.uv = in_uv;
}


[FragmentShader]

in data {
	vec2 uv; 
} i;

layout(location = 0) out vec4 out_color;

void main()
{
	vec3 color = texture2D(debugDrawTexture, i.uv).rgb;
	out_color = debugDrawTextureOffset + vec4(pow(color,vec3(debugDrawTextureGamma)),1) * debugDrawTextureScale;	
}
	
