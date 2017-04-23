[include internal/include.all.shader]

[include shaders/include.planet.glsl]














#define iGlobalTime (engine.totalElapsedSecondsSinceEngineStart)
#define iResolution (engine.screenSize)

// MODIFIED SHADER FROM: https://www.shadertoy.com/view/Ms2SD1
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
float diffuse(vec3 normal,vec3 dirToLight,float p) {
    return pow(dot(normal,dirToLight) * 0.4 + 0.6,p);
}
float specular(vec3 normal,vec3 dirToLight,vec3 dirToCamera,float s) {    
    float nrm = (s + 8.0) / (3.1415 * 8.0);
    return pow(max(dot(reflect(dirToCamera,normal),dirToLight),0.0),s) * nrm;
}

// sky
vec3 getSkyColor(vec3 dirToCamera) {
    dirToCamera.y = max(dirToCamera.y,0.0);
    return vec3(1,1,1);
    return vec3(pow(1.0-dirToCamera.y,2.0), 1.0-dirToCamera.y, 0.6+(1.0-dirToCamera.y)*0.4);
}

// sea
float sea_octave(vec2 uv, float choppy) {
    uv += noise(uv);        
    vec2 wv = 1.0-abs(sin(uv));
    vec2 swv = abs(cos(uv));    
    wv = mix(wv,swv,wv);
    return pow(1.0-pow(wv.x * wv.y,0.65),choppy);
}


float map(vec2 uv) {
	float freq = SEA_FREQ;
    float amp = SEA_HEIGHT;
    float choppy = SEA_CHOPPY;
    
    float d, h = 0.0;    
    for(int i = 0; i < ITER_GEOMETRY; i++) {        
    	d = sea_octave((uv+SEA_TIME)*freq,choppy);
    	d += sea_octave((uv-SEA_TIME)*freq,choppy);
        h += d * amp;        
    	uv *= octave_m; freq *= 1.9; amp *= 0.22;
        choppy = mix(choppy,1.0,0.2);
    }
    //return -h;
    return -h;
}


float map_detailed(vec2 uv) {
    float freq = SEA_FREQ;
    float amp = SEA_HEIGHT;
    float choppy = SEA_CHOPPY;
    
    float d, h = 0.0;    
    for(int i = 0; i < ITER_FRAGMENT; i++) {        
    	d = sea_octave((uv+SEA_TIME)*freq,choppy);
    	d += sea_octave((uv-SEA_TIME)*freq,choppy);
        h += d * amp;        
    	uv *= octave_m; freq *= 1.9; amp *= 0.22;
        choppy = mix(choppy,1.0,0.2);
    }
    return -h;
}


vec3 getNormal(vec2 uv, float eps) {
    vec3 normal;
    normal.z = map_detailed(uv); 
    normal.x = map_detailed(vec2(uv.x+eps,uv.y)) - normal.z;
    normal.y = map_detailed(vec2(uv.x,uv.y+eps)) - normal.z;
    normal.z = eps;
    return normalize(normal);
}

vec2 getSeaUv(vec2 uv)
{
	return uv*100000;
}








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
	o.normal = (model.modelMatrix * vec4(in_normal,0)).xyz;

	vec2 uv = getSeaUv(in_uv);
	float height = map(uv);

	vec3 modelPos = in_position + o.normal * height;
	vec4 worldPos4 = model.modelMatrix * vec4(modelPos, 1);	
	vec3 worldPos3 = worldPos4.xyz / worldPos4.w;

	o.worldPos = worldPos3;
	o.modelPos = modelPos;
	o.tangent = (model.modelMatrix * vec4(in_tangent,0)).xyz;
	o.uv = in_uv;
	gl_Position = model.modelViewProjectionMatrix * vec4(modelPos,1);
}











[FragmentShader]
#line 194

in data {
	vec3 worldPos;
	vec3 modelPos;
	vec3 normal; 
	vec2 uv; 
	vec3 tangent;
} i;

