using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEngine.SimulateGLSL
{
	public class Base
	{

		public static vec2 vec2(float x, float y) => MyEngine.SimulateGLSL.vec2.New(x, y);
		public static vec3 vec3(float x, float y, float z) => MyEngine.SimulateGLSL.vec3.New(x, y, z);
		public static vec4 vec4(float x, float y, float z, float w) => MyEngine.SimulateGLSL.vec4.New(x, y, z, w);

		public static vec2 vec2(double x, double y) => MyEngine.SimulateGLSL.vec2.New(x, y);
		public static vec3 vec3(double x, double y, double z) => MyEngine.SimulateGLSL.vec3.New(x, y, z);
		public static vec4 vec4(double x, double y, double z, double w) => MyEngine.SimulateGLSL.vec4.New(x, y, z, w);

		public static double floor(double a) => Math.Floor(a);
		public static float floor(float a) => (float)Math.Floor(a);
		public static vec2 floor(vec2 a) => vec2(floor(a.x), floor(a.y));
		public static vec3 floor(vec3 a) => vec3(floor(a.x), floor(a.y), floor(a.z));
		public static vec4 floor(vec4 a) => vec4(floor(a.x), floor(a.y), floor(a.z), floor(a.w));


	}
}
