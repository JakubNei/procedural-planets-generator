using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace MyEngine
{
    public static class GeometryUtility
    {
        public static bool TestPlanesAABB(Plane[] planes, Bounds bounds)
        {
            for (uint i = 0; i < 6; i++)
            {
                if (planes[i].GetDistanceToPoint(bounds.Center) < -bounds.Size.Length)
                {
                    return false;
                }
            }
            return true;        
        }


        public static bool Intersects(Triangle triangle, Sphere sphere)
        {

            // from http://realtimecollisiondetection.net/blog/?p=103
            var A = triangle.a - sphere.center;
            var B = triangle.b - sphere.center;
            var C = triangle.c - sphere.center;
            var rr = sphere.radius * sphere.radius;

            var V = (B - A).Cross(C - A);
            var d = A.Dot(V);
            var e = V.Dot( V);
            var sep1 = (d * d > rr * e);
            var aa = A.Dot(A);
            var ab = A.Dot(B);
            var ac = A.Dot(C);
            var bb = B.Dot(B);
            var bc = B.Dot(C);
            var cc = C.Dot(C);
            var sep2 = (aa > rr) & (ab > aa) & (ac > aa);
            var sep3 = (bb > rr) & (ab > bb) & (bc > bb);
            var sep4 = (cc > rr) & (ac > cc) & (bc > cc);
            var AB = B - A;
            var BC = C - B;
            var CA = A - C;
            var d1 = ab - aa;
            var d2 = bc - bb;
            var d3 = ac - cc;
            var e1 = AB.Dot(AB);
            var e2 = BC.Dot(BC);
            var e3 = CA.Dot(CA);
            var Q1 = A * e1 - d1 * AB;
            var Q2 = B * e2 - d2 * BC;
            var Q3 = C * e3 - d3 * CA;
            var QC = C * e1 - Q1;
            var QA = A * e2 - Q2;
            var QB = B * e3 - Q3;
            var sep5 = (Q1.Dot(Q1) > rr * e1 * e1) & (Q1.Dot(QC) > 0);
            var sep6 = (Q2.Dot(Q2) > rr * e2 * e2) & (Q2.Dot(QA) > 0);
            var sep7 = (Q3.Dot(Q3) > rr * e3 * e3) & (Q3.Dot(QB) > 0);
            return !(sep1 | sep2 | sep3 | sep4 | sep5 | sep6 | sep7);
            // or this, but i failed to convert it into shere x triangle http://www.phatcode.net/articles.php?id=459
        }

    }
}
