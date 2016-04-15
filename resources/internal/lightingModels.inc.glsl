

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


