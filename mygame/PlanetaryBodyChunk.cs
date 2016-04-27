using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using OpenTK;

using MyEngine;
using MyEngine.Components;

namespace MyGame
{
    public class PlanetaryBodyChunk
    {
        public Triangle noElevationRange;
        public Triangle realVisibleRange;
        public List<PlanetaryBodyChunk> childs = new List<PlanetaryBodyChunk>();
        public MeshRenderer renderer;

        public float hideIn;
        public float showIn;
        public float visibility;

        int subdivisionDepth;
        PlanetaryBody planetaryBody;
        PlanetaryBodyChunk parentChunk;      

        public PlanetaryBodyChunk(PlanetaryBody planetInfo, PlanetaryBodyChunk parentChunk)
        {
            this.planetaryBody = planetInfo;
            this.parentChunk = parentChunk;
            childs.Clear();
        }

        public Vector3 GetCenterPos()
        {
            return realVisibleRange.CenterPos;
        }


        void MAKE_CHILD(Vector3 A, Vector3 B, Vector3 C) {

            var child = new PlanetaryBodyChunk(planetaryBody, this);
            childs.Add(child);
            child.subdivisionDepth = subdivisionDepth + 1;
            child.noElevationRange.a = A;
            child.noElevationRange.b = B;
            child.noElevationRange.c = C;
            child.realVisibleRange.a = planetaryBody.GetFinalPos(child.noElevationRange.a);
            child.realVisibleRange.b = planetaryBody.GetFinalPos(child.noElevationRange.b);
            child.realVisibleRange.c = planetaryBody.GetFinalPos(child.noElevationRange.c);
        }

        public void SubDivide()
        {
            if (childs.Count <= 0)
            {
                var a = noElevationRange.a;
                var b = noElevationRange.b;
                var c = noElevationRange.c;
                var ab = (a + b).Divide(2.0f).Normalized();
                var ac = (a + c).Divide(2.0f).Normalized();
                var bc = (b + c).Divide(2.0f).Normalized();

                ab *= planetaryBody.radius;
                ac *= planetaryBody.radius;
                bc *= planetaryBody.radius;

                MAKE_CHILD(a, ab, ac);
                MAKE_CHILD(ab, b, bc);
                MAKE_CHILD(ac, bc, c);
                MAKE_CHILD(ab, bc, ac);
            }

        }


