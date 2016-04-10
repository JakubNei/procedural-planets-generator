vec3 ambientLight = vec3(0.2);

vec3 CookTorence(vec3 position, vec3 normal) {

	LightData light = lights[0];


	vec3 dirPixelToEye = normalize(engine.cameraPosition - position);
	vec3 dirPixelToLight = normalize(light.position - position);

	// http://ruh.li/GraphicsCookTorrance.html

	// set important material values
	float roughnessValue = 0.6; // 0 : smooth, 1: rough
	float F0 = 0.8; // fresnel reflectance at normal incidence
	float k = 0.5; // fraction of diffuse reflection (specular reflection = 1 - k)
	
   
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



	return ambientLight + light.color *  NdotL * (k + specular * (1.0 - k) );

}



[VertexShader]

layout(location = 0) in vec3 in_position;
layout(location = 1) in vec3 in_normal;
layout(location = 2) in vec3 in_tangent;
layout(location = 3) in vec2 in_uv;
// in mat4 in_modelMatrix; // instanced rendering



void main()
{
	
	gl_Position = vec4(in_position,1);

}


[FragmentShader]


layout(location = 0) out vec4 out_color;

void main()
{
	vec2 screenCoord = gl_FragCoord.xy / engine.screenSize;
	
	vec3 color = texture(gBuffer.color, screenCoord).xyz;
	vec3 position = texture(gBuffer.position, screenCoord).xyz;
	vec3 normal = normalize(texture(gBuffer.normal, screenCoord).xyz);
	vec2 uv = texture(gBuffer.uv, screenCoord).xy;



	// is skybox
	//if(normal==vec3(0,0,0)) discard;
	//if(length(normal)<0.1) {
	if(normal==vec3(0,0,0)) {
		out_color = vec4(0,0,0.5,1);
		return;
	}

	


	
	







	out_color =  vec4( color * CookTorence(position, normal)  , 1);


	//float s=(1-texture(shadowMap.level0, screenCoord).x)*200;
	//out_color = vec4(vec3(s),1);
	//if(s==0) out_color = vec4(vec3(1),1);

	//if(NdotL > 0.0) out_color+=vec4(vec3(  NdotL +  pow(max(dot(dirPixelToEye, reflect(-dirPixelToLight,normal)),0),100)  ), 1); // phong
	//out_color = vec4(max(0,dot(,1);
	//out_color = vec4(finalColor,1);
	//out_color = light.position;
	//out_color = normalize(dirPixelToLight)/2+0.5;
	//out_color = normalize(normal)/2+0.5;
	//out_color = normalize(normal) - normalize(dirPixelToLight);
	//out_color = vec4(position,1);
	//out_color = vec4(normal,1);
	//out_color = vec4(color,1);	
	//float d=dot(normalize(normal),normalize(lights[0].position-position));
	//out_color = vec4(d,d,d );
	//out_color=vec4(1,1,0,1);

}
	
