struct GBufferData {
	sampler2D depthBuffer;
	sampler2D albedo;
	sampler2D position;
	sampler2D normal;
	sampler2D data;
	sampler2D final;
};
uniform GBufferData gBufferUniform;

struct GBufferPerPixel {
	vec3 color;
	vec3 position;
	vec3 normal;
	vec3 final;
	float depth;
	float emission;
	float metallic;
	float smoothness;
};

vec3 GBufferGetFinal(vec2 screenCoord) {
	return texture(gBufferUniform.final, screenCoord).xyz;
}

float GBufferGetDepth(vec2 screenCoord) {
	return texture(gBufferUniform.depthBuffer, screenCoord).x;
}

void GBufferPackData_Emission(out vec4 data, float emission) {
	data.x = emission;
}



GBufferPerPixel GetGBufferPerPixel(vec2 screenCoord) {
	GBufferPerPixel g;
	//vec2 screenCoord = gl_FragCoord.xy / engine.screenSize;
	
	g.color = texture(gBufferUniform.albedo, screenCoord).xyz;
	g.position = texture(gBufferUniform.position, screenCoord).xyz;
	g.normal = texture(gBufferUniform.normal, screenCoord).xyz;
	vec4 data = texture(gBufferUniform.data, screenCoord);

	g.emission = data.x;
	g.metallic = data.y;
	g.smoothness = data.z;

	g.final = texture(gBufferUniform.final, screenCoord).xyz;
	g.depth = GBufferGetDepth(screenCoord);

	return g;
}