        int numbetOfChunksGenerated = 0;
        bool isGenerated = false;
        void CreateRendererAndGenerateMesh()
        {
            lock(this)
            {
                if (isGenerated) return;
                isGenerated = true;
            }

            int numberOfVerticesOnEdge = planetaryBody.chunkNumberOfVerticesOnEdge;
            if (numberOfVerticesOnEdge % 2 == 0) numberOfVerticesOnEdge ++;


            var mesh = new Mesh();// "PlanetaryBodyChunk depth:" + subdivisionDepth + " #" + numbetOfChunksGenerated);
            numbetOfChunksGenerated++;

            var realRange = noElevationRange;

            const bool useSkirts = false;
            //const bool useSkirts = true;

            if (useSkirts)
            {
                var s = noElevationRange.a.Distance(noElevationRange.b) / (numberOfVerticesOnEdge - 1);
                var o = (float)Math.Sqrt(s * s + s * s);

                var d = noElevationRange.a.Distance(noElevationRange.CenterPos);

                var ratio = d / (d - o);
                //ratio *= 1.06f;
                //ratio *= 1.12f;
                //ratio = 0.9f; // debug

                var c = realRange.CenterPos;
                realRange.a = c + Vector3.Multiply(realRange.a - c, ratio);
                realRange.b = c + Vector3.Multiply(realRange.b - c, ratio);
                realRange.c = c + Vector3.Multiply(realRange.c - c, ratio);
            }
            



            //uint numberOfVerticesOnEdge = 4; // must be over 4, if under or 4 skirts will move all of it

            // realRange triangle is assumed to have all sides the same length

            // generate evenly spaced vertices, then we make triangles out of them
            var positionsFinal = new List<Vector3>();


            // the planetary chunk vertices blend from positonsInitial to positionsFinal
            // to nicely blend in more detail
            // var positionsInitial = new List<Vector3>(); 
            var positionsInitial = new Mesh.VertexBufferObject<Vector3>()
            {
                bufferTarget = OpenTK.Graphics.OpenGL.BufferTarget.ArrayBuffer,
                pointerType = OpenTK.Graphics.OpenGL.VertexAttribPointerType.Float,
                dataStrideInElementsNumber = 3,                
            };
            var normalsInitial = new Mesh.VertexBufferObject<Vector3>()
            {
                bufferTarget = OpenTK.Graphics.OpenGL.BufferTarget.ArrayBuffer,
                pointerType = OpenTK.Graphics.OpenGL.VertexAttribPointerType.Float,
                dataStrideInElementsNumber = 3,
            };



            positionsFinal.Add(realRange.a);
            // add positions, line by line
            {
                var startStep = (realRange.b - realRange.a) / (float)(numberOfVerticesOnEdge - 1);
                var endStep = (realRange.c - realRange.a) / (float)(numberOfVerticesOnEdge - 1);
                int numberOfVerticesInBetween = 0;
                for (uint y = 1; y < numberOfVerticesOnEdge; y++)
                {
                    Vector3 start = realRange.a + startStep * (float)y;
                    Vector3 end = realRange.a + endStep * (float)y;
                    positionsFinal.Add(start);
                    if (numberOfVerticesInBetween > 0)
                    {
                        var step = (end - start) / (float)(numberOfVerticesInBetween + 1);
                        for (uint x = 1; x <= numberOfVerticesInBetween; x++)
                        {
                            var v = start + step * (float)x;
                            positionsFinal.Add(v);
                        }
                    }
                    positionsFinal.Add(end);
                    numberOfVerticesInBetween++;
                }
            }


            /*

                 /\  top line
                /\/\
               /\/\/\
              /\/\/\/\ middle lines
             /\/\/\/\/\
            /\/\/\/\/\/\ bottom line

            */


            var indicies = new List<int>();
            // make triangles indicies list
            {
                int lineStartIndex = 0;
                int nextLineStartIndex = 1;
                indicies.Add(0);
                indicies.Add(1);
                indicies.Add(2);

                int numberOfVerticesInBetween = 0;
                // we skip first triangle as it was done manually
                // we skip last row of vertices as there are no triangles under it
                for (int y = 1; y < numberOfVerticesOnEdge - 1; y++)
                {

                    lineStartIndex = nextLineStartIndex;
                    nextLineStartIndex = lineStartIndex + numberOfVerticesInBetween + 2;

                    for (int x = 0; x <= numberOfVerticesInBetween + 1; x++)
                    {

                        indicies.Add(lineStartIndex + x);
                        indicies.Add(nextLineStartIndex + x);
                        indicies.Add(nextLineStartIndex + x + 1);

                        if (x <= numberOfVerticesInBetween) // not a last triangle in line
                        {
                            indicies.Add(lineStartIndex + x);
                            indicies.Add(nextLineStartIndex + x + 1);
                            indicies.Add(lineStartIndex + x + 1);
                        }
                    }

                    numberOfVerticesInBetween++;
                }
            }




            // add procedural heights to final positions
            for (int i = 0; i < positionsFinal.Count; i++)
            {
                var v = planetaryBody.GetFinalPos(positionsFinal[i]);
                positionsFinal[i] = v;
            }


            mesh.vertices.SetData(positionsFinal);
            mesh.triangleIndicies.SetData(indicies);
            mesh.RecalculateNormals();

            var normalsFinal = mesh.normals;

            // fill in initial positions, every odd positon is average of the two neighbouring final positions
            {
                positionsInitial.Resize(positionsFinal.Count);
                normalsInitial.Resize(positionsFinal.Count);

                int i = 0;
                positionsInitial[i] = positionsFinal[i];
                normalsInitial[i] = normalsFinal[i];
                i++;
                
                int numberOfVerticesOnLine = 2;
                for (int y = 1; y < numberOfVerticesOnEdge; y++)
                {
                    for (int x = 0; x < numberOfVerticesOnLine; x++)
                    {
                        if (y % 2 == 0)
                        {
                            if (x % 2 == 0)
                            {
                                positionsInitial[i] = positionsFinal[i];
                                normalsInitial[i] = normalsFinal[i];
                                i++;
                            }
                            else
                            {
                                int a = i - 1;
                                int b = i + 1;
                                positionsInitial[i] = (positionsFinal[a] + positionsFinal[b]) / 2.0f;
                                normalsInitial[i] = (normalsFinal[a] + normalsFinal[b]) / 2.0f;
                                normalsInitial[i] = normalsFinal[a];
                                i++;
                            }
                        }
                        else
                        {
                            if (x % 2 == 0)
                            {
                                int a = i - numberOfVerticesOnLine + 1;
                                int b = i + numberOfVerticesOnLine;
                                positionsInitial[i] = (positionsFinal[a] + positionsFinal[b]) / 2.0f;
                                normalsInitial[i] = (normalsFinal[a] + normalsFinal[b]) / 2.0f;
                                normalsInitial[i] = normalsFinal[a];
                                i++;
                            }
                            else
                            {
                                int a = i - numberOfVerticesOnLine;
                                int b = i + numberOfVerticesOnLine + 1;
                                positionsInitial[i] = (positionsFinal[a] + positionsFinal[b]) / 2.0f;
                                normalsInitial[i] = (normalsFinal[a] + normalsFinal[b]) / 2.0f;
                                normalsInitial[i] = normalsFinal[a];
                                i++;
                            }
                        }
                    }
                                    
                    numberOfVerticesOnLine++;
                }
            }

            var skirtIndicies = new List<int>();
            // gather the edge vertices indicies
            {
                int lineStartIndex = 0;
                int nextLineStartIndex = 1;
                int numberOfVerticesInBetween = 0;
                skirtIndicies.Add(0); // first line
                // top and all middle lines
                for (int i = 1; i < numberOfVerticesOnEdge - 1; i++)
                {
                    lineStartIndex = nextLineStartIndex;
                    nextLineStartIndex = lineStartIndex + numberOfVerticesInBetween + 2;
                    skirtIndicies.Add(lineStartIndex);
                    skirtIndicies.Add((lineStartIndex + numberOfVerticesInBetween + 1));
                    numberOfVerticesInBetween++;
                }
                // bottom line
                lineStartIndex = nextLineStartIndex;
                for (int i = 0; i < numberOfVerticesOnEdge; i++)
                {
                    skirtIndicies.Add((lineStartIndex + i));
                }
            }


            // make skirts
            if(useSkirts) {
                // the deeper chunk it the less the multiplier should be
                var skirtMultiplier = 0.99f + 0.01f * subdivisionDepth / (planetaryBody.subdivisionMaxRecurisonDepth + 2);
                skirtMultiplier = MyMath.Clamp(skirtMultiplier, 0.95f, 1.0f);

                var chunkCenter = realRange.CenterPos;
                foreach (var index in skirtIndicies)
                {
                    // lower the skirts towards middle
                    // move chunks towards triangle center
                    {
                        var v = mesh.vertices[index];
                        v *= skirtMultiplier;
                        v = chunkCenter + (v - chunkCenter) * skirtMultiplier;
                        mesh.vertices[index] = v;
                    }
                    {
                        var v = positionsInitial[index];
                        v *= skirtMultiplier;
                        v = chunkCenter + (v - chunkCenter) * skirtMultiplier;
                        positionsInitial[index] = v;
                    }
                }
            }



            mesh.VertexArrayObj.AddVertexBufferObject("positionsInitial", positionsInitial);
            mesh.VertexArrayObj.AddVertexBufferObject("normalsInitial", normalsInitial);

            mesh.RecalculateBounds();

            if (renderer != null) throw new Exception("something went terribly wrong, renderer should be null");
            renderer = planetaryBody.Entity.AddComponent<MeshRenderer>();
            renderer.Mesh = mesh;

            if(planetaryBody.planetMaterial != null) renderer.Material = planetaryBody.planetMaterial.CloneTyped();
            renderer.RenderingMode = RenderingMode.DontRender;
            this.visibility = 0;

        }

