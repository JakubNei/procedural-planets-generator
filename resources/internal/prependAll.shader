#version 400 core


// precision highp float;


layout(std140) uniform engine_block {
	mat4 viewMatrix;
	mat4 projectionMatrix;
	mat4 viewProjectionMatrix;
	vec3 cameraPosition;
	int numberOfLights;
	vec2 screenSize;
	float nearClipPlane;
	float farClipPlane;
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





/*
uniform sampler2D gBuffer_color;
uniform sampler2D gBuffer_position;
uniform sampler2D gBuffer_normal;
uniform sampler2D gBuffer_uv;
*/

/*layout(std140) uniform LightData {
	vec3 color;
	vec3 position; // position == 0,0,0 => directional light 
	vec3 direction; // direction == 0,0,0 => point light
	float spotExponent; 
	float sportCutOff;
} lights[MAX_NUMBER_OF_LIGHTS];*/
/*
struct LightData {
	vec3 color;
	vec3 position; // position == 0,0,0 => directional light 
	vec3 direction; 
	float spotExponent; 
	float sportCutOff;
};
uniform LightData lights[MAX_NUMBER_OF_LIGHTS];
*/


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





float PhongShaderLightIntensity(vec3 worldPosition, vec3 worldNormal) {
	vec3 lightPosition = light.position;
	vec3 lightDirection = light.direction;

	vec3 dirPixelToLight;
	if (lightPosition == vec3(0, 0, 0)) dirPixelToLight = -lightDirection; // is directional light
	else dirPixelToLight = normalize(lightPosition - worldPosition); // is point light
	// else is spot light

	float NdotL = max(dot(worldNormal, dirPixelToLight), 0.0);
	return NdotL;
}

vec3 PhongShader(vec3 worldPosition, vec3 worldNormal, float metallic, float smoothness) {
	vec3 lightPosition = light.position;
	vec3 lightDirection = light.direction;
	vec3 lightColor = light.color;

	vec3 dirPixelToLight;
	if (lightPosition == vec3(0, 0, 0)) dirPixelToLight = -lightDirection; // is directional light
	else dirPixelToLight = normalize(lightPosition - worldPosition); // is point light
	// else is spot light

	vec3 dirPixelToEye = normalize(engine.cameraPosition - worldPosition);


	float NdotL = max(dot(worldNormal, dirPixelToLight), 0.0);

	float specular = 0.0;
	if (NdotL > 0.0)
	{
		specular = 0;
	}

	return lightColor *  NdotL;

}





vec3 CookTorence(vec3 position, vec3 normal, float metallic, float smoothness) {

	vec3 lightPosition = light.position;
	vec3 lightDirection = light.direction;
	vec3 lightColor = light.color;

	vec3 dirPixelToLight;
	if (lightPosition == vec3(0, 0, 0)) dirPixelToLight = -lightDirection; // is directional light
	else dirPixelToLight = normalize(lightPosition - position); // is point light
	// else is spot light

	vec3 dirPixelToEye = normalize(engine.cameraPosition - position);

	// http://ruh.li/GraphicsCookTorrance.html

	// set important material values

	//float roughnessValue = 0.5; // 0 : smooth, 1: rough
	float roughnessValue = 1-smoothness;

	float F0 = 0.8; // fresnel reflectance at normal incidence

	//float k = 0.5; // fraction of diffuse reflection (specular reflection = 1 - k)
	float k = metallic;

	// do the lighting calculation for each fragment.
	float NdotL = max(dot(normal, dirPixelToLight), 0.0);

	float specular = 0.0;
	if (NdotL > 0.0)
	{

		// calculate intermediary values
		vec3 halfVector = normalize(dirPixelToLight + dirPixelToEye);
		float NdotH = max(dot(normal, halfVector), 1.0e-7);
		float NdotV = max(dot(normal, dirPixelToEye), 0.0); // note: this could also be NdotL, which is the same value
		float VdotH = max(dot(dirPixelToEye, halfVector), 0.0);
		float mSquared = roughnessValue * roughnessValue;

		// geometric attenuation
		float NH2 = 2.0 * NdotH;
		float g1 = (NH2 * NdotV) / VdotH;
		float g2 = (NH2 * NdotL) / VdotH;
		float geoAtt = min(1.0, min(g1, g2));

		// roughness (or: microfacet distribution function)
		// beckmann distribution function
		float r1 = 1.0 / (3.14 * mSquared * pow(NdotH, 4.0));
		float r2 = (NdotH * NdotH - 1.0) / (mSquared * NdotH * NdotH);
		float roughness = r1 * exp(r2);

		// fresnel
		// Schlick approximation
		float fresnel = pow(1.0 - VdotH, 5.0);
		fresnel *= (1.0 - F0);
		fresnel += F0;

		specular = (fresnel * geoAtt * roughness) / (NdotV * NdotL * 3.14);

	}

	return lightColor *  NdotL * (k + specular * (1.0 - k));

}


