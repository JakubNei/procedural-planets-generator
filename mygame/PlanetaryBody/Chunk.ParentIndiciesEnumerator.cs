using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using OpenTK;

using MyEngine;
using MyEngine.Components;
using System.Collections;

namespace MyGame.PlanetaryBody
{
    public partial class Chunk
    {
		class ParentIndiciesEnumerator : IEnumerator<int>
		{
			public int Current
			{
				get
				{
					return parent_current;
				}
			}

			object IEnumerator.Current
			{
				get
				{
					return Current;
				}
			}
			int parent_current;
			int parent_lineLength;
			int child_currentOnLine;
			int child_lineLength;
			int numOfVerticesOnEdgeWholeTriangle
			{
				get
				{
					return parentChunk.planetaryBody.chunkNumberOfVerticesOnEdge;
				}
			}
			Chunk parentChunk;
			ChildPosition myPos;
			public ParentIndiciesEnumerator(Chunk parentChunk, ChildPosition myPos)
			{
				this.parentChunk = parentChunk;
				this.myPos = myPos;
				Reset();
			}
			public void Dispose()
			{

			}

			public bool MoveNext()
			{
				switch (myPos)
				{
					case ChildPosition.Top:
						parent_current++;
						break;
					case ChildPosition.Left:
					case ChildPosition.Right:
						parent_current++;
						child_currentOnLine++;
						if (child_currentOnLine >= child_lineLength)
						{
							parent_current += parent_lineLength - child_lineLength;
							child_lineLength++;
							parent_lineLength++;
							child_currentOnLine = 0;
						}
						break;
					case ChildPosition.Middle:
						parent_current--;
						child_currentOnLine--;
						if (child_currentOnLine < 0)
						{
							parent_current -= parent_lineLength;
							parent_current += child_lineLength + 1;
							child_lineLength++;
							parent_lineLength--;
							child_currentOnLine = child_lineLength - 1;
						}
						break;

				}
				return true;
			}

			public void Reset()
			{
				switch (myPos)
				{
					// all but middle child triangles are iterated from left to right, top to bottm
					case ChildPosition.Top:
						parent_current = 0;
						break;
					case ChildPosition.Left:
						parent_current = 0;
						parent_lineLength = 1;
						for (int i = 0; i < (numOfVerticesOnEdgeWholeTriangle - 1) / 2; i++)
						{
							parent_current += parent_lineLength;
							parent_lineLength++;
						}
						child_lineLength = 1;
						child_currentOnLine = 0;
						break;
					case ChildPosition.Right:
						parent_current = 0;
						parent_lineLength = 1;
						for (int i = 0; i < (numOfVerticesOnEdgeWholeTriangle - 1) / 2; i++)
						{
							parent_current += parent_lineLength;
							parent_lineLength++;
						}
						parent_current += parent_lineLength - 1; // move to the end of the line
						child_lineLength = 1;
						child_currentOnLine = 0;
						break;
					case ChildPosition.Middle: // child middle triangle is iterated from right to left, bottom to top
						parent_current = 0;
						parent_lineLength = 1;
						for (int i = 0; i < numOfVerticesOnEdgeWholeTriangle - 1; i++) // stop at last line
						{
							parent_current += parent_lineLength;
							parent_lineLength++;
						}
						parent_current += (parent_lineLength - 1) / 2; // move to middle
						child_lineLength = 1;
						child_currentOnLine = 0;
						break;
				}
			}
		}

	}
}
