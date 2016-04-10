
#version 400 core

struct EngineData {
	mat4 projectionMatrix;
	mat4 viewMatrix;
	mat4 modelMatrix;
	mat4 modelViewMatrix;
	mat4 modelViewProjectionMatrix;
	int numberOfLights;
};

uniform EngineData engine;


struct LightData {
	vec3 color;
	vec3 position; // position == 0,0,0 => directional light 
	vec3 direction; // direction == 0,0,0 => point light
	float spotExponent; 
	float sportCutOff;
};


uniform LightData lights[MAX_NUMBER_OF_LIGHTS];





	
	
// LIGHTING
/*
vec3 applyLighting(vec3 worldPos, vec3 normal, vec3 albedo, float specularPower) {

	vec3 pixelToCamera = normalize(worldPos-EyePosition); 
	vec3 result=albedo*ambientLight;

	for(int i=0; i<lightsNum; i++) {
		Light light=lights[i];

		vec3 lightToPixel;
		float koef=1;

		if(light.position==vec3(0,0,0)) {
			lightToPixel=normalize(-light.direction);
		} else {
			lightToPixel=normalize(light.position-worldPos); 
		}
			
		if(light.cutOff>0) {
			float spotFactor = dot(lightToPixel,normalize(light.direction));																									
			if(spotFactor > light.cutOff) {
				koef *= (1.0 - (1.0 - spotFactor) *
						1.0 / (1.0 - light.cutOff ));	
				koef=max(koef,1.0);
			} else {
				koef = 0; 
			}
		}
					
		float diffuseFactor = max(0,dot(normal,lightToPixel)); 
		float specularFactor = 0;

		if(specularPower>0) {				
			float specularFactor=clamp(pow(max(0,dot(pixelToCamera,reflect(lightToPixel,normal))),specularPower),0,1);
		}

		if(light.attenuation>0 && koef>0) {
			float distance=distance(light.position,worldPos);
			float attenuation=light.attenuation*distance*distance;	
			koef/=max(attenuation,1);	
		}

		result+= ( light.color*albedo*diffuseFactor - light.color*albedo*specularFactor ) *koef;
	}

	return result;
}*/



[VertexShader]

in vec3 in_position;
in vec4 in_normal;
in vec4 in_tangent;
in vec2 in_uv;
// in mat4 in_modelMatrix; // instanced rendering

out data {
	vec3 normal; 
	vec3 worldPos;
	vec2 uv; 
	vec3 tangent;
} o;


void main()
{
	
	gl_Position = engine.modelViewProjectionMatrix * vec4(in_position,1.0);

	o.normal = (engine.modelMatrix * in_normal).xyz;

}


[FragmentShader]

in data {
	vec3 normal; 
	vec3 worldPos;
	vec2 uv; 
	vec3 tangent;
} i;

layout(location = 0) out vec4 color;

void main()
{
	float dot = dot(normalize(i.normal), vec3(1,1,0));
	color = vec4(lights[0].color,1);
}