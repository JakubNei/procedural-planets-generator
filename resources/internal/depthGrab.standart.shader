


[VertexShader]

layout(location = 0) in vec3 in_position;

void main()
{
	
	gl_Position = model.modelViewProjectionMatrix * vec4(in_position,1);

}


[FragmentShader]



layout(location = 0) out float out_depth;


void main()
{
	out_depth = gl_FragCoord.z;
}

	