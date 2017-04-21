struct GBufferData {
	sampler2D depth;
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

// from http://stackoverflow.com/questions/6652253/getting-the-true-z-value-from-the-depth-buffer
// 0..1 to zNear..zFar
float linearDepth(float depthSample)
{
    depthSample = 2.0 * depthSample - 1.0;
    float zLinear = 2.0 * engine.nearClipPlane * engine.farClipPlane / (engine.farClipPlane + engine.nearClipPlane - depthSample * (engine.farClipPlane - engine.nearClipPlane));
    return zLinear;
}

// result suitable for assigning to gl_FragDepth
// zNear..zFar to 0..1
float depthSample(float linearDepth)
{
    float nonLinearDepth = (engine.farClipPlane + engine.nearClipPlane - 2.0 * engine.nearClipPlane * engine.farClipPlane / linearDepth) / (engine.farClipPlane - engine.nearClipPlane);
    nonLinearDepth = (nonLinearDepth + 1.0) / 2.0;
    return nonLinearDepth;
}


vec2 GBufferGetScreenCoord()
{
	return gl_FragCoord.xy/engine.screenSize;	
}
vec3 GBufferGetFinal(vec2 screenCoord) {
	return textureLod(gBufferUniform.final, screenCoord, 0).xyz;
}
/*float GBufferGetDepth() {
	return GBufferGetDepth(gl_FragCoord.xy/engine.screenSize);
}*/

// 0..1
float GBufferGetDepth(vec2 screenCoord) {
	return textureLod(gBufferUniform.depth, screenCoord, 0).x;
}

// zNear..zFar
float GBufferDistanceOfCurrentFragmentToDepthBuffer()
{
	return linearDepth(GBufferGetDepth(GBufferGetScreenCoord())) - linearDepth(gl_FragCoord.z);
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