layout(location = 0) out vec4 out_color;




vec3 getSeaColor(float height, vec3 normal, vec3 dirToLight, vec3 dirToCamera) {  
    
	//normal = normal.xzy;

    vec3 color = vec3(0);
    
    float fresnel = clamp(1.0 - dot(normal,-dirToCamera), 0.0, 1.0);
    fresnel = pow(fresnel,3.0) * 0.65;        
    vec3 reflected = getSkyColor(reflect(dirToCamera,normal));    
    vec3 refracted = SEA_BASE + diffuse(normal,dirToLight,80.0) * SEA_WATER_COLOR * 0.12; 
	color += mix(refracted,reflected,fresnel);    
    
    //DEBUG
	//color += getSkyColor(reflect(dirToCamera,normal));

    float dist = length(i.worldPos);
    //dist = 100;
    float atten = max(1.0 - dist * 0.001, 0.0); 
    //color += SEA_WATER_COLOR * (-height) * 0.18 * atten;
    
    //color += vec3(specular(normal,dirToLight,dirToCamera,60.0));
    
    return color;
}



vec3 normalMap(vec3 normal) {
	vec3 N = i.normal;
	vec3 T = i.tangent;
	vec3 T2 = T - N * dot(N, T); // Gram-Schmidt orthogonalization of T
	vec3 B = normalize(cross(N,T2));
	mat3 normalMatrix = mat3(T,B,N); // column0, column1, column2		
	return normalMatrix * normal;
}

void main()
{

	const float startAtCameraDist = 10000;

	const vec3 defaultColor = vec3(28,50,57)/255; //vec3(0.18,0.56,0.8);


    vec3 dirToCamera = normalize(vec3(0) - i.worldPos);
    vec3 dirToLight = normalize(light.position - i.worldPos);
    vec3 color = getSeaColor(0, i.normal, dirToLight, dirToCamera);


	float dist = length(i.worldPos);

	if(dist < startAtCameraDist) {

		vec2 uv = getSeaUv(i.uv);
		float height = map(uv);
		vec3 normalTangentSpace = getNormal(uv, 100 * EPSILON_NRM);

		//DEBUG
		//normal = vec3(0,0,1);

		vec3 normalWorldSpace = normalMap(normalTangentSpace);

		vec3 realColor = getSeaColor(height, normalWorldSpace, dirToLight, dirToCamera);

		color = mix(color, realColor, smoothstep(startAtCameraDist, startAtCameraDist/2, dist));

		//DEBUG
		//color = vec3(height);
		//color = normal;
	}

    // dustance to surface sea bed from water surface
    float distanceToSurface = GBufferDistanceOfCurrentFragmentToDepthBuffer();;

    float alpha = 0.05 + 0.95 * smoothstep(distanceToSurface, 0, 10);

    //d = float(linearDepth(gl_FragCoord.z) > 10000);
    //d = float(linearDepth(GBufferGetDepth(GBufferGetScreenCoord())) > 10000);


    {
        float e1 = -0.3;
        float e2 = -e1;
        float li = dot(dirToLight, i.normal);
        li = clamp(li, e1, e2);
        li = smoothstep(e1, e2, li);
        //li = 0.1 + 0.9 * li;
        color *= li;
    }

	color = pow(color,vec3(engine.gammaCorrectionTextureRead));
	vec4 color4 = vec4(color, alpha);
	out_color = color4;

    //DEBUG
    //out_color = vec4(vec3(GBufferGetDepth(gl_FragCoord.yx/engine.screenSize)),1);
    //out_color = vec4(vec3(gl_FragCoord.yx/engine.screenSize,1),1);
    //out_color = vec4(vec3(alpha),1);
    //out_color = vec4(hsv2rgb(vec3(d,1,1)),1);
    //out_color = vec4(dirToLight, 1);

}

