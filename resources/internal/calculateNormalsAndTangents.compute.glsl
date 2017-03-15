#version 430
[ComputeShader]


uniform int param_indiciesCount;




struct vec2_struct
{
    float x;
    float y;
};
vec2 fromStruct(vec2_struct p)
{
	return vec2(p.x, p.y);
}
vec2_struct toStruct(vec2 p)
{
	vec2_struct s;
	s.x = p.x;
	s.y = p.y;
	return s;
}


struct vec3_struct
{
    float x;
    float y;
    float z;
};
vec3 fromStruct(vec3_struct p)
{
	return vec3(p.x, p.y, p.z);
}
vec3_struct toStruct(vec3 p)
{
	vec3_struct s;
	s.x = p.x;
	s.y = p.y;
	s.z = p.z;
	return s;
}



layout( binding=0 ) buffer buffer1 {
    vec3_struct positions[];
};
layout( binding=1 ) buffer buffer2 {
    vec3_struct normals[];
};
layout( binding=2 ) buffer buffer3 {
    vec3_struct tangents[];
};
layout( binding=3 ) buffer buffer4 {
    vec2_struct uvs[];
};
layout( binding=4 ) buffer buffer5 {
    int indicies[];
};


layout(local_size_x = 1, local_size_y = 1, local_size_z = 1) in;
 





void main() {
	
	int id = int(gl_GlobalInvocationID.x);	

	vec3 normal = vec3(0);
	vec3 tangent = vec3(0);
	int count = 0;

	for(int i=0; i<param_indiciesCount; i+=3)
	{

		int indexA = indicies[i];
		int indexB = indicies[i+1];
		int indexC = indicies[i+2];

		if(indexA == id || indexB == id || indexC== id) {

			vec3 posA = fromStruct(positions[indexA]);
			vec3 posB = fromStruct(positions[indexB]);
			vec3 posC = fromStruct(positions[indexC]);

			vec3 posAToB = posB - posA;
			vec3 posAToC = posC - posA;

			normal += normalize(cross(normalize(posAToB), normalize(posAToC)));


			vec2 uvA = fromStruct(uvs[indexA]);
			vec2 uvB = fromStruct(uvs[indexB]);
			vec2 uvC = fromStruct(uvs[indexC]);

			vec2 uvAToB = uvB - uvA;
			vec2 uvAToC = uvC - uvA;

			float r = 1.0f / (uvAToB.x * uvAToC.y - uvAToB.y * uvAToC.x);
			vec3 t = (posAToB * uvAToC.y - posAToC * uvAToB.y) * r;

			tangent += t;


			count++;

		}


	}

	if(count > 0) {
		normal = normalize(normal/count);
		tangent = normalize(tangent/count);
	}

	normals[id] = toStruct(normal);
	tangents[id] = toStruct(tangent);
}
