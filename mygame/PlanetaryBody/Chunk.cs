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
        /// <summary>
        /// Planet local position
        /// </summary>
        public Triangle noElevationRange;
        /// <summary>
        /// Planet local position
        /// </summary>
        public Triangle realVisibleRange;
        public List<Chunk> childs { get; } = new List<Chunk>();
        public MeshRenderer renderer;

        public bool IsRendererReady => renderer != null;


        int subdivisionDepth;
        Root planetaryBody;

        //MeshGenerationService GenerationService => planetaryBody.MeshGenerationService;

        public Chunk parentChunk;
        ChildPosition childPosition;

        public enum ChildPosition
        {
            Top = 0,
            Left = 1,
            Middle = 2,
            Right = 3,
            NoneNoParent = -1,
        }

        public Chunk(Root planetInfo, Chunk parentChunk, ChildPosition childPosition = ChildPosition.NoneNoParent)
        {
            this.planetaryBody = planetInfo;
            this.parentChunk = parentChunk;
            this.childPosition = childPosition;
        }


        public double GetWeight(Camera cam)
        {
            bool isVisible = true;

            var myPos = realVisibleRange.CenterPos + planetaryBody.Transform.Position;
            var dirToCamera = myPos.Towards(cam.ViewPointPosition).ToVector3d().Normalized();

            // 0 looking at chunk from side, 1 looking at chunk from front, -1 looking at chunk from bottom/behind
            var dotToCamera = realVisibleRange.Normal.Dot(dirToCamera);

            var distanceToCamera = myPos.Distance(cam.ViewPointPosition);
            if (renderer != null && renderer.Mesh != null)
            {
                var localCamPos = planetaryBody.Transform.Position.Towards(cam.ViewPointPosition).ToVector3();
                distanceToCamera = renderer.Mesh.Vertices.FindClosest((v) => v.DistanceSqr(localCamPos)).Distance(localCamPos);

                {
                    isVisible = renderer.GetCameraRenderStatus(cam).HasFlag(RenderStatus.Rendered);
                }
            }

            double radiusCameraSpace;
            {
                // this is world space, doesnt take into consideration rotation, not good
                var sphere = realVisibleRange.ToBoundingSphere();
                var radiusWorldSpace = sphere.radius;
                var fov = cam.fieldOfView;
                radiusCameraSpace = radiusWorldSpace * MyMath.Cot(fov / 2) / distanceToCamera;
            }

            /*
            {
                var a = cam.WorldToScreenPos(realVisibleRange.a + planetaryBody.Transform.Position);
                var b = cam.WorldToScreenPos(realVisibleRange.b + planetaryBody.Transform.Position);
                var c = cam.WorldToScreenPos(realVisibleRange.c + planetaryBody.Transform.Position);
                a.Z = 0;
                b.Z = 0;
                c.Z = 0;
                var aabb = new Bounds();
                aabb.Encapsulate(a);
                aabb.Encapsulate(b);
                aabb.Encapsulate(c);
                radiusCameraSpace = aabb.Size.Length;
            }
            */

            var weight = radiusCameraSpace * (1 + dotToCamera);

            renderer?.Material.Uniforms.Set("param_debugWeight", (float)weight);

            return weight;
        }
        /*
        public void RequestMeshGeneration()
        {
            if (renderer != null) return;

            var cam = planetaryBody.Entity.Scene.mainCamera;

            GenerationService.RequestGenerationOfMesh(this, GetWeight(planetaryBody.Scene.mainCamera));

            // help from http://stackoverflow.com/questions/3717226/radius-of-projected-sphere
            
            //var sphere = noElevationRange.ToBoundingSphere();
            //var radiusWorldSpace = sphere.radius;
            //var sphereDistanceToCameraWorldSpace = cam.Transform.Position.Distance(planetaryBody.Transform.Position + sphere.center.ToVector3());
            //var fov = cam.fieldOfView;
            //var radiusCameraSpace = radiusWorldSpace * MyMath.Cot(fov / 2) / sphereDistanceToCameraWorldSpace;
            //var priority = sphereDistanceToCameraWorldSpace / radiusCameraSpace;
            //if (priority < 0) priority *= -1;
            //meshGenerationService.RequestGenerationOfMesh(this, priority);
            
            //if (parentChunk != null && parentChunk.renderer != null)
            //{
            //    var cameraStatus = parentChunk.renderer.GetCameraRenderStatus(planetaryBody.Scene.mainCamera);
            //    if (cameraStatus.HasFlag(Renderer.RenderStatus.Visible)) priority *= 0.3f;
            //}
        }
        */

        void MAKE_CHILD(Vector3d A, Vector3d B, Vector3d C, ChildPosition cp)
        {
            var child = new Chunk(planetaryBody, this, cp);
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
            lock (childs)
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

                    MAKE_CHILD(a, ab, ac, ChildPosition.Top);
                    MAKE_CHILD(ab, b, bc, ChildPosition.Left);
                    MAKE_CHILD(ac, bc, c, ChildPosition.Right);
                    MAKE_CHILD(ab, bc, ac, ChildPosition.Middle);
                }
            }
        }

        int numberOfChunksGenerated = 0;
        bool isGenerated = false;

        List<int> indiciesList;
        List<int> GetIndiciesList()
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

            var numberOfVerticesOnEdge = NumberOfVerticesOnEdge;
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
                for (int y = 1; y < numberOfVerticesOnEdge - 1; y++)
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

        public bool GetVisibleChunksWithin(List<Chunk> chunksResult, Sphere sphere)
        {
            if (sphere.Intersects(this.realVisibleRange))
            {
                if (this.renderer != null && this.renderer.RenderingMode.HasFlag(MyRenderingMode.RenderGeometry))
                {
                    chunksResult.Add(this);
                    return true;
                }
                else
                {
                    var anythingAdded = false;
                    foreach (var chunk in childs)
                    {
                        anythingAdded |= chunk.GetVisibleChunksWithin(chunksResult, sphere);
                    }
                    return anythingAdded;
                }
            }
            return false;
        }
    }
}