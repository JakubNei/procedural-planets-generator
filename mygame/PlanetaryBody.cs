using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

using MyEngine;
using MyEngine.Components;

namespace MyGame
{
    public class PlanetaryBody : MonoBehaviour
    {
        public int chunkNumberOfVerticesOnEdge = 10;
        public float radius;
        public int subdivisionRecurisonDepth = 10;
        public Material planetMaterial;
        public float radiusVariation = 20;
        public float subdivisionSphereRadiusModifier = 0.5f;
        public float startingRadiusSubdivisionModifier = 2;

        Perlin perlin;
        Worley worley;

        List<PlanetaryBodyChunk> rootChunks = new List<PlanetaryBodyChunk>();



        public PlanetaryBody(Entity entity) : base(entity)
        {
            perlin = new Perlin(5646);
            worley = new Worley(894984, Worley.DistanceFunction.Euclidian);
        }



        // http://stackoverflow.com/questions/1185408/converting-from-longitude-latitude-to-cartesian-coordinates
        public SpehricalCoord CalestialToSpherical(Vector3 c)
        {
            var r = c.Length;
            if (r == 0) return new SpehricalCoord(0, 0, 0);
            return new SpehricalCoord(
                (float)Math.Atan2(c.Z, c.X),
                (float)Math.Asin(c.Y / r),
                r
            );
        }

        // x = left right = longitude
        // y = top down = latitude
        // z = radius
        public Vector3 SphericalToCalestial(SpehricalCoord s)
        {
            var r = s.altitude;
            //s.latitude = s.latitude / 180.0f * M_PI;
            //s.longitude = s.longitude / 180.0f * M_PI;
            //if (r == 0) r = radius;
            return new Vector3(
                (float)Math.Cos(s.latitude) * (float)Math.Cos(s.longitude) * r,
                (float)Math.Sin(s.latitude) * r,
                (float)Math.Cos(s.latitude) * (float)Math.Sin(s.longitude) * r
            );
        }



        public Vector3 GetFinalPos(Vector3 calestialPos, int detailDensity = 1)
        {
            //return calestialPos;
            var s = CalestialToSpherical(calestialPos);
            s.altitude = GetHeight(calestialPos, detailDensity);
            return SphericalToCalestial(s);
        }


        public float GetHeight(Vector3 calestialPos, int detailDensity = 1)
        {
            var initialPos = calestialPos.Normalized();
            var pos = initialPos;

            int octaves = 3;
            float freq = 10;
            float ampModifier = .05f;
            float freqModifier = 15;
            float result = 0.0f;
            float amp = radiusVariation;
            pos *= freq;
            for (int i = 0; i < octaves; i++)
            {
                result += perlin.Get3D(pos) * amp;
                pos *= freqModifier;
                amp *= ampModifier;
            }

            {
                // hill tops
                var p = perlin.Get3D(initialPos * 10.0f);
                if (p > 0) result -= p * radiusVariation * 2;
            }

            {
                // craters
                var p = worley.GetAt(initialPos * 2.0f, 1);
                result += MyMath.SmoothStep(0.0f, 0.1f, p[0]) * radiusVariation * 2;
            }

            result += radius;
            return result;

            /*
            int octaves = 4;
            float sum = 0.5;
            float freq = 1.0, amp = 1.0;
            vec2 dsum = vec2(0);
            for (int i=0; i < octaves; i++)
            {
                Vector3 n = perlin.Get3D(calestialPos*freq);
                dsum += vec2(n.y, n.z);
                sum += amp * n.x / (1 + dot(dsum, dsum));
                freq *= lacunarity;
                amp *= gain;
            }
            return sum;
            */
        }


        void SetMaterialProperties(Material mat)
        {
            mat.SetUniform("planetRadius", this.radius);
            mat.SetUniform("planetCenter", Transform.Position);
        }


        List<Vector3> vertices;
        void FACE(int A, int B, int C)
        {

            var child = new PlanetaryBodyChunk(this, null);
            child.range.a = vertices[A];
            child.range.b = vertices[B];
            child.range.c = vertices[C];
            child.visibilityCollisionRange.a = GetFinalPos(child.range.a);
            child.visibilityCollisionRange.b = GetFinalPos(child.range.b);
            child.visibilityCollisionRange.c = GetFinalPos(child.range.c);
            this.rootChunks.Add(child);
        }



