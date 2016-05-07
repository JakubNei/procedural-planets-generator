using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace MyEngine
{
    public static partial class Vector3Extensions
    {
        public static Vector3d ToVector3d(this Vector3 floatVector)
        {
            return new Vector3d(
                floatVector.X,
                floatVector.Y,
                floatVector.Z
            );
        }

        public static Vector3 ToVector3(this Vector3d floatVector)
        {
            return new Vector3(
                (float)floatVector.X,
                (float)floatVector.Y,
                (float)floatVector.Z
            );
        }
    }
}