        public void StopMeshGeneration()
        {
            meshGenerationService.DoesNotNeedMeshGeneration(this);
        }


        public void RequestMeshGeneration()
        {
            if (renderer != null) return;

            var cam = planetaryBody.Entity.Scene.mainCamera;

            // help from http://stackoverflow.com/questions/3717226/radius-of-projected-sphere
            var sphere = realVisibleRange.ToBoundingSphere();
            var radiusWorldSpace = sphere.radius;
            var sphereDistanceToCameraWorldSpace = cam.Transform.Position.Distance(planetaryBody.Transform.TransformPoint(sphere.center));
            var fov = cam.fieldOfView;
            var radiusCameraSpace = radiusWorldSpace * MyMath.Cot(fov / 2) / sphereDistanceToCameraWorldSpace;
            var priority = sphereDistanceToCameraWorldSpace / radiusCameraSpace;
            if (priority < 0) priority *= -1;

            if (parentChunk != null && parentChunk.renderer != null) {
                var cameraStatus = parentChunk.renderer.GetCameraRenderStatus(planetaryBody.Scene.mainCamera);
                if (cameraStatus.HasFlag(Renderer.RenderStatus.Visible)) priority *= 0.3f;
            }

            // smaller priority is more important
            meshGenerationService.RequestGenerationOfMesh(this, priority);
        }


