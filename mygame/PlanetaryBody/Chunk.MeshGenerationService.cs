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
	
		static MeshGenerationService meshGenerationService = new MeshGenerationService();
		class MeshGenerationService
		{


			int generationThreadMiliSecondsSleep;


			HashSet<Chunk> chunkIsBeingGenerated = new HashSet<Chunk>();

			//ReaderWriterLock chunkToPriority_mutex = new ReaderWriterLock();
			Dictionary<Chunk, double> chunkToWeight = new Dictionary<Chunk, double>();

			List<Thread> threads = new List<Thread>();

			bool doRun;

			public MeshGenerationService()
			{
				Start();
			}

			void Start()
			{
				generationThreadMiliSecondsSleep = 1;
				chunkToWeight.Clear();
				doRun = true;
				int numThreads = Environment.ProcessorCount;
#if DEBUG
				//numThreads = 1;
#endif
				for (int i = 0; i < numThreads; i++)
				{
					var threadIndex = i;
					var t = new Thread(() =>
					{
						ThreadMain(threadIndex);
					});
					t.IsBackground = true;
					t.Start();
					threads.Add(t);
				}
			}

			void ThreadMain(int threadIndex)
			{
				while (doRun)
				{

					Chunk chunk = null;

					lock (chunkToWeight)
					{
						if (chunkToWeight.Count > 0)
						{
							double weight = -1;
							foreach (var kvp in chunkToWeight)
							{
								if (kvp.Value > weight)
								{
									weight = kvp.Value;
									chunk = kvp.Key;
								}
							}
							if (chunk != null)
							{
								lock (chunkIsBeingGenerated)
								{
									if (chunkIsBeingGenerated.Contains(chunk))
									{
										chunk = null; // other thread found it faster than this one
									}
									else
									{
										chunkIsBeingGenerated.Add(chunk);
										chunkToWeight.Remove(chunk);
									}
								}
							}
						}
					}


					// this takes alot of time
					if (chunk != null)
					{

						chunk.CreateRendererAndGenerateMesh();

						lock (chunkIsBeingGenerated)
						{
							chunkIsBeingGenerated.Remove(chunk);
						}
					}

					if (threadIndex == 0)
					{
						Debug.AddValue("chunksToGenerateQueued", chunkToWeight.Count.ToString());


						//if (fps < 55) generationThreadMiliSecondsSleep *= 2;
						//else generationThreadMiliSecondsSleep /= 2;

						generationThreadMiliSecondsSleep = MyMath.Clamp(generationThreadMiliSecondsSleep, 10, 200);
					}
					Thread.Sleep(generationThreadMiliSecondsSleep);

				}
			}

			/// <summary>
			/// Smaller priority is more important.
			/// </summary>
			/// <param name="chunk"></param>
			/// <param name="weight"></param>
			public void RequestGenerationOfMesh(Chunk chunk, double weight)
			{
				if (chunk.renderer != null) return;

				if (chunk.parentChunk != null && chunk.parentChunk.renderer == null)
				{
					chunk.parentChunk.RequestMeshGeneration();
					return;
				}

				lock (chunkIsBeingGenerated)
				{
					var isChunkBeingGenerated = chunkIsBeingGenerated.Contains(chunk);
					if (isChunkBeingGenerated) return;
				}

				if (chunk.renderer != null) return;

				lock (chunkToWeight)
				{
					/*
                    var found = chunkToPriority.ContainsKey(chunk);
                    if (found == false)
                    {
                        chunkToPriority[chunk] = 0;
                    }
                    */
					chunkToWeight[chunk] = weight;
				}
			}


			public void DoesNotNeedMeshGeneration(Chunk chunk)
			{
				lock (chunkToWeight)
				{
					chunkToWeight.Remove(chunk);
				}
			}
		}

    }
}
