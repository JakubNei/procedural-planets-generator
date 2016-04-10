

[VertexShader] // pass thru vertex shader

layout(location = 0) in vec3 in_position;

void main()
{
	gl_Position = vec4(in_position,1);
}









[FragmentShader]

int IsInShadow(vec2 screenCoord) {	


	const float zOffset = -0.00005;

	vec3 worldPos = texture(gBufferUniform.position, screenCoord).xyz;
	//vec3 worldNormal = texture(gBufferUniform.normal, screenCoord).xyz;
	//if( PhongShaderLightIntensity(worldPos, worldNormal) < 0) return 1;

	vec4 shadowPos = (shadowMap.viewProjectionMatrix * vec4(worldPos, 1));
	vec2 shadowUV = shadowPos.xy;
	if (shadowUV.x >= 1 || shadowUV.x <= 0 || shadowUV.y >= 1 || shadowUV.y <= 0) return 0;

	float closestDepthFromLight = texture(shadowMap.level0, shadowUV).x;
	float distanceToLight = shadowPos.z;
	if (closestDepthFromLight < distanceToLight + zOffset) return 1;

	return 0;
}



float GetShadowValue2(vec2 screenCoord) {
	
	if(light.hasShadows<1) return 1; // return lit up value if we have no shadows

	vec3 worldPos = texture(gBufferUniform.position, screenCoord).xyz;
	float distancePixelToCamera = distance(worldPos, engine.cameraPosition);

	float visibility = 1;

	visibility = 1;

	const float PI = 3.14159265359;
	const float TWO_PI = 3.14159265359 * 2.0;

	const int angleSteps = 16;
	const float angleAdd = TWO_PI / angleSteps;

	const int radiusSteps = 5;
	float radiusStart = 0.05 / distancePixelToCamera;
	float radiusEnd = 0.15 / distancePixelToCamera;
	float radiusAdd = (radiusEnd - radiusStart) / radiusSteps;

	const float shadowAdd = 1.0 / angleSteps / radiusSteps;

	//for (float radius = radiusStart; radius < radiusEnd; radius += radiusAdd) {
	//for (int i = 0; i < radiusSteps; i++) {
	for (int i = radiusSteps-1; i >= 0; i--) {
		float radius = (radiusStart + radiusAdd*i);
		for (float angle = 0.0; angle < TWO_PI; angle += angleAdd) {
			vec2 shadowMapUV = vec2( sin(angle) * radius, cos(angle) * radius );
			if(IsInShadow(screenCoord+shadowMapUV)>0) visibility-=shadowAdd;
		}
		if(visibility==1) return visibility; // return if there is no change in shadows in largest radius
	}

	return visibility;
}




float GetShadowValue(vec3 position) {

	//return 1;

	vec2 poissonDisk[4] = vec2[](
		vec2(-0.94201624, -0.39906216),
		vec2(0.94558609, -0.76890725),
		vec2(-0.094184101, -0.92938870),
		vec2(0.34495938, 0.29387760)
	);

	float visibility = 1;

	const float zOffset = -0.00005;


	vec4 shadowPos = (shadowMap.viewProjectionMatrix * vec4(position, 1));
	vec2 shadowUV = shadowPos.xy;
	if (shadowUV.x >= 1 || shadowUV.x <= 0 || shadowUV.y >= 1 || shadowUV.y <= 0) return visibility;

	for (int i = 0; i < 4; i++) {
		float closestDepthFromLight = texture(shadowMap.level0, shadowUV + poissonDisk[i] / 700.0).x;
		float distanceToLight = shadowPos.z;
		if (closestDepthFromLight < distanceToLight + zOffset) visibility -= 0.25;
	}


	// see if we are on the edge, and if so make a nice soft shadow with many many samples from shadowmap
	if (visibility > 0 && visibility < 1) {		

		visibility = 1;

		const float PI = 3.14159265359;
		const float TWO_PI = 3.14159265359 * 2.0;

		const int angleSteps = 4;
		const float angleAdd = TWO_PI / angleSteps;

		const float radiusStart = 0.0004;
		const float radiusEnd = 0.0014;
		const float radiusSteps = 10;
		const float radiusAdd = (radiusEnd - radiusStart) / radiusSteps;

		const float shadowAdd = -1.0 / angleSteps / radiusSteps;

		//for (float radius = radiusStart; radius < radiusEnd; radius += radiusAdd) {
		for (int i = 0; i < radiusSteps; i++) {
			float radius = radiusStart + radiusAdd*i;
			for (float angle = 0.0; angle < TWO_PI; angle += angleAdd) {

				vec2 shadowMapUV = shadowUV;
				shadowMapUV.x += sin(angle) * radius;
				shadowMapUV.y += cos(angle) * radius;

				float closestDepthFromLight = texture(shadowMap.level0, shadowMapUV).x;
				float distanceToLight = shadowPos.z;
				if (closestDepthFromLight < distanceToLight + zOffset) visibility += shadowAdd;
			}
		}

	}

	return visibility;
}


vec3 GetAmbientLight(vec3 normal) {
	return vec3(0.2);
}



layout(location = 0) out vec4 out_color;

void main()
{

	vec2 screenCoord = gl_FragCoord.xy / engine.screenSize;

	GBufferPerPixel gBuffer = GetGBufferPerPixel(screenCoord);

	vec3 color;

	// skybox, just pass thru color
	if (gBuffer.normal==vec3(0,0,0)) {
		if(light.lightIndex == 0) out_color = vec4(gBuffer.color, 1);
		else out_color = vec4(0, 0, 0, 1);
		//out_color = vec4(gBuffer.color, 1);
		return;
	}

	//if(light.lightIndex == 0) color = GetAmbientLight(gBuffer.normal);

	//float shadowValue = GetShadowValue(gBuffer.position);
	float shadowValue = GetShadowValue2(screenCoord);
	//if(shadowValue>0) color += shadowValue * CookTorence(gBuffer.position, gBuffer.normal, gBuffer.metallic, gBuffer.smoothness);
	if(shadowValue>0) color += gBuffer.color * shadowValue * PhongShader(gBuffer.position, gBuffer.normal, gBuffer.metallic, gBuffer.smoothness);

	out_color = vec4( color, 1);
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
	//out_color = vec4(vec3(1- texture(gBufferUniform.depthBuffer, screenCoord).x*10000.0), 1);
	//float depth=(1-texture(shadowMap.level0,screenCoord).x) * 10; out_color=vec4(depth,depth,depth,1);
	
}
	