        static MeshGenerationService meshGenerationService = new MeshGenerationService();
        class MeshGenerationService {


            int generationThreadMiliSecondsSleep;


            HashSet<PlanetaryBodyChunk> chunkIsBeingGenerated = new HashSet<PlanetaryBodyChunk>();

            //ReaderWriterLock chunkToPriority_mutex = new ReaderWriterLock();
            Dictionary<PlanetaryBodyChunk, float> chunkToPriority = new Dictionary<PlanetaryBodyChunk, float>();

            List<Thread> threads = new List<Thread>();

            bool doRun;

            public MeshGenerationService()
            {
                Start();
            }

            void Start()
            {
                generationThreadMiliSecondsSleep = 1;
                chunkToPriority.Clear();
                doRun = true;
                int numThreads = Environment.ProcessorCount;
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

                    PlanetaryBodyChunk chunk = null;
                    float priority = float.MaxValue;

                    lock(chunkToPriority)
                    {
                        foreach (var kvp in chunkToPriority)
                        {
                            if (kvp.Value < priority)
                            {
                                priority = kvp.Value;
                                chunk = kvp.Key;
                            }
                        }
                        if(chunk != null)
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
                                    chunkToPriority.Remove(chunk);
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
                        Debug.AddValue("chunksToGenerateQueued", chunkToPriority.Count.ToString());


                        //if (fps < 55) generationThreadMiliSecondsSleep *= 2;
                        //else generationThreadMiliSecondsSleep /= 2;

                        generationThreadMiliSecondsSleep = MyMath.Clamp(generationThreadMiliSecondsSleep, 10, 200);
                    }
                    Thread.Sleep(generationThreadMiliSecondsSleep);

                }
            }

            public void RequestGenerationOfMesh(PlanetaryBodyChunk chunk, float priorityAdd)
            {
                if (chunk.renderer != null) return;

                lock (chunkIsBeingGenerated)
                {
                    var isChunkBeingGenerated = chunkIsBeingGenerated.Contains(chunk);
                    if (isChunkBeingGenerated) return;
                }

                if (chunk.renderer != null) return;

                lock (chunkToPriority)
                {
                    /*
                    var found = chunkToPriority.ContainsKey(chunk);
                    if (found == false)
                    {
                        chunkToPriority[chunk] = 0;
                    }
                    */
                    chunkToPriority[chunk] = priorityAdd;
                }
            }


            public void DoesNotNeedMeshGeneration(PlanetaryBodyChunk chunk)
            {
                lock(chunkToPriority)
                {
                    chunkToPriority.Remove(chunk);
                }
            }
        }

    }
}