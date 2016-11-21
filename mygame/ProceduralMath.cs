using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
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

		[DllImport(@"ProceduralMath.dll", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		static extern IntPtr MakeInstance();

		[DllImport(@"ProceduralMath.dll", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		static extern void DestroyInstance(IntPtr instance);


		[DllImport(@"ProceduralMath.dll", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		static extern double GetHeight(IntPtr instance, double x, double y, double z, int detailLevel);

		[DllImport(@"ProceduralMath.dll", CallingConvention = CallingConvention.Cdecl), SuppressUnmanagedCodeSecurity]
		static extern void Configure(IntPtr instance, double radius, double radiusVariation);

		public void Configure(double radius, double radiusVariation)
		{
			Configure(instance, radius, radiusVariation);
		}
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
