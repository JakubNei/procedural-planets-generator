[include internal/include.all.shader]

[include shaders/include.planet.glsl]


[VertexShader]

layout(location = 0) in vec3 in_position;
layout(location = 1) in vec3 in_normal;
layout(location = 2) in vec3 in_tangent;
layout(location = 3) in vec2 in_uv;

out data {
	vec3 worldPos;
	vec3 modelPos;
	vec3 normal; 
	vec2 uv; 
	vec3 tangent;
} o;

void main()
{
	vec3 modelPos = in_position;
	vec4 worldPos4 = model.modelMatrix * vec4(modelPos, 1);	
	vec3 worldPos3 = worldPos4.xyz / worldPos4.w;

	o.worldPos = worldPos3;
	o.modelPos = modelPos;
	o.normal = (model.modelMatrix * vec4(in_normal,0)).xyz;
	o.tangent = (model.modelMatrix * vec4(in_tangent,0)).xyz;
	o.uv = in_uv;
	gl_Position = model.modelViewProjectionMatrix * vec4(modelPos,1);
}








	

[TessControlShader]

layout(vertices = 3) out;	

in data {
	vec3 worldPos;
	vec3 modelPos;
	vec3 normal; 
	vec2 uv; 
	vec3 tangent;
} i[];

out data {
	vec3 worldPos;
	vec3 modelPos;
	vec3 normal; 
	vec2 uv; 
	vec3 tangent;		
} o[];

int closestPowerOf2(float a) {
	//return pow(2, ceil(log(a)/log(2)));
	if(a>512) return 512;
	if(a>256) return 256;
	if(a>128) return 128;
	if(a>64) return 64;
	if(a>32) return 32;
	if(a>16) return 16;
	if(a>8) return 8;
	if(a>4) return 4;
	if(a>2) return 2;
	return 1;
}
int tessLevel(float d1, float d2) {		
	float d=(d1+d2)/2;
	float r= 1000000/d;
	return closestPowerOf2(r);
}

void main() {

	gl_out[gl_InvocationID].gl_Position = gl_in[gl_InvocationID].gl_Position;

	// COPY OVER PARAMS
	o[gl_InvocationID].worldPos = i[gl_InvocationID].worldPos;
	o[gl_InvocationID].modelPos = i[gl_InvocationID].modelPos;
	o[gl_InvocationID].normal 	= i[gl_InvocationID].normal;
	o[gl_InvocationID].uv 		= i[gl_InvocationID].uv;
	o[gl_InvocationID].tangent 	= i[gl_InvocationID].tangent;

	// TESS LEVEL BASED ON EYE DISTANCE
	float d0=distance(i[0].worldPos,vec3(0));
	float d1=distance(i[1].worldPos,vec3(0));
	float d2=distance(i[2].worldPos,vec3(0));	

	gl_TessLevelOuter[0] = tessLevel(d1,d2);
	gl_TessLevelOuter[1] = tessLevel(d0,d2);
	gl_TessLevelOuter[2] = tessLevel(d0,d1);
	//gl_TessLevelOuter[3] = tess;

	gl_TessLevelInner[0] = gl_TessLevelOuter[2];
	//gl_TessLevelInner[1] = tess;

	//DEBUG
	//gl_TessLevelInner[0] = gl_TessLevelOuter[0] = gl_TessLevelOuter[1] = gl_TessLevelOuter[2] = 2;
}





[TessEvaluationShader]

layout(triangles, equal_spacing, ccw) in;

in data {
	vec3 worldPos;
	vec3 modelPos;
	vec3 normal; 
	vec2 uv; 
	vec3 tangent;
} i[];

out data {
	vec3 worldPos;
	vec3 modelPos;
	vec3 normal; 
	vec2 uv; 
	vec3 tangent;
} o;

vec2 interpolate2D(vec2 v0, vec2 v1, vec2 v2) {
	return vec2(gl_TessCoord.x)*v0 + vec2(gl_TessCoord.y)*v1 + vec2(gl_TessCoord.z)*v2;
}
vec2 interpolate2D(vec2 v0, vec2 v1, vec2 v2, vec2 v3) {
	return mix(  mix(v0,v1,gl_TessCoord.x),  mix(v2,v3,gl_TessCoord.x),  gl_TessCoord.y);
}
vec3 interpolate3D(vec3 v0, vec3 v1, vec3 v2) {
	return vec3(gl_TessCoord.x)*v0 + vec3(gl_TessCoord.y)*v1 + vec3(gl_TessCoord.z)*v2;
}
vec3 interpolate3D(vec3 v0, vec3 v1, vec3 v2, vec3 v3) {
	return mix(  mix(v0,v1,gl_TessCoord.x),  mix(v2,v3,gl_TessCoord.x),  gl_TessCoord.y);
}
vec4 interpolate4D(vec4 v0, vec4 v1, vec4 v2) {
	return vec4(gl_TessCoord.x)*v0 + vec4(gl_TessCoord.y)*v1 + vec4(gl_TessCoord.z)*v2;
}
vec4 interpolate4D(vec4 v0, vec4 v1, vec4 v2, vec4 v3) {
	return mix(  mix(v0,v1,gl_TessCoord.x),  mix(v2,v3,gl_TessCoord.x),  gl_TessCoord.y);
}


void main()
{

	float radius = float(param_radiusMin);

	// INTERPOLATE NEW PARAMS
	o.worldPos	= interpolate3D(	i[0].worldPos,	i[1].worldPos,	i[2].worldPos	); 
	o.modelPos	= interpolate3D(	i[0].modelPos,	i[1].modelPos,	i[2].modelPos	); 
	o.normal	= interpolate3D(	i[0].normal,	i[1].normal,	i[2].normal		); 
	o.uv		= interpolate2D(	i[0].uv,		i[1].uv,		i[2].uv			); 
	o.tangent	= interpolate3D(	i[0].tangent,	i[1].tangent,	i[2].tangent	); 

	//vec3 offset = o.worldPos - o.modelPos;
	
	//o.worldPos = offset + o.modelPos;

	o.modelPos = normalize(o.modelPos);

	vec4 worldPos4 = model.modelMatrix * vec4(o.modelPos, 1);
	vec3 worldPos3 = worldPos4.xyz / worldPos4.w;
	o.worldPos = worldPos3;

	gl_Position = model.modelViewProjectionMatrix * vec4(o.modelPos,1);

}












[FragmentShader]

in data {
	vec3 worldPos;
	vec3 modelPos;
	vec3 normal; 
	vec2 uv; 
	vec3 tangent;
} i;

layout(location = 0) out vec4 out_color;


void main()
{

	vec3 color = vec3(0,0,1);	

	out_color = vec4(pow(color,vec3(engine.gammaCorrectionTextureRead)),1);

}

	