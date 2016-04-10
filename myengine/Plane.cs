using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace MyEngine
{
    public struct Plane
    {
        public Vector3 normal;
        public float distance;

        /*
        public Plane(Vector3 inNormal, Vector3 inPoint)
        {
            SetNormalAndPosition(inNormal, inPoint);
        }
        public Plane(Vector3 a, Vector3 b, Vector3 c)
        {
            Set3Points(a, b, c);
        }
        public void SetNormalAndPosition(Vector3 inNormal, Vector3 inPoint) {

        }
        public void Set3Points(Vector3 a, Vector3 b, Vector3 c)
        {

        }*/

        // http://www.youtube.com/watch?v=4p-E_31XOPM
        public float GetDistanceToPoint(Vector3 point)
        {
            return Vector3.Dot(point, normal) + distance;
        }
    }
}
