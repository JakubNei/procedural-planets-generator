// help from http://gamedev.stackexchange.com/questions/53377/reinhard-tone-mapping-and-color-space

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


/// <summary>
/// Gets the luminance value for a pixel.
/// <summary>
float GetLuminance (vec3 rgb)
{
	// ITU-R BT.709 primaries
	//return (0.2126 * rgb.x) + (0.7152 * rgb.y) + (0.0722 * rgb.z);
	return dot(rgb, vec3(0.2126f, 0.7152f, 0.0722f));
}


/// <summary>
/// Convert an sRGB pixel into a CIE xyY (xy = chroma, Y = luminance).
/// <summary>
vec3 RGB2xyY (vec3 rgb)
{
	const mat3 RGB2XYZ = mat3(0.4124, 0.3576, 0.1805,
							  0.2126, 0.7152, 0.0722,
							  0.0193, 0.1192, 0.9505);
	vec3 XYZ = RGB2XYZ * rgb;
	
	// XYZ to xyY
	return vec3(XYZ.x / (XYZ.x + XYZ.y + XYZ.z),
				XYZ.y / (XYZ.x + XYZ.y + XYZ.z),
				XYZ.y);
}


/// <summary>
/// Convert a CIE xyY value into sRGB.
/// <summary>
vec3 xyY2RGB (vec3 xyY)
{
	// xyY to XYZ
	vec3 XYZ = vec3((xyY.z / xyY.y) * xyY.x,
					xyY.z,
					(xyY.z / xyY.y) * (1.0 - xyY.x - xyY.y));

	const mat3 XYZ2RGB = mat3(3.2406, -1.5372, -0.4986,
                              -0.9689, 1.8758, 0.0415, 
                              0.0557, -0.2040, 1.0570);
	
	return XYZ2RGB * XYZ;
}


float FindMaxLuminance() {

	float maxLuminance = 0;
	
	float d = 0.25f;

	for(float x=0; x<1; x+=d) {
		for(float y=0; y<1; y+=d) {
			vec2 screenCoord = i.uv + vec2(x, y);		
			vec3 color = textureLod(gBufferUniform.final, screenCoord, 8).xyz;
			float luminance = RGB2xyY(color).z;
			if(luminance > maxLuminance) maxLuminance = luminance;
		}
	}

	return maxLuminance;
}


void main()
{


	vec2 screenCoord = i.uv;

	vec3 thisPixel = textureLod(gBufferUniform.final, screenCoord, 0).xyz;

	float luminance = GetLuminance(thisPixel);
	float scale = 1;
	vec3 averagePixel = textureLod(gBufferUniform.final, screenCoord, 1000).xyz;
	float avgLuminance = GetLuminance(averagePixel);
	float maxLuminance = FindMaxLuminance();

	// Ld(x,y) = (L(x,y) + (1.0 + (L(x,y) / Lwhite^2))) / (1 + L(x,y))
	// L(x,y) = (scale / avgLuminance) * Lw(x,y)
	float Lwhite = maxLuminance * maxLuminance;
	float L = (scale / avgLuminance) * luminance;
	float Ld = (L * (1.0 + L / Lwhite)) / (1.0 + L);

	Ld = clamp(Ld, 0, 1);

	thisPixel.xyz = RGB2xyY(thisPixel.xyz);
	thisPixel.z *= Ld;
	thisPixel.xyz = xyY2RGB(thisPixel.xyz);

	out_color = vec4(thisPixel, 1);

	//DEBUG
	//out_color = vec4(1,0,0, 1);
		
}
	

