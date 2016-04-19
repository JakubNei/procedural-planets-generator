using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace MyEngine
{
    public static class QuaternionUtility
    {
        public static Quaternion LookRotation(Vector3 direction)
        {
            var up = Vector3.UnitY;
            if (up == direction) up = Vector3.UnitX;
            Matrix4 rot = Matrix4.LookAt(Vector3.Zero, direction, up);
            return rot.ExtractRotation();
        }
    }
}
