using System;
using System.Linq;
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


		public struct VertexIndex : IEquatable<VertexIndex>
		{
			public int vertexIndex;

			public VertexIndex(int vertexIndex)
			{
				this.vertexIndex = vertexIndex;
			}
			public override bool Equals(object obj)
			{
				return Equals((VertexIndex)obj);
			}
			public bool Equals(VertexIndex other)
			{
				return other.vertexIndex == this.vertexIndex;
			}
			public override int GetHashCode()
			{
				return vertexIndex.GetHashCode();
			}
			public static implicit operator int(VertexIndex me)
			{
				return me.vertexIndex;
			}
			public static implicit operator VertexIndex(int other)
			{
				return new VertexIndex(other);
			}
			public override string ToString()
			{
				return vertexIndex.ToString();
			}
		}

		public IEnumerable<VertexIndex> FindNeighbours(VertexIndex a)
		{
			var indexOfA = this.TriangleIndicies.IndexOf(a);
			while (indexOfA != -1)
			{
				var verticeTripleIndex = (int)((indexOfA / 3.0).Floor() * 3);
				yield return new VertexIndex(this.TriangleIndicies[verticeTripleIndex + 0]);
				yield return new VertexIndex(this.TriangleIndicies[verticeTripleIndex + 1]);
				yield return new VertexIndex(this.TriangleIndicies[verticeTripleIndex + 2]);
				indexOfA = this.TriangleIndicies.IndexOf(a, verticeTripleIndex + 3); // start search at next triple
			}
		}

		public void AddTriangle(VertexIndex a, VertexIndex b, VertexIndex c)
		{
			this.TriangleIndicies.Add(a);
			this.TriangleIndicies.Add(b);
			this.TriangleIndicies.Add(c);
		}

		public bool IsNeighbouring(VertexIndex a, VertexIndex b)
		{
			return FindNeighbours(a).Contains(b);
		}

		public VertexIndex[] Extrude(VertexIndex[] extrudeVertices)
		{
			return Extrude(extrudeVertices, Vertices, Normals, Tangents);
		}
		public VertexIndex[] Extrude(VertexIndex[] extrudeVertices, params IVertexBufferObject[] extrudeVBOs)
		{
			var newExtrudedVertices = new VertexIndex[extrudeVertices.Length];

			// duplicate selected vertexes
			for (int i = 0; i < extrudeVertices.Length; i++)
			{
				newExtrudedVertices[i] = new VertexIndex(Vertices.Count);

				foreach (var vbo in extrudeVBOs)
				{
					vbo.Duplicate(extrudeVertices[i]);
				}
			}

			// if they are neighbouring, make face between them
			for (int a = 0; a < newExtrudedVertices.Length; a++)
			{
				for (int b = a; b < newExtrudedVertices.Length; b++)
				{
					if (IsNeighbouring(extrudeVertices[a], extrudeVertices[b]))
					{
						AddTriangle(extrudeVertices[a], extrudeVertices[b], newExtrudedVertices[b]);
						AddTriangle(extrudeVertices[a], newExtrudedVertices[b], newExtrudedVertices[a]);
					}
				}
			}

			return newExtrudedVertices;
		}

		public void MoveVertexes(VertexIndex[] moveVertices, Vector3 addVector)
		{
			MoveVertexes(moveVertices, addVector, Vertices);
		}

		public void MoveVertexes(VertexIndex[] moveVertices, Vector3 addVector, params VertexBufferObject<Vector3>[] extrudeVBOs)
		{
			foreach (var vbo in extrudeVBOs)
			{
				for (int i = 0; i < moveVertices.Length; i++)
				{
					vbo[moveVertices[i].vertexIndex] += addVector;
				}
			}
		}

	}
}
