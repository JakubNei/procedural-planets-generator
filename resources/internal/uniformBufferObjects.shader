

layout(std140) uniform engine_block {
	mat4 viewMatrix;
	mat4 projectionMatrix;
	mat4 viewProjectionMatrix;
	vec3 cameraPosition;
	int numberOfLights;
	vec2 screenSize;
	float nearClipPlane;
	float farClipPlane;
	vec3 ambientColor;
} engine;


layout(std140) uniform model_block { 
	mat4 modelMatrix;
	mat4 modelViewMatrix;
	mat4 modelViewProjectionMatrix;
} model;




struct MaterialData {
	vec4 albedo;
	sampler2D albedoTexture;
	float metallic;
	sampler2D metallicTexture;
	float smoothness;
	sampler2D smoothnessTexture;
	vec3 emission;

	bool useNormalMapping;
	bool useParallaxMapping;
	sampler2D normalMap;
	sampler2D depthMap;
};
uniform MaterialData material;

uniform mat4 shadowMapMatrix;





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
	float metallic;
	float smoothness;
	vec3 final;
	float depth;
};

float GBufferGetDepth(vec2 screenCoord) {
	return texture(gBufferUniform.depthBuffer, screenCoord).x;
}

GBufferPerPixel GetGBufferPerPixel(vec2 screenCoord) {
	GBufferPerPixel g;
	//vec2 screenCoord = gl_FragCoord.xy / engine.screenSize;
	
	g.color = texture(gBufferUniform.albedo, screenCoord).xyz;
	g.position = texture(gBufferUniform.position, screenCoord).xyz;
	g.normal = texture(gBufferUniform.normal, screenCoord).xyz;
	vec4 data = texture(gBufferUniform.data, screenCoord);

	g.metallic = data.x;
	g.smoothness = data.y;

	g.final = texture(gBufferUniform.final, screenCoord).xyz;
	g.depth = GBufferGetDepth(screenCoord);

	return g;
}






layout(std140) uniform light_block {
	vec3 color;
	float spotExponent;
	vec3 position; // position == 0,0,0 => directional light 
	float spotCutOff;
	vec3 direction; // direction == 0,0,0 => point light
	int hasShadows;
	int lightIndex;
} light;




struct ShadowMapData {
	sampler2D level0;
	mat4 viewProjectionMatrix;
};
uniform ShadowMapData shadowMap;



