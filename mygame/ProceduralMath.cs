using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MyGame
{
	public class ProceduralMath : IDisposable
	{

		IntPtr instance;

		public ProceduralMath()
		{
			instance = MakeInstance();
		}

		[DllImport(@"ProceduralMath.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern IntPtr MakeInstance();

		[DllImport(@"ProceduralMath.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern void DestroyInstance(IntPtr instance);


		[DllImport(@"ProceduralMath.dll", CallingConvention = CallingConvention.Cdecl)]
		static extern double GetHeight(IntPtr instance, double x, double y, double z, int detailLevel);


		public double GetHeight(Vector3d pos, int detailLevel)
		{
			return GetHeight(instance, pos.X, pos.Y, pos.Z, detailLevel);
		}

		public void Dispose()
		{
			DestroyInstance(instance);
		}
	}
}
