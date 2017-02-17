using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Graphics.OpenGL4;
using System.Diagnostics;
using OpenTK;
//using OpenTK.Graphics.OpenGL4;

namespace MyEngine
{
    public static class My
    {
		//[Conditional("DEBUG")]
        public static void Check()
        {
            ErrorCode err;
            while ((err = GL.GetError()) != ErrorCode.NoError)
            {
                Debug.Instance.Error("GL Error: " + err);
                throw new Exception("GL Error: " + err);
            }
        }

        public static void Uniform3(int location, Vector3d vec)
        {
            GL.Uniform3(location, vec.X, vec.Y, vec.Z);
        }
    }
}
