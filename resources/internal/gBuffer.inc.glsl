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
	return textureLod(gBufferUniform.final, screenCoord, 0).xyz;
}

float GBufferGetDepth(vec2 screenCoord) {
	return textureLod(gBufferUniform.depthBuffer, screenCoord, 0).x;
}

void GBufferPackData_Emission(out vec4 data, float emission) {
	data.x = emission;
}



GBufferPerPixel GetGBufferPerPixel(vec2 screenCoord) {
	GBufferPerPixel g;
	//vec2 screenCoord = gl_FragCoord.xy / engine.screenSize;
	
	g.color = textureLod(gBufferUniform.albedo, screenCoord, 0).xyz;
	g.position = textureLod(gBufferUniform.position, screenCoord, 0).xyz;
	g.normal = textureLod(gBufferUniform.normal, screenCoord, 0).xyz;
	vec4 data = textureLod(gBufferUniform.data, screenCoord, 0);

	g.emission = data.x;
	g.metallic = data.y;
	g.smoothness = data.z;

	g.final = texture2D(gBufferUniform.final, screenCoord).xyz;
	g.depth = GBufferGetDepth(screenCoord);

	return g;
}
