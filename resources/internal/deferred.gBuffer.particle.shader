

[VertexShader]

layout(location = 0) in vec3 currentPositionH;
layout(location = 1) in vec3 currentVelocityH;
layout(location = 2) in vec3 currentAccelerationH;
layout(location = 3) in float currentLifeTimeH;
layout(location = 4) in vec4 startColorH;
layout(location = 5) in vec4 endColorH;
layout(location = 6) in float startSizeH;
layout(location = 7) in float endSizeH;
layout(location = 8) in float startLifeTimeH;

out data {
	vec3 position;
	vec4 color;
} o;

void main()
{
	


	if(currentLifeTimeH<0.1) {

		gl_PointSize = 0;
		o.color = vec4(0,0,1,1);

	} else {

		vec3 in_position = currentPositionH;

		gl_Position = model.modelViewProjectionMatrix * vec4(in_position,1);

		vec4 p = (model.modelMatrix * vec4(in_position, 1));
		o.position = p.xyz / p.w;
		

		float partInLife = currentLifeTimeH/startLifeTimeH; // 1 at start , under 0 at death

		o.color = mix(endColorH,startColorH,partInLife);		
		float size = mix(endSizeH,startSizeH,partInLife);

		//o.color = vec4(0,0,1,1);

		gl_PointSize = size/gl_Position.z;
		
	}
}



[FragmentShader]

in data {
	vec3 position;
	vec4 color; 
} i;

layout(location = 0) out vec4 out_color;
layout(location = 1) out vec3 out_position;
layout(location = 2) out vec4 out_normal;
layout(location = 3) out vec4 out_data;

void main()
{


	vec2 uv = gl_PointCoord;

	uv = vec2( uv.x, 1-uv.y);


	vec3 pixelToCamera = engine.cameraPosition - i.position;
	float pixelToCameraDist = length(pixelToCamera);
	

	out_color = i.color * texture(material.albedoTexture, uv);
	//out_color = vec4(uv.x, uv.y, 0, 0);


	out_position = i.position;



	//out_normal = normalize(pixelToCamera);
	out_normal = vec4(0,0,0,out_color.a);
	


	//out_normal = i.normal;
	out_data = vec4(material.metallic, material.smoothness, 0, 0);


	//LightData light = lights[0];
	//out_color = vec3( dot(out_normal, normalize(lights[0].position-out_position)) );
	//out_color = vec3(   pow(max(dot(normalize(engine.cameraPosition-out_position), reflect( normalize(lights[0].position-out_position),out_normal)),0), 100) );
}

	