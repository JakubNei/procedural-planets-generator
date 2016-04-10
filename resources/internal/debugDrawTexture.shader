

[VertexShader]

layout(location = 0) in vec3 in_position;
layout(location = 1) in vec3 in_normal;
layout(location = 2) in vec3 in_tangent;
layout(location = 3) in vec2 in_uv;
// in mat4 in_modelMatrix; // instanced rendering


uniform vec4 debugDrawTexturePositionScale;
uniform vec4 debugDrawTexturePositionOffset;

void main()
{
	
	gl_Position = debugDrawTexturePositionOffset + vec4(in_position,1) * debugDrawTexturePositionScale;

}


[FragmentShader]


uniform sampler2D debugDrawTexture;

uniform float debugDrawTextureScale = 1;
uniform float debugDrawTextureOffset = 0;

layout(location = 0) out vec4 out_color;

void main()
{

	vec2 screenCoord = gl_FragCoord.xy / engine.screenSize;

	out_color = debugDrawTextureOffset + texture(debugDrawTexture, screenCoord) * debugDrawTextureScale ;
	
}
	
