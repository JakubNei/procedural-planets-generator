

[VertexShader]

layout(location = 0) in vec3 in_position;
layout(location = 1) in vec3 in_normal;
layout(location = 2) in vec3 in_tangent;
layout(location = 3) in vec2 in_uv;
// in mat4 in_modelMatrix; // instanced rendering

out data {
	vec3 position;
	vec3 normal; 
	vec2 uv; 
	vec3 tangent;
} o;

void main()
{
	
	gl_Position = model.modelViewProjectionMatrix * vec4(in_position,1);

	vec4 p = (model.modelMatrix * vec4(in_position, 1));
	o.position = p.xyz / p.w;
	//o.position = in_position;

	o.uv = in_uv;

	o.normal = normalize((model.modelMatrix * vec4(in_normal,0)).xyz);
	//o.normal = in_normal;

	o.tangent = normalize((model.modelMatrix * vec4(in_tangent,0)).xyz);
}


[FragmentShader]

in data {
	vec3 position;
	vec3 normal; 
	vec2 uv; 
	vec3 tangent;
} i;

layout(location = 0) out vec4 out_color;
layout(location = 1) out vec3 out_position;
layout(location = 2) out vec3 out_normal;
layout(location = 3) out vec4 out_data;

void main()
{



	vec2 uv = vec2( i.uv.x, 1-i.uv.y);


	vec3 N = i.normal;
	vec3 T = i.tangent;
	vec3 T2 = T - N * dot(N, T); // Gram-Schmidt orthogonalization of T
	vec3 B = normalize(cross(N,T2));
	//if (dot(B2, B) < 0.0f) B2 *= -1.0f;
	mat3 normalMatrix = mat3(-T,B,N); // column0, column1, column2
	//mat3 normalMatrix = mat3(-T,B,N); //works // column0, column1, column2

	vec3 pixelToCamera = engine.cameraPosition - i.position;
	float pixelToCameraDist = length(pixelToCamera);
	
	//paralax mapping
	if(material.useParallaxMapping) {	
	//if(false) {	
		const float scale = 0.02; // 0.05
		float numSteps = 1 + 64* smoothstep(100, 0, pixelToCameraDist);
		float step =  1.0 / numSteps; 
		
		vec3 pixelToCameraInTangentSpace = normalize(transpose(normalMatrix) * normalize(pixelToCamera));
		vec2 uvDelta = pixelToCameraInTangentSpace.xy * scale / (numSteps * pixelToCameraInTangentSpace.z);
		float effectHeight = 1.0;
		vec2 newUV = uv;
		float height = texture(material.depthMap, newUV).r;
		while (height < effectHeight) {
			effectHeight -= step;
			newUV += uvDelta;
			height = texture(material.depthMap, newUV).r;
		}
		uv = newUV;
	}
	out_color = material.albedo * texture(material.albedoTexture, uv);
	//out_color = vec4(0,1,0,1);



	//out_position = i.position-vec3(0,0,10);
	out_position = i.position;


	//normal mapping
	if(material.useNormalMapping) {
	//if(false) {
		vec3 newNormal =  normalize(texture(material.normalMap, uv).xyz*2.0-1.0);
		//out_normal = normalize( N*newNormal.z + B*newNormal.x + T*newNormal.y );
		out_normal = normalize( normalMatrix * newNormal );
	} else {
		out_normal = normalize(i.normal);
	}


	//out_normal = i.normal;
	out_data = vec4(material.metallic, material.smoothness, 0, 0);


	//LightData light = lights[0];
	//out_color = vec3( dot(out_normal, normalize(lights[0].position-out_position)) );
	//out_color = vec3(   pow(max(dot(normalize(engine.cameraPosition-out_position), reflect( normalize(lights[0].position-out_position),out_normal)),0), 100) );
}

	