        public void Start()
        {
            //detailLevel = (int)ceil(planetInfo.rootChunks[0].range.ToBoundingSphere().radius / 100);


            vertices = new List<Vector3>();
            var indicies = new List<uint>();




            var r = this.radius / 2.0f;

            var t = (1 + MyMath.Sqrt(5.0f)) / 2.0f * r;
            var d = r;


            vertices.Add(new Vector3(-d, t, 0));
            vertices.Add(new Vector3(d, t, 0));
            vertices.Add(new Vector3(-d, -t, 0));
            vertices.Add(new Vector3(d, -t, 0));

            vertices.Add(new Vector3(0, -d, t));
            vertices.Add(new Vector3(0, d, t));
            vertices.Add(new Vector3(0, -d, -t));
            vertices.Add(new Vector3(0, d, -t));

            vertices.Add(new Vector3(t, 0, -d));
            vertices.Add(new Vector3(t, 0, d));
            vertices.Add(new Vector3(-t, 0, -d));
            vertices.Add(new Vector3(-t, 0, d));



            // 5 faces around point 0
            FACE(0, 11, 5);
            FACE(0, 5, 1);
            FACE(0, 1, 7);
            FACE(0, 7, 10);
            FACE(0, 10, 11);

            // 5 adjacent faces
            FACE(1, 5, 9);
            FACE(5, 11, 4);
            FACE(11, 10, 2);
            FACE(10, 7, 6);
            FACE(7, 1, 8);

            // 5 faces around point 3
            FACE(3, 9, 4);
            FACE(3, 4, 2);
            FACE(3, 2, 6);
            FACE(3, 6, 8);
            FACE(3, 8, 9);

            // 5 adjacent faces
            FACE(4, 9, 5);
            FACE(2, 4, 11);
            FACE(6, 2, 10);
            FACE(8, 6, 7);
            FACE(9, 8, 1);



        }


        void HideChildsIn(PlanetaryBodyChunk chunk, float seconds)
        {
            foreach (var child in chunk.childs)
            {
                if (child.IsMeshReady()) child.renderer.HideIn(seconds);
                child.StopMeshGeneration();
                HideChildsIn(child, seconds);
            }
        }

        // returns if chunk or its children are fully visible
        bool TrySubdivideToLevel(PlanetaryBodyChunk chunk, Sphere sphere, int recursionDepth)
        {
            const float showTime = 0.2f;
            const float hideTime = showTime;
            if (recursionDepth > 0 && GeometryUtility.Intersects(chunk.visibilityCollisionRange, sphere))
            {
                var areChildrenFullyVisible = true;
                chunk.SubDivide();
                sphere.radius *= subdivisionSphereRadiusModifier;
                foreach (var child in chunk.childs)
                {
                    areChildrenFullyVisible = TrySubdivideToLevel(child, sphere, recursionDepth - 1);
                }

                // hide only if all our childs are visible
                if (areChildrenFullyVisible) if (chunk.IsMeshReady()) chunk.renderer.HideIn(hideTime);

                return areChildrenFullyVisible;
            }
            else
            {
                if (chunk.IsMeshReady() == false) chunk.RequestMeshGeneration();
                if (chunk.IsMeshReady())
                {
                    if (chunk.renderer.GetVisibility() != 1)
                    {
                        // is not visible
                        // show it
                        chunk.renderer.ShowIn(showTime);
                    }
                    else {
                        // is visible
                        // now we can hide others

                        // hide childs
                        HideChildsIn(chunk, hideTime);
                    }
                }
                return chunk.IsMeshReady() && chunk.renderer.GetVisibility() == 1;
            }

        }

        public void TrySubdivideOver(Vector3 pos)
        {
            var sphere = new Sphere(pos - Transform.Position, this.radius * startingRadiusSubdivisionModifier);
            foreach (PlanetaryBodyChunk rootChunk in this.rootChunks)
            {
                TrySubdivideToLevel(rootChunk, sphere, this.subdivisionRecurisonDepth);
            }
        }


    }
}