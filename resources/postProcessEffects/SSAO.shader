
[VertexShader] // pass thru vertex shader

layout(location = 0) in vec3 in_position;

void main()
{
	gl_Position = vec4(in_position,1);
}





[FragmentShader]

uniform vec3 testColor;

layout(location = 0) out vec4 out_color;

void main()
{


	vec2 screenCoord = gl_FragCoord.xy / engine.screenSize;

	GBufferPerPixel gBuffer = GetGBufferPerPixel();

	// skybox, just pass thru color
	if (gBuffer.normal == vec3(0, 0, 0)) {
		out_color = vec4(gBuffer.color, 1);
		return;
	}

	//out_color = vec4(pow(gBuffer.final,vec3(gBuffer.depth)), 1);
	out_color = vec4(gBuffer.final, 1);
	out_color = vec4(testColor, 1);
	
	//out_color = vec4(vec3(gBuffer.depth*100), 1);
	
}
	

