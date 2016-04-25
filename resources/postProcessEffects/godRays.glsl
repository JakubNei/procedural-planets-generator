[include internal/prependAll.shader]

uniform vec3 param_lightScreenPos;
uniform vec3 param_lightWorldPos;
uniform float param_lightWorldRadius;


[VertexShader] // pass thru vertex shader

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

in data {
	vec2 uv; 
} i;

layout(location = 0) out vec4 out_color;



//http://www.walterzorn.de/en/grapher/grapher_e.htm
// 0.5*exp(-( x*x / pow(6/4, 2) ))
float gausian(float x, float totalWidth) 
{
	return exp(-( (x*x)/(totalWidth/4)*(totalWidth/4) ));	
}


/// <summary>
/// Gets the luminance value for a pixel.
/// <summary>
float GetLuminance (vec3 rgb)
{
	// ITU-R BT.709 primaries
	//return (0.2126 * rgb.x) + (0.7152 * rgb.y) + (0.0722 * rgb.z);
	return dot(rgb, vec3(0.2126f, 0.7152f, 0.0722f));
}

// https://developer.nvidia.com/gpugems/GPUGems3/gpugems3_ch13.html
void main()
{

	vec2 screenCoord = i.uv;

	/*
	if(
		any(greaterThan(param_lightScreenPos.xy, vec2(1))) ||
		any(lessThan(param_lightScreenPos.xy, vec2(0)))
	) {
		vec3 color = textureLod(gBufferUniform.final, screenCoord, 0).xyz;
		out_color = vec4(color, 1);
		return;
	}
	*/


	int NUM_SAMPLES = 20;
	vec2 texCoord = screenCoord;
	float Density = 0.5;
	float Decay = 0.9;
	float Weight = 0.1;
	float Exposure = 0.5;


	// Calculate vector from pixel to light source in screen space.
	vec2 deltaTexCoord = (texCoord - param_lightScreenPos.xy);


	//NUM_SAMPLES = int(ceil(length(deltaTexCoord) * 20));

	// Divide by number of samples and scale by control factor.
	deltaTexCoord *= 1.0f / NUM_SAMPLES * Density;
	// Store initial sample.
	vec3 color = textureLod(gBufferUniform.final, texCoord, 0).xyz;

	//float depth = linearDepth(textureLod(gBufferUniform.depth, texCoord, 0).x);
	//if(depth > linearDepth(param_lightScreenPos.z)) {

	//vec3 coord = textureLod(gBufferUniform.position, texCoord, 0).xyz;
	//vec3 normal = textureLod(gBufferUniform.normal, texCoord, 0).xyz;
	//float c = dot(normalize(normal), normalize(param_lightWorldPos-coord));

	//out_color = vec4(vec3(c),1); return;

	

	// Set up illumination decay factor.
	float illuminationDecay = 1;
	// Evaluate summation from Equation 3 NUM_SAMPLES iterations.
	for (int i = 0; i < NUM_SAMPLES; i++)
	{
		// Step sample location along ray.
		texCoord -= deltaTexCoord;

		vec3 worldPos = textureLod(gBufferUniform.position, texCoord, 0).xyz;
		if(worldPos==vec3(0) || distance(worldPos, param_lightWorldPos) < param_lightWorldRadius) {
			// Retrieve sample at new location.
			vec3 s = textureLod(gBufferUniform.final, texCoord, 5).rgb;
			// Apply sample attenuation scale/decay factors.
			s *= illuminationDecay * GetLuminance(s) * Weight;
			// Accumulate combined color.
			color += s * Exposure * 100;
		} 

		// Update exponential decay factor.
		illuminationDecay *= Decay;
	}


	out_color = vec4(color, 1);

	//DEBUG
	//out_color = vec4(1,0,0, 1);
	//out_color = vec4(vec3(linearDepth(texture2D(gBufferUniform.depth, screenCoord).x) / 10000), 1);
		
}
	