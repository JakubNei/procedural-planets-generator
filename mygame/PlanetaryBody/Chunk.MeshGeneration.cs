using MyEngine;
using MyEngine.Components;
using Neitri;
using OpenTK;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyGame.PlanetaryBody
{
	public partial class Chunk
	{
		Mesh.VertexIndex[] edgeVerticesIndexes;
		int NumberOfVerticesOnEdge => planetaryBody.chunkNumberOfVerticesOnEdge;


		int numberOfVerticesNeededTotal = -1;
		int NumberOfVerticesNeededTotal
		{
			get
			{
				if (numberOfVerticesNeededTotal != -1) return numberOfVerticesNeededTotal;

				numberOfVerticesNeededTotal = 1;
				{
					int numberOfVerticesInBetween = 0;
					for (uint y = 1; y < NumberOfVerticesOnEdge; y++)
					{
						numberOfVerticesNeededTotal++;
						if (numberOfVerticesInBetween > 0)
						{
							numberOfVerticesNeededTotal += numberOfVerticesInBetween;
						}
						numberOfVerticesNeededTotal++;
						numberOfVerticesInBetween++;
					}
				}

				return numberOfVerticesNeededTotal;
			}
		}

		Mesh.VertexIndex[] skirtIndicies = null;
		Mesh.VertexIndex[] GetEdgeVertices()
		{
			if (skirtIndicies != null) return skirtIndicies;

			var s = new List<Mesh.VertexIndex>();
			// gather the edge vertices indicies
			{
				int lineStartIndex = 0;
				int nextLineStartIndex = 1;
				int numberOfVerticesInBetween = 0;
				s.Add(0); // first line
						  // top and all middle lines
				for (int i = 1; i < NumberOfVerticesOnEdge - 1; i++)
				{
					lineStartIndex = nextLineStartIndex;
					nextLineStartIndex = lineStartIndex + numberOfVerticesInBetween + 2;
					s.Add(lineStartIndex);
					s.Add((lineStartIndex + numberOfVerticesInBetween + 1));
					numberOfVerticesInBetween++;
				}
				// bottom line
				lineStartIndex = nextLineStartIndex;
				for (int i = 0; i < NumberOfVerticesOnEdge; i++)
				{
					s.Add((lineStartIndex + i));
				}
			}
			skirtIndicies = s.ToArray();
			return skirtIndicies;
		}

		List<Vector3> verticesList = new List<Vector3>();
		List<Vector3> GetVerticesList()
		{
			if (verticesList.Count > 0) return verticesList;

			var r = new Random();
			while (verticesList.Count < NumberOfVerticesNeededTotal)
				verticesList.Add(new Vector3d(r.NextDouble(), r.NextDouble(), r.NextDouble()).ToVector3()); // WTF WTF WTF WTF FUCK
																											// THE FUCK IS THIS
																											// WHY THE FUCK DOES IT NOT WORK WITH ZEROS OR ONES
			return verticesList;
		}


		public void CreateRendererAndGenerateMesh()
		{
			if (parentChunk != null && parentChunk.renderer == null)
			{
				return;
			}
			lock (this)
			{
				if (isGenerated) return;
				isGenerated = true;
			}

			var offsetCenter = noElevationRange.CenterPos;
			var mesh = new Mesh();// "PlanetaryBodyChunk depth:" + subdivisionDepth + " #" + numbetOfChunksGenerated);
			numberOfChunksGenerated++;


			// generate evenly spaced vertices, then we make triangles out of them

			var normals = mesh.Normals;


			// generate all of our vertices
			/*
			{
				//positionsFinal.Add(noElevationRange.a.ToVector3());
				vertices.Add(GetFinalPos(noElevationRange.a).ToVector3());
				//vertices.Add(Vector3.Zero);

				// add positions, line by line
				{
					int numberOfVerticesInBetween = 0;
					for (uint y = 1; y < NumberOfVerticesOnEdge; y++)
					{
						var percent = y / (float)(NumberOfVerticesOnEdge - 1);
						var start = MyMath.Slerp(noElevationRange.a, noElevationRange.b, percent);
						var end = MyMath.Slerp(noElevationRange.a, noElevationRange.c, percent);

						//positionsFinal.Add(start.ToVector3());
						vertices.Add(GetFinalPos(start).ToVector3());
						//vertices.Add(Vector3.Zero);

						if (numberOfVerticesInBetween > 0)
						{
							for (uint x = 1; x <= numberOfVerticesInBetween; x++)
							{
								var v = MyMath.Slerp(start, end, x / (float)(numberOfVerticesInBetween + 1));

								//positionsFinal.Add(v.ToVector3());
								vertices.Add(GetFinalPos(v).ToVector3());
								//vertices.Add(Vector3.Zero);

							}
						}

						//positionsFinal.Add(end.ToVector3());
						vertices.Add(GetFinalPos(end).ToVector3());
						//vertices.Add(Vector3.Zero);

						numberOfVerticesInBetween++;
					}
				}
			}
			*/

			List<Vector3> vertices = GetVerticesList();
			List<int> indicies = GetIndiciesList();

			mesh.Vertices.SetData(vertices);
			mesh.TriangleIndicies.SetData(indicies);

			// tell every vertex where on the planet it is, so it can query from biomes splat map
			/*
			mesh.UVs.Clear();
			mesh.UVs.Capacity = mesh.Vertices.Count;
			for (int i = 0; i < mesh.Vertices.Count; i++)
			{
				var v = mesh.Vertices[i];
				var s = planetaryBody.CalestialToSpherical(v);
				mesh.UVs.Add(new Vector2((float)((s.longitude / Math.PI + 1) / 2), (float)(s.latitude / Math.PI + 0.5)));
			}
			*/

			edgeVerticesIndexes = GetEdgeVertices();

			bool useSkirts = false;
			//useSkirts = true;
			if (useSkirts)
			{
				var skirtVertices = mesh.Duplicate(edgeVerticesIndexes, mesh.Vertices, mesh.Normals);
				var moveAmount = this.noElevationRange.ToBoundingSphere().radius / 10;
				mesh.MoveVertices(skirtVertices, -this.noElevationRange.Normal.ToVector3() * (float)moveAmount, mesh.Vertices);
			}
			
			{
				var o = offsetCenter.ToVector3();
				var c = 0;
				var n = noElevationRange.Normal.ToVector3();

				mesh.Bounds = new Bounds(noElevationRange.CenterPos.ToVector3() - o);

				mesh.Bounds.Encapsulate(noElevationRange.a.ToVector3() + n * c - o);
				mesh.Bounds.Encapsulate(noElevationRange.b.ToVector3() + n * c - o);
				mesh.Bounds.Encapsulate(noElevationRange.c.ToVector3() + n * c - o);
				mesh.Bounds.Encapsulate(noElevationRange.CenterPos.ToVector3() + n * c - o);

				mesh.Bounds.Encapsulate(noElevationRange.a.ToVector3() - n * c - o);
				mesh.Bounds.Encapsulate(noElevationRange.b.ToVector3() - n * c - o);
				mesh.Bounds.Encapsulate(noElevationRange.c.ToVector3() - n * c - o);
				mesh.Bounds.Encapsulate(noElevationRange.CenterPos.ToVector3() - n * c - o);
			}

			if (renderer != null) throw new Exception("something went terribly wrong, renderer should be null");
			renderer = planetaryBody.Entity.AddComponent<MeshRenderer>();
			renderer.Mesh = mesh;
			renderer.Offset += offsetCenter;

			if (planetaryBody.planetMaterial != null) renderer.Material = planetaryBody.planetMaterial.CloneTyped();
			renderer.RenderingMode = MyRenderingMode.DontRender;
		}


		public void DestroyRenderer()
		{
			if (renderer != null)
			{
				//renderer.Mesh.Dispose();
				planetaryBody.Entity.DestroyComponent(renderer);
				renderer = null;
			}
		}

		//public void SmoothEdgeNormalsBasedOn(Chunk otherChunk)
		//{
		//	// my index, other index
		//	var toAverageIndexes = new List<Tuple<Mesh.VertexIndex, Mesh.VertexIndex>>();

		//	var myMesh = this.renderer.Mesh;
		//	var otherMesh = otherChunk.renderer.Mesh;

		//	foreach (var otherIndex in otherChunk.edgeVerticesIndexes)
		//	{
		//		var otherVertice = otherMesh.Vertices[otherIndex];
		//		foreach (var myIndex in this.edgeVerticesIndexes)
		//		{
		//			var myVertice = myMesh.Vertices[myIndex];
		//			if (myVertice.DistanceSqr(otherVertice) < 0.1f)
		//			{
		//				toAverageIndexes.Add(Tuple.Create(myIndex, otherIndex));
		//			}
		//		}
		//	}

		//	var myNormalsFinal = myMesh.Normals;
		//	var otherNormals = otherMesh.Normals;

		//	var myNormalsInitial = myMesh.VertexArray.GetVertexArrayBufferObject("normalsInitial") as Mesh.VertexBufferObject<Vector3>;
		//	var otherNormalsInitial = otherMesh.VertexArray.GetVertexArrayBufferObject("normalsInitial") as Mesh.VertexBufferObject<Vector3>;

		//	foreach (var toAverageIndex in toAverageIndexes)
		//	{
		//		{
		//			var myNormal = this.originalNormalsFinal[toAverageIndex.Item1];
		//			var otherNormal = otherChunk.originalNormalsFinal[toAverageIndex.Item2];

		//			var newNormal = (myNormal + otherNormal) / 2.0f;

		//			myNormalsFinal[toAverageIndex.Item1] = newNormal;
		//		}

		//		{
		//			var myNormal = this.originalNormalsInitial[toAverageIndex.Item1];
		//			var otherNormal = otherChunk.originalNormalsInitial[toAverageIndex.Item2];

		//			var newNormal = (myNormal + otherNormal) / 2.0f;

		//			myNormalsInitial[toAverageIndex.Item1] = newNormal;
		//		}
		//	}

		//	myMesh.NotifyDataChanged();
		//}

		public void SmoothEdgeNormalsWith(Chunk otherChunk)
		{
			// my index, other index
			//var toAverageIndexes = new List<Tuple<Mesh.VertexIndex, Mesh.VertexIndex>>();

			//var myMesh = this.renderer.Mesh;
			//var otherMesh = otherChunk.renderer.Mesh;

			//foreach (var otherIndex in otherChunk.edgeVerticesIndexes)
			//{
			//	var otherVertice = otherMesh.Vertices[otherIndex];
			//	foreach (var myIndex in this.edgeVerticesIndexes)
			//	{
			//		var myVertice = myMesh.Vertices[myIndex];
			//		if (myVertice.DistanceSqr(otherVertice) < 0.1f)
			//		{
			//			toAverageIndexes.Add(Tuple.Create(myIndex, otherIndex));
			//		}
			//	}
			//}

			//var myNormals = myMesh.Normals;
			//var otherNormals = otherMesh.Normals;

			//var myNormalsInitial = myMesh.VertexArray.GetVertexArrayBufferObject("normalsInitial") as Mesh.VertexBufferObject<Vector3>;
			//var otherNormalsInitial = otherMesh.VertexArray.GetVertexArrayBufferObject("normalsInitial") as Mesh.VertexBufferObject<Vector3>;

			//foreach (var toAverageIndex in toAverageIndexes)
			//{
			//	{
			//		var myNormal = myNormals[toAverageIndex.Item1];
			//		var otherNormal = otherNormals[toAverageIndex.Item2];

			//		var newNormal = (myNormal + otherNormal) / 2.0f;

			//		myNormals[toAverageIndex.Item1] = newNormal;
			//		otherNormals[toAverageIndex.Item2] = newNormal;
			//	}

			//	{
			//		var myNormalInitial = myNormalsInitial[toAverageIndex.Item1];
			//		var otherNormalInitial = otherNormalsInitial[toAverageIndex.Item2];

			//		var newNormalInitial = (myNormalInitial + otherNormalInitial) / 2.0f;

			//		myNormalsInitial[toAverageIndex.Item1] = newNormalInitial;
			//		otherNormalsInitial[toAverageIndex.Item2] = newNormalInitial;
			//	}
			//}

			//myMesh.NotifyDataChanged();
			//otherMesh.NotifyDataChanged();

		}
	}
}