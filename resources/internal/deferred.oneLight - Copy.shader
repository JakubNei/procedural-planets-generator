

[VertexShader] // pass thru vertex shader

layout(location = 0) in vec3 in_position;

void main()
{
	gl_Position = vec4(in_position,1);
}









[FragmentShader]




float GetShadowValue(vec3 position) {

	//return 1;

	vec2 poissonDisk[4] = vec2[](
		vec2(-0.94201624, -0.39906216),
		vec2(0.94558609, -0.76890725),
		vec2(-0.094184101, -0.92938870),
		vec2(0.34495938, 0.29387760)
	);

	float visibility = 1;

	vec4 shadowPos = (shadowMap.viewProjectionMatrix * vec4(position, 1));
	vec2 shadowUV = shadowPos.xy;
	if (shadowUV.x >= 1 || shadowUV.x <= 0 || shadowUV.y >= 1 || shadowUV.y <= 0) return visibility;

	for (int i = 0; i < 4; i++) {
		float closestDepthFromLight = texture(shadowMap.level0, shadowUV + poissonDisk[i] / 700.0);
		float distanceToLight = shadowPos.z;
		if (closestDepthFromLight < distanceToLight - 0.0005) visibility -= 0.25;
	}


	// see if we are on the edge, and if so make a nice soft shadow with many many samples from shadowmap
	/*if (visibility > 0 && visibility < 1) {

		for (int i = 0; i < 4; i++) {
			float closestDepthFromLight = texture(shadowMap.level0, shadowUV + poissonDisk[i] / 700.0);
			float distanceToLight = shadowPos.z;
			if (closestDepthFromLight < distanceToLight - 0.0005) visibility -= 0.25;
		}

	}*/

	return visibility;
}


vec3 GetAmbientLight(vec3 normal) {
	return vec3(0.2);
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

	float roughnessValue = 0.6; // 0 : smooth, 1: rough
	//float roughnessValue = 1-smoothness;

	float F0 = 0.8; // fresnel reflectance at normal incidence

	float k = 0.5; // fraction of diffuse reflection (specular reflection = 1 - k)
	//float k = metallic;
	
   
	// do the lighting calculation for each fragment.
	float NdotL = max(dot(normal, dirPixelToLight), 0.0);
	
	float specular = 0.0;
	if(NdotL > 0.0)
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
		float r1 = 1.0 / ( 3.14 * mSquared * pow(NdotH, 4.0));
		float r2 =  (NdotH * NdotH - 1.0) / (mSquared * NdotH * NdotH);
		float roughness = r1 * exp(r2);
		
		
		// fresnel
		// Schlick approximation
		float fresnel = pow(1.0 - VdotH, 5.0);
		fresnel *= (1.0 - F0);
		fresnel += F0;
		
		specular = (fresnel * geoAtt * roughness) / (NdotV * NdotL * 3.14);	
		
	} 



	return lightColor *  NdotL * (k + specular * (1.0 - k) );

}





layout(location = 0) out vec4 out_color;

void main()
{

	vec2 screenCoord = gl_FragCoord.xy / engine.screenSize;

	GBufferPerPixel gBuffer = GetGBufferPerPixel();

	// skybox, just pass thru color
	if (gBuffer.normal == vec3(0, 0, 0)) {
		if(light.lightIndex == 1) out_color = vec4(gBuffer.color, 1);
		else out_color = vec4(0, 0, 0, 1);
		return;
	}


	vec3 color = GetAmbientLight(gBuffer.normal);
	float shadowValue = GetShadowValue(gBuffer.position);	
	if(shadowValue>0) color += shadowValue * CookTorence(gBuffer.position, gBuffer.normal, gBuffer.metallic, gBuffer.smoothness);

	out_color = vec4( gBuffer.color * color, 1);
	//out_color = vec4( gBuffer.color, 1);
	//out_color = vec4(vec3(shadowValue), 1);

	//float s=(1-texture(shadowMap.level0, screenCoord).x)*200;
	//out_color = vec4(vec3(s),1);
	//if(s==0) out_color = vec4(vec3(1),1);

	//if(NdotL > 0.0) out_color+=vec4(vec3(  NdotL +  pow(max(dot(dirPixelToEye, reflect(-dirPixelToLight,normal)),0),100)  ), 1); // phong
	//out_color = vec4(max(0,dot(,1);
	//out_color = vec4(finalColor,1);
	//out_color = vec4(lights[0].position,1 );
	//out_color = normalize(dirPixelToLight)/2+0.5;
	//out_color = normalize(normal)/2+0.5;
	//out_color = normalize(normal) - normalize(dirPixelToLight);
	//out_color = vec4(gBuffer.position,1);
	//out_color = vec4(gBuffer.normal,1);
	//out_color = vec4(color,1);	
	//float d=dot(normalize(normal),normalize(lights[0].position-position));
	//out_color = vec4(d,d,d );
	//out_color=vec4(1,1,0,1);
	//out_color=vec4(engine.cameraPosition,1);
	//float depth=(1-texture(shadowMap.level0,screenCoord).x) * 10; out_color=vec4(depth,depth,depth,1);
	
}
	

