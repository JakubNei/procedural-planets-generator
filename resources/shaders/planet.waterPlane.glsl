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
	return 128;
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


	// INTERPOLATE NEW PARAMS
	o.worldPos	= interpolate3D(	i[0].worldPos,	i[1].worldPos,	i[2].worldPos	); 
	o.modelPos	= interpolate3D(	i[0].modelPos,	i[1].modelPos,	i[2].modelPos	); 
	o.normal	= interpolate3D(	i[0].normal,	i[1].normal,	i[2].normal		); 
	o.uv		= interpolate2D(	i[0].uv,		i[1].uv,		i[2].uv			); 
	o.tangent	= interpolate3D(	i[0].tangent,	i[1].tangent,	i[2].tangent	); 

	//vec3 offset = o.worldPos - o.modelPos;
	
	//o.worldPos = offset + o.modelPos;

	o.modelPos = vec3( normalize(o.modelPos) * (param_radiusMin + param_baseHeightMapMultiplier/2) );

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







#define iGlobalTime (engine.totalElapsedSecondsSinceEngineStart)
#define iResolution (engine.screenSize)

// TAKEN FROM: https://www.shadertoy.com/view/Ms2SD1
/*
 * "Seascape" by Alexander Alekseev aka TDM - 2014
 * License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.
 * Contact: tdmaav@gmail.com
 */

const int NUM_STEPS = 8;
const float PI	 	= 3.1415;
const float EPSILON	= 1e-3;
#define EPSILON_NRM (0.1 / iResolution.x)

// sea
const int ITER_GEOMETRY = 3;
const int ITER_FRAGMENT = 5;
const float SEA_HEIGHT = 0.6;
const float SEA_CHOPPY = 4.0;
const float SEA_SPEED = 0.8;
const float SEA_FREQ = 0.16;
const vec3 SEA_BASE = vec3(0.1,0.19,0.22);
const vec3 SEA_WATER_COLOR = vec3(0.8,0.9,0.6);
#define SEA_TIME (1.0 + iGlobalTime * SEA_SPEED)
const mat2 octave_m = mat2(1.6,1.2,-1.2,1.6);

// math
mat3 fromEuler(vec3 ang) {
	vec2 a1 = vec2(sin(ang.x),cos(ang.x));
    vec2 a2 = vec2(sin(ang.y),cos(ang.y));
    vec2 a3 = vec2(sin(ang.z),cos(ang.z));
    mat3 m;
    m[0] = vec3(a1.y*a3.y+a1.x*a2.x*a3.x,a1.y*a2.x*a3.x+a3.y*a1.x,-a2.y*a3.x);
	m[1] = vec3(-a2.y*a1.x,a1.y*a2.y,a2.x);
	m[2] = vec3(a3.y*a1.x*a2.x+a1.y*a3.x,a1.x*a3.x-a1.y*a3.y*a2.x,a2.y*a3.y);
	return m;
}
float hash( vec2 p ) {
	float h = dot(p,vec2(127.1,311.7));	
    return fract(sin(h)*43758.5453123);
}
float noise( in vec2 p ) {
    vec2 i = floor( p );
    vec2 f = fract( p );	
	vec2 u = f*f*(3.0-2.0*f);
    return -1.0+2.0*mix( mix( hash( i + vec2(0.0,0.0) ), 
                     hash( i + vec2(1.0,0.0) ), u.x),
                mix( hash( i + vec2(0.0,1.0) ), 
                     hash( i + vec2(1.0,1.0) ), u.x), u.y);
}

// lighting
float diffuse(vec3 n,vec3 l,float p) {
    return pow(dot(n,l) * 0.4 + 0.6,p);
}
float specular(vec3 n,vec3 l,vec3 e,float s) {    
    float nrm = (s + 8.0) / (3.1415 * 8.0);
    return pow(max(dot(reflect(e,n),l),0.0),s) * nrm;
}

// sky
vec3 getSkyColor(vec3 e) {
    e.y = max(e.y,0.0);
    return vec3(pow(1.0-e.y,2.0), 1.0-e.y, 0.6+(1.0-e.y)*0.4);
}

// sea
float sea_octave(vec2 uv, float choppy) {
    uv += noise(uv);        
    vec2 wv = 1.0-abs(sin(uv));
    vec2 swv = abs(cos(uv));    
    wv = mix(wv,swv,wv);
    return pow(1.0-pow(wv.x * wv.y,0.65),choppy);
}

