using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace MyEngine
{
    public static class Constants
    {
        public static readonly Vector3 Vector3Up = Vector3.UnitY;
        public static readonly Vector3 Vector3Right = Vector3.UnitX;
        public static readonly Vector3 Vector3Forward = -Vector3.UnitZ;

		public static readonly Vector3d Vector3dUp = Vector3d.UnitY;
		public static readonly Vector3d Vector3dRight = Vector3d.UnitX;
		public static readonly Vector3d Vector3dForward = -Vector3d.UnitZ;
	}
}

