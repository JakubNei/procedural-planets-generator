// help from http://prideout.net/archive/bloom/

[include internal/prependAll.shader]

[VertexShader] // pass thru vertex shader

layout(location = 0) in vec3 in_position;
layout(location = 1) in vec3 in_normal;
layout(location = 2) in vec3 in_tangent;
layout(location = 3) in vec2 in_uv;

out data {
	vec2 uv; 
} o;

void main()
{
	o.uv = in_uv;
	gl_Position = vec4(in_position,1);
}


[FragmentShader]

in data {
	vec2 uv; 
} i;

layout(location = 0) out vec4 out_color;


//http://www.walterzorn.de/en/grapher/grapher_e.htm
// 0.5*exp(-( x*x / pow(6/4, 2) ))
float gausian(float x, float totalWidth) 
{
	return exp(-( (x*x)/(totalWidth/4)*(totalWidth/4) ));	
}



void main()
{


	vec2 thisPixelScreenCoord = i.uv;

	GBufferPerPixel gBuffer = GetGBufferPerPixel(thisPixelScreenCoord);


	vec3 col = vec3(gBuffer.final);

	// skybox, just pass thru color
	/*
	if (gBuffer.normal == vec3(0, 0, 0)) {
		out_color = vec4(gBuffer.color, 1);
		return;
	}
	*/

	//out_color = vec4(pow(gBuffer.final,vec3(gBuffer.depth)), 1);
	//out_color = vec4(gBuffer.final, 1);
	//out_color = vec4(testColor, 1);	


	vec3 final;
	float intensity;
	vec2 screenCoord;


	#define ADD(X, Y, MULT) \
		screenCoord = thisPixelScreenCoord + vec2(xScale*X, yScale*Y); \
		if( \
			any(greaterThan(screenCoord, vec2(1))) == false && \
			any(lessThan(screenCoord, vec2(0))) == false \
		) { \
			final = textureLod(gBufferUniform.final, screenCoord, 2).xyz; \
			if(any(greaterThan(final, vec3(1)))) { \
				col += final * MULT; \
			} \
		} \


	float scale = 1 * 2 *2 * 2;
	float xScale = scale/engine.screenSize.x;
	float yScale = scale/engine.screenSize.y;

	int size = 6;

	for(int x=-size; x<size; x++) {
		for(int y=-size; y<size; y++) {
			float v =  1 - sqrt(x*x + y*y) / sqrt(size * size * 5);
			v = clamp(v,0,1) * 0.01;
			ADD(x,y,v);
		}
	}

	/*
	ADD(0,+1,1)
	ADD(0,-1,1)
	ADD(+1,0,1)
	ADD(-1,0,1)
	ADD(+1,+1,0.5)
	ADD(-1,+1,0.5)
	ADD(+1,-1,0.5)		
	ADD(-1,-1,0.5)
	*/


	out_color = vec4(col, 1);

	//DEBUG
	//out_color = vec4(1,0,0, 1);
		
}
	

