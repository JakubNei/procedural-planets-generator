using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Collections;

namespace MyEngine
{
    public partial class Mesh
    {

        public static void CalculateNormals(IList<int> inTriangleIndicies, IList<Vector3> inPositions, IList<Vector3> outNormals)
        {
            int verticesNum = inPositions.Count;
            int indiciesNum = inTriangleIndicies.Count;


            int[] counts = new int[verticesNum];

            outNormals.Clear();
            if (outNormals is List<Vector3>)
            {
                (outNormals as List<Vector3>).Capacity = verticesNum;
            }

            for (int i = 0; i < verticesNum; i++)
            {
                outNormals.Add(Vector3.Zero);
            }

            for (int i = 0; i <= indiciesNum - 3; i += 3)
            {

                int ai = inTriangleIndicies[i];
                int bi = inTriangleIndicies[i + 1];
                int ci = inTriangleIndicies[i + 2];

                if (ai < verticesNum && bi < verticesNum && ci < verticesNum)
                {
                    Vector3 av = inPositions[ai];
                    Vector3 n = Vector3.Normalize(Vector3.Cross(
                        inPositions[bi] - av,
                        inPositions[ci] - av
                    ));

                    outNormals[ai] += n;
                    outNormals[bi] += n;
                    outNormals[ci] += n;

                    counts[ai]++;
                    counts[bi]++;
                    counts[ci]++;
                }
            }

            for (int i = 0; i < verticesNum; i++)
            {
                outNormals[i] /= counts[i];
            }


        }
        public void RecalculateNormals()
        {
            CalculateNormals(TriangleIndicies, Vertices, Normals);
        }

        public void RecalculateTangents()
        {
            if (HasUVs() == false) Uvs.Resize(Vertices.Count);

            //partialy stolen from http://www.opengl-tutorial.org/intermediate-tutorials/tutorial-13-normal-mapping/

            int verticesNum = Vertices.Count;
            int indiciesNum = TriangleIndicies.Count;

            int[] counts = new int[verticesNum];

            Tangents.Clear();
            Tangents.Capacity = verticesNum;

            for (int i = 0; i < verticesNum; i++)
            {
                counts[i] = 0;
                Tangents.Add(Vector3.Zero);
            }

            for (int i = 0; i <= indiciesNum - 3; i += 3)
            {

                int ai = TriangleIndicies[i];
                int bi = TriangleIndicies[i + 1];
                int ci = TriangleIndicies[i + 2];

                if (ai < verticesNum && bi < verticesNum && ci < verticesNum)
                {
                    Vector3 av = Vertices[ai];
                    Vector3 deltaPos1 = Vertices[bi] - av;
                    Vector3 deltaPos2 = Vertices[ci] - av;

                    Vector2 auv = Uvs[ai];
                    Vector2 deltaUV1 = Uvs[bi] - auv;
                    Vector2 deltaUV2 = Uvs[ci] - auv;

                    float r = 1.0f / (deltaUV1.X * deltaUV2.Y - deltaUV1.Y * deltaUV2.X);
                    Vector3 t = (deltaPos1 * deltaUV2.Y - deltaPos2 * deltaUV1.Y) * r;


                    Tangents[ai] += t;
                    Tangents[bi] += t;
                    Tangents[ci] += t;

                    counts[ai]++;
                    counts[bi]++;
                    counts[ci]++;
                }
            }

            for (int i = 0; i < verticesNum; i++)
            {
                Tangents[i] /= counts[i];
            }


        }


        public struct VertexIndex
        {
            public int vertexIndex;
            public override int GetHashCode()
            {
                return vertexIndex.GetHashCode();
            }
            public static implicit operator int(VertexIndex me)
            {
                return me.vertexIndex;
            }
        }

        public VertexIndex[] ExtrudeOnlyEdges(VertexIndex[] extrudeVertices, Vector3 addVector)
        {
            var v = ExtrudeOnlyEdges(extrudeVertices);
            MoveVertexes(v, addVector);
            return v;
        }


        public VertexIndex[] ExtrudeOnlyEdges(VertexIndex[] extrudeVertices)
        {
            var extrudedVertices = new HashSet<VertexIndex>(extrudeVertices);

            int duplicatedVerticesStartIndex = Vertices.Count;

            int originalVerticesCount = extrudeVertices.Length;
            // duplicate selected vertexes
            for (int i = 0; i < originalVerticesCount; i++)
            {
                var duplicateVertex = Vertices[extrudeVertices[i]];
                Vertices.Add(duplicateVertex);
            }

            for (int i = 0; i < originalVerticesCount; i++)
            {
                this.TriangleIndicies.
            }




            // if they are neighbouring, make face between them
        }

        public void MoveVertexes(VertexIndex[] moveVertices, Vector3 addVector)
        {
            for (int i = 0; i < moveVertices.Length; i++)
            {
                this.Vertices[moveVertices[i].vertexIndex] += addVector;
            }
        }

    }
}