float map(vec3 p) {
    float freq = SEA_FREQ;
    float amp = SEA_HEIGHT;
    float choppy = SEA_CHOPPY;
    vec2 uv = p.xz; uv.x *= 0.75;
    
    float d, h = 0.0;    
    for(int i = 0; i < ITER_GEOMETRY; i++) {        
    	d = sea_octave((uv+SEA_TIME)*freq,choppy);
    	d += sea_octave((uv-SEA_TIME)*freq,choppy);
        h += d * amp;        
    	uv *= octave_m; freq *= 1.9; amp *= 0.22;
        choppy = mix(choppy,1.0,0.2);
    }
    return p.y - h;
}

float map_detailed(vec3 p) {
    float freq = SEA_FREQ;
    float amp = SEA_HEIGHT;
    float choppy = SEA_CHOPPY;
    vec2 uv = p.xz; uv.x *= 0.75;
    
    float d, h = 0.0;    
    for(int i = 0; i < ITER_FRAGMENT; i++) {        
    	d = sea_octave((uv+SEA_TIME)*freq,choppy);
    	d += sea_octave((uv-SEA_TIME)*freq,choppy);
        h += d * amp;        
    	uv *= octave_m; freq *= 1.9; amp *= 0.22;
        choppy = mix(choppy,1.0,0.2);
    }
    return p.y - h;
}

vec3 getSeaColor(vec3 p, vec3 n, vec3 l, vec3 eye, vec3 dist) {  
    float fresnel = clamp(1.0 - dot(n,-eye), 0.0, 1.0);
    fresnel = pow(fresnel,3.0) * 0.65;
        
    vec3 reflected = getSkyColor(reflect(eye,n));    
    vec3 refracted = SEA_BASE + diffuse(n,l,80.0) * SEA_WATER_COLOR * 0.12; 
    
    vec3 color = mix(refracted,reflected,fresnel);
    
    float atten = max(1.0 - dot(dist,dist) * 0.001, 0.0);
    color += SEA_WATER_COLOR * (p.y - SEA_HEIGHT) * 0.18 * atten;
    
    color += vec3(specular(n,l,eye,60.0));
    
    return color;
}

// tracing
vec3 getNormal(vec3 p, float eps) {
    vec3 n;
    n.y = map_detailed(p);    
    n.x = map_detailed(vec3(p.x+eps,p.y,p.z)) - n.y;
    n.z = map_detailed(vec3(p.x,p.y,p.z+eps)) - n.y;
    n.y = eps;
    return normalize(n);
}

float heightMapTracing(vec3 ori, vec3 dir, out vec3 p) {  
    float tm = 0.0;
    float tx = 1000.0;    
    float hx = map(ori + dir * tx);
    if(hx > 0.0) return tx;   
    float hm = map(ori + dir * tm);    
    float tmid = 0.0;
    for(int i = 0; i < NUM_STEPS; i++) {
        tmid = mix(tm,tx, hm/(hm-hx));                   
        p = ori + dir * tmid;                   
    	float hmid = map(p);
		if(hmid < 0.0) {
        	tx = tmid;
            hx = hmid;
        } else {
            tm = tmid;
            hm = hmid;
        }
    }
    return tmid;
}

// main
void mainImage(out vec4 fragColor, in vec2 fragCoord ) {


    // ray
    vec3 ori = vec3(1);
    vec3 dir = normalize(i.modelPos); 
    dir = normalize(dir);
    
    // tracing
    vec3 p;
    heightMapTracing(ori,dir,p);
    vec3 dist = p - ori;
    vec3 n = getNormal(p, dot(dist,dist) * EPSILON_NRM);
    vec3 light = normalize(vec3(0.0,1.0,0.8)); 
             
    // color
    vec3 color = mix(
        getSkyColor(dir),
        getSeaColor(p,n,light,dir,dist),
    	pow(smoothstep(0.0,-0.05,dir.y),0.3));
        
    // post
	fragColor = vec4(pow(color,vec3(0.75)), 1.0);
}










void main()
{

	vec4 color = vec4(0);	

	mainImage(color, gl_FragCoord.xy);

	out_color = color;
	//out_color = pow(color,engine.gammaCorrectionTextureRead);

}

