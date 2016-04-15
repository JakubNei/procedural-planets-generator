[include internal/prependAll.shader]

[VertexShader] // pass thru vertex shader

layout(location = 0) in vec3 in_position;

void main()
{
	gl_Position = vec4(in_position,1);
}





[FragmentShader]

uniform vec3 testColor;

layout(location = 0) out vec4 out_color;



//http://www.walterzorn.de/en/grapher/grapher_e.htm
// 0.5*exp(-( x*x / pow(6/4, 2) ))
float gausian(float x, float totalWidth) 
{
	return exp(-( (x*x)/(totalWidth/4)*(totalWidth/4) ));	
}


// https://developer.nvidia.com/gpugems/GPUGems3/gpugems3_ch13.html
void main()
{


	vec2 screenCoord = gl_FragCoord.xy / engine.screenSize;

	int NUM_SAMPLES = 15;
	vec2 ScreenLightPos = vec2(0.5, 0.5);
	vec2 texCoord = screenCoord;
	float Density = 0.5;
	float Decay = 0.9;
	float Weight = 0.1;
	float Exposure = 1;

	// Calculate vector from pixel to light source in screen space.
	vec2 deltaTexCoord = (texCoord - ScreenLightPos.xy);
	// Divide by number of samples and scale by control factor.
	deltaTexCoord *= 1.0f / NUM_SAMPLES * Density;
	// Store initial sample.
	vec3 color = textureLod(gBufferUniform.final, texCoord, 0).xyz;
	// Set up illumination decay factor.
	float illuminationDecay = 1.0f;
	// Evaluate summation from Equation 3 NUM_SAMPLES iterations.
	for (int i = 0; i < NUM_SAMPLES; i++)
	{
		// Step sample location along ray.
		texCoord -= deltaTexCoord;
		// Retrieve sample at new location.
		vec3 s = textureLod(gBufferUniform.final, texCoord, 4).xyz;
		// Apply sample attenuation scale/decay factors.
		s *= illuminationDecay * Weight;
		// Accumulate combined color.
		color += s;
		// Update exponential decay factor.
		illuminationDecay *= Decay;
	}
	

	out_color = vec4(color * Exposure, 1);

	//DEBUG
	//out_color = vec4(1,0,0, 1);
		
}
	

