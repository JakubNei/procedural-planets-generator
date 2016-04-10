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
        /*
        PlanetaryBodyChunk(PlanetaryBody* planetInfo, PlanetaryBodyChunk* parent);
        ~PlanetaryBodyChunk();
        int subdivisionDepth;
        vector<PlanetaryBodyChunk*> childs;
        Triangle range;
        Triangle visibilityCollisionRange;
        PlanetaryBody* planetInfo;
        PlanetaryBodyChunk* parentChunk;
        bool childsCreates;
        Renderer* renderer;

        Vector3 GetCenterPos();

        void SubDivide();
        bool IsMeshReady();
        void RequestMeshGeneration();
        void StopMeshGeneration();

        private:

	    void CreateRendererAndGenerateMesh();

        class MeshGenerationService : public Scheduled
	    {
	    public:

		    ~MeshGenerationService();

            int generationThreadMiliSecondsSleep;

            mutable shared_timed_mutex chunkIsBeingGenerated_mutex;
		    unordered_set<PlanetaryBodyChunk*> chunkIsBeingGenerated;

            mutable shared_timed_mutex chunkToPriority_mutex;
		    unordered_map<PlanetaryBodyChunk*, float> chunkToPriority;

            vector<thread*> threads;

            bool doRun;
            void Start();
            void ThreadMain(uint threadIndex);
            void RequestGenerationOfMesh(PlanetaryBodyChunk*, float priorityAdd = 1);
            void DoesNotNeedMeshGeneration(PlanetaryBodyChunk*);
        };
        static MeshGenerationService* meshGenerationService;
    */
        public Triangle range;
        public Triangle visibilityCollisionRange;
        public List<PlanetaryBodyChunk> childs = new List<PlanetaryBodyChunk>();
        public MeshRenderer renderer;

        int subdivisionDepth;
        PlanetaryBody planetInfo;
        PlanetaryBodyChunk parentChunk;
        bool childsCreates;

        public PlanetaryBodyChunk(PlanetaryBody planetInfo, PlanetaryBodyChunk parentChunk)
        {
            this.planetInfo = planetInfo;
            this.parentChunk = parentChunk;
            childs.Clear();
        }

        public Vector3 GetCenterPos()
        {
            return (
                (visibilityCollisionRange.a) +
                (visibilityCollisionRange.b) +
                (visibilityCollisionRange.c)
            ) / 3.0f;
        }


        public bool IsMeshReady()
        {
            return renderer != null;
        }


        void MAKE_CHILD(Vector3 A, Vector3 B, Vector3 C) {

            var child = new PlanetaryBodyChunk(planetInfo, this);
            childs.Add(child);
            child.subdivisionDepth = subdivisionDepth + 1;
            child.range.a = A;
            child.range.b = B;
            child.range.c = C;
            child.visibilityCollisionRange.a = planetInfo.GetFinalPos(child.range.a);
            child.visibilityCollisionRange.b = planetInfo.GetFinalPos(child.range.b);
            child.visibilityCollisionRange.c = planetInfo.GetFinalPos(child.range.c);
        }

        public void SubDivide()
        {
            if (childs.Count <= 0)
            {
                var a = range.a;
                var b = range.b;
                var c = range.c;
                var ab = (a + b).Divide(2.0f).Normalized().Multiply(planetInfo.radius);
                var ac = (a + c).Divide(2.0f).Normalized().Multiply(planetInfo.radius);
                var bc = (b + c).Divide(2.0f).Normalized().Multiply(planetInfo.radius);

                MAKE_CHILD(a, ab, ac);
                MAKE_CHILD(ab, b, bc);
                MAKE_CHILD(ac, bc, c);
                MAKE_CHILD(ab, bc, ac);
            }

        }


        int numbetOfChunksGenerated = 0;
        void CreateRendererAndGenerateMesh()
        {
            int numberOfVerticesOnEdge = planetInfo.chunkNumberOfVerticesOnEdge;

            var mesh = new Mesh();// "PlanetaryBodyChunk depth:" + subdivisionDepth + " #" + numbetOfChunksGenerated);
            numbetOfChunksGenerated++;

            var realRange = range;
            {

                var s = range.a.Distance(range.b) / (numberOfVerticesOnEdge - 1);
                var o = (float)Math.Sqrt(s * s + s * s);

                var d = range.a.Distance(range.CenterPos());

                var ratio = d / (d - o);
                //ratio *= 1.06f;
                ratio *= 1.12f;
                //ratio = 0.9f; // debug

                var c = realRange.CenterPos();
                realRange.a = c + Vector3.Multiply(realRange.a - c, ratio);
                realRange.b = c + Vector3.Multiply(realRange.b - c, ratio);
                realRange.c = c + Vector3.Multiply(realRange.c - c, ratio);
            }



            //uint numberOfVerticesOnEdge = 4; // must be over 4, if under or 4 skirts will move all of it

            // realRange triangle is assumed to have all sides the same length

            // generate evenly spaced vertices, then we make triangles out of them
            var vertices = new List<Vector3>();
            vertices.Add(realRange.a);
            var startStep = (realRange.b - realRange.a) / (float)(numberOfVerticesOnEdge - 1);
            var endStep = (realRange.c - realRange.a) / (float)(numberOfVerticesOnEdge - 1);
            int numberOfVerticesInBetween = 0;
            for (uint u = 1; u < numberOfVerticesOnEdge; u++)
            {
                Vector3 start = realRange.a + startStep * (float)u;
                Vector3 end = realRange.a + endStep * (float)u;
                vertices.Add(start);
                if (numberOfVerticesInBetween > 0)
                {
                    var step = (end - start) / (float)(numberOfVerticesInBetween + 1);
                    for (uint i = 1; i <= numberOfVerticesInBetween; i++)
                    {
                        var v = start + step * (float)i;
                        vertices.Add(v);
                    }
                }
                vertices.Add(end);
                numberOfVerticesInBetween++;
            }


            var indicies = new List<int>();


            // make triangles

            int lineStartIndex = 0;
            int nextLineStartIndex = 1;
            indicies.Add(0);
            indicies.Add(1);
            indicies.Add(2);

            numberOfVerticesInBetween = 0;
            // we skip first triangle as it was done manually
            // we skip last row of vertices as there are no triangles under it
            for (int i = 1; i < numberOfVerticesOnEdge - 1; i++)
            {

                lineStartIndex = nextLineStartIndex;
                nextLineStartIndex = lineStartIndex + numberOfVerticesInBetween + 2;

                for (int u = 0; u <= numberOfVerticesInBetween + 1; u++)
                {

                    indicies.Add(lineStartIndex + u);
                    indicies.Add(nextLineStartIndex + u);
                    indicies.Add(nextLineStartIndex + u + 1);

                    if (u <= numberOfVerticesInBetween)
                    {
                        indicies.Add(lineStartIndex + u);
                        indicies.Add(nextLineStartIndex + u + 1);
                        indicies.Add(lineStartIndex + u + 1);
                    }
                }

                numberOfVerticesInBetween++;
            }


            mesh.vertices = vertices.ToArray();
            mesh.triangleIndicies = indicies.ToArray();

            // add procedural heights
            for (int i = 0; i < vertices.Count; i++)
            {
                mesh.vertices[i] = planetInfo.GetFinalPos(mesh.vertices[i]);
            }
            mesh.RecalculateNormals();

            // the deeper chunk it the less the multiplier should be
            var skirtMultiplier = 0.95f + 0.05f * subdivisionDepth / (planetInfo.subdivisionRecurisonDepth + 2);
            skirtMultiplier = MyMath.Clamp(skirtMultiplier, 0.95f, 1.0f);


            var chunkCenter = realRange.CenterPos();


            var skirtIndicies = new List<int>();

            lineStartIndex = 0;
            nextLineStartIndex = 1;
            numberOfVerticesInBetween = 0;
            skirtIndicies.Add(0); // first line

            // all middle lines
            for (int i = 1; i < numberOfVerticesOnEdge - 1; i++)
            {
                lineStartIndex = nextLineStartIndex;
                nextLineStartIndex = lineStartIndex + numberOfVerticesInBetween + 2;
                skirtIndicies.Add(lineStartIndex);
                skirtIndicies.Add((lineStartIndex + numberOfVerticesInBetween + 1));
                numberOfVerticesInBetween++;
            }

            // last line
            lineStartIndex = nextLineStartIndex;
            for (int i = 0; i < numberOfVerticesOnEdge; i++)
            {
                skirtIndicies.Add((lineStartIndex + i));
            }


            foreach (var index in skirtIndicies)
            {
                // make skirts
                // lower the skirts towards middle
                // move chunks towards triangle center
                var v = mesh.vertices[index];
                v *= skirtMultiplier;
                v = chunkCenter + (v - chunkCenter) * skirtMultiplier;
                mesh.vertices[index] = v;
            }



            mesh.RecalculateBounds();

            renderer = planetInfo.Entity.AddComponent<MeshRenderer>();
            renderer.Mesh = mesh;
            //renderer.material = planetInfo.planetMaterial;
            renderer.HideIn(0);

        }

        public void StopMeshGeneration()
        {
            meshGenerationService.DoesNotNeedMeshGeneration(this);
        }


        public void RequestMeshGeneration()
        {
            var cam = planetInfo.Entity.Scene.mainCamera;


            // help from http://stackoverflow.com/questions/3717226/radius-of-projected-sphere
            var sphere = visibilityCollisionRange.ToBoundingSphere();
            var radiusWorldSpace = sphere.radius;
            var sphereDistanceToCameraWorldSpace = cam.Transform.Position.Distance(planetInfo.Transform.TransformPoint(sphere.center));
            var fov = cam.fieldOfView;
            var radiusCameraSpace = radiusWorldSpace * MyMath.Cot(fov / 2) / sphereDistanceToCameraWorldSpace;
            var priority = sphereDistanceToCameraWorldSpace;
            if (priority < 0) priority *= -1;
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
                generationThreadMiliSecondsSleep = 50;
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
                    }


                    if (chunk != null)
                    {
                        lock (chunkIsBeingGenerated)
                        {
                            chunkIsBeingGenerated.Add(chunk);
                        }
                        lock(chunkToPriority)
                        {
                            chunkToPriority.Remove(chunk);
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

                lock(chunkToPriority)
                {
                    var found = chunkToPriority.ContainsKey(chunk);
                    if (found == false)
                    {
                        chunkToPriority[chunk] = 0;
                    }
                    chunkToPriority[chunk] += priorityAdd;
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