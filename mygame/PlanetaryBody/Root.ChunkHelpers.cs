using MyEngine;
using MyEngine.Components;
using MyEngine.Events;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace MyGame.PlanetaryBody
{
    public partial class Root
    {

		List<Vector3> verticesList = new List<Vector3>();
		public List<Vector3> GetVerticesList()
		{
			if (verticesList.Count > 0) return verticesList;

			var r = new Random();
			while (verticesList.Count < NumberOfVerticesNeededTotal)
				verticesList.Add(new Vector3d(r.NextDouble(), r.NextDouble(), r.NextDouble()).ToVector3()); // WTF WTF WTF WTF FUCK
																											// THE FUCK IS THIS
																											// WHY THE FUCK DOES IT NOT WORK WITH ZEROS OR ONES
			return verticesList;
		}

		List<int> indiciesList;
		public int AIndex => 0;
		public int BIndex;
		public int CIndex;
		public List<int> GetIndiciesList()
		{
			/*

                 /\  top line
                /\/\
               /\/\/\
              /\/\/\/\ middle lines
             /\/\/\/\/\
            /\/\/\/\/\/\ bottom line

            */
			if (indiciesList != null) return indiciesList;

			indiciesList = new List<int>();
			// make triangles indicies list
			{
				int lineStartIndex = 0;
				int nextLineStartIndex = 1;
				indiciesList.Add(0);
				indiciesList.Add(1);
				indiciesList.Add(2);

				int numberOfVerticesInBetween = 0;
				// we skip first triangle as it was done manually
				// we skip last row of vertices as there are no triangles under it
				for (int y = 1; y < ChunkNumberOfVerticesOnEdge - 1; y++)
				{
					lineStartIndex = nextLineStartIndex;
					nextLineStartIndex = lineStartIndex + numberOfVerticesInBetween + 2;

					for (int x = 0; x <= numberOfVerticesInBetween + 1; x++)
					{
						indiciesList.Add(lineStartIndex + x);
						indiciesList.Add(nextLineStartIndex + x);
						indiciesList.Add(nextLineStartIndex + x + 1);

						if (x <= numberOfVerticesInBetween) // not a last triangle in line
						{
							indiciesList.Add(lineStartIndex + x);
							indiciesList.Add(nextLineStartIndex + x + 1);
							indiciesList.Add(lineStartIndex + x + 1);
						}
					}

					numberOfVerticesInBetween++;
				}
			}
			return indiciesList;
		}


		Mesh.VertexIndex[] skirtIndicies = null;
		public Mesh.VertexIndex[] GetEdgeVertices()
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
				for (int i = 1; i < ChunkNumberOfVerticesOnEdge - 1; i++)
				{
					lineStartIndex = nextLineStartIndex;
					nextLineStartIndex = lineStartIndex + numberOfVerticesInBetween + 2;
					s.Add(lineStartIndex);
					s.Add((lineStartIndex + numberOfVerticesInBetween + 1));
					numberOfVerticesInBetween++;
				}
				// bottom line
				lineStartIndex = nextLineStartIndex;
				for (int i = 0; i < ChunkNumberOfVerticesOnEdge; i++)
				{
					s.Add((lineStartIndex + i));
				}
			}
			skirtIndicies = s.ToArray();
			return skirtIndicies;
		}
	}
}
