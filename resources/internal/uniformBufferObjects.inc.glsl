

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
	float totalElapsedSecondsSinceEngineStart; // totalElapsedSecondsSinceEngineStart
	float gammaCorrectionTextureRead; // 2.2
	float gammaCorrectionFinalColor; // 1/2.2
} engine;



vec3 GammaCorrectTextureRead(vec3 rgb) {
	return pow(rgb, vec3(engine.gammaCorrectionTextureRead));
}
vec3 GammaCorrectFinalColor(vec3 rgb) {
	return pow(rgb, vec3(1.0 / engine.gammaCorrectionFinalColor));
}


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



