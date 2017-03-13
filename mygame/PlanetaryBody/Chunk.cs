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
		public TriangleD NoElevationRange { get; private set; }
		TriangleD realVisibleRange;
		TriangleD rangeToCalculateScreenSizeOn;

		public List<Chunk> childs { get; } = new List<Chunk>();
		public CustomChunkMeshRenderer renderer { get; set; }

		public class CustomChunkMeshRenderer : MeshRenderer
		{
			public Chunk chunk;
			public CustomChunkMeshRenderer(Entity entity) : base(entity)
			{
			}

			public override bool ShouldRenderInContext(Camera camera, RenderContext renderContext)
			{
				if (base.ShouldRenderInContext(camera, renderContext))
				{
					var dotToCam = chunk.DotToCamera(camera);
					if (dotToCam > 0) return true;

					return false;
				}
				return false;
			}
		}

		public bool isGenerationDone;


		int subdivisionDepth;
		Root planetaryBody;



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

		public Chunk(Root planetInfo, TriangleD noElevationRange, Chunk parentChunk, ChildPosition childPosition = ChildPosition.NoneNoParent)
		{
			this.planetaryBody = planetInfo;
			this.parentChunk = parentChunk;
			this.childPosition = childPosition;
			this.NoElevationRange = noElevationRange;
			this.rangeToCalculateScreenSizeOn = noElevationRange;
		}


		TriangleD[] meshTriangles;

		TriangleD[] GetMeshTriangles()
		{
			if (meshTriangles == null)
				meshTriangles = renderer?.Mesh?.GetMeshTrianglesD();
			return meshTriangles;
		}

		Vector3d CenterPosVec3 => NoElevationRange.CenterPos;

		public double GetHeight(Vector3d chunkLocalPosition)
		{
			//var barycentricOnChunk = noElevationRange.CalculateBarycentric(planetLocalPosition);
			//var u = barycentricOnChunk.X;
			//var v = barycentricOnChunk.Y;

			var triangles = GetMeshTriangles();
			if (triangles != null)
			{
				var ray = new RayD(-CenterPosVec3.Normalized(), chunkLocalPosition);
				foreach (var t in triangles)
				{
					var hit = ray.CastRay(t);
					if (hit.DidHit)
					{
						return (ray.GetPoint(hit.HitDistance) + CenterPosVec3).Length;
					}
				}
			}

			return -1;
		}


		/// <summary>
		/// 1 looking at it from top, 0 looking from side, -1 looking from bottom
		/// </summary>
		/// <param name="cam"></param>
		/// <returns></returns>
		public double DotToCamera(Camera cam)
		{
			var dotToCamera = NoElevationRange.Normal.Dot(
				planetaryBody.Transform.Position.Towards(cam.ViewPointPosition).ToVector3d().Normalized()
			);

			return dotToCamera;
		}

		public double GetSizeOnScreen(Camera cam)
		{
			bool isVisible = true;

			var myPos = rangeToCalculateScreenSizeOn.CenterPos + planetaryBody.Transform.Position;
			var dirToCamera = myPos.Towards(cam.ViewPointPosition).ToVector3d();

			// 0 looking at it from side, 1 looking at it from top, -1 looking at it from behind
			var dotToCamera = rangeToCalculateScreenSizeOn.Normal.Dot(dirToCamera);

			var distanceToCamera = myPos.Distance(cam.ViewPointPosition);
			if (renderer != null && renderer.Mesh != null)
			{
				//var localCamPos = planetaryBody.Transform.Position.Towards(cam.ViewPointPosition).ToVector3();
				//distanceToCamera = renderer.Mesh.Vertices.FindClosest((v) => v.DistanceSqr(localCamPos)).Distance(localCamPos);
				//isVisible = cam.GetFrustum().VsBounds(renderer.GetCameraSpaceBounds(cam.ViewPointPosition));
				isVisible = renderer.GetCameraRenderStatusFeedback(cam).HasFlag(RenderStatus.Rendered);
			}

			double radiusCameraSpace;
			{
				// this is world space, doesnt take into consideration rotation, not good
				var sphere = rangeToCalculateScreenSizeOn.ToBoundingSphere();
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


			var weight = radiusCameraSpace * MyMath.SmoothStep(2, 1, MyMath.Clamp01(dotToCamera));
			if (isVisible == false) weight *= 0.3f;
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

		public void CalculateRealVisibleRange()
		{
			//rangeToCalculateScreenSizeOn = realVisibleRange;
		}

		void AddChild(Vector3d a, Vector3d b, Vector3d c, ChildPosition cp)
		{
			var range = new TriangleD();
			range.a = a;
			range.b = b;
			range.c = c;
			var child = new Chunk(planetaryBody, range, this, cp);
			childs.Add(child);
			child.subdivisionDepth = subdivisionDepth + 1;
			child.rangeToCalculateScreenSizeOn = range;
		}

		public void CreteChildren()
		{
			if (childs.Count <= 0)
			{
				var a = NoElevationRange.a;
				var b = NoElevationRange.b;
				var c = NoElevationRange.c;
				var ab = (a + b).Divide(2.0f).Normalized();
				var ac = (a + c).Divide(2.0f).Normalized();
				var bc = (b + c).Divide(2.0f).Normalized();

				ab *= planetaryBody.RadiusMin;
				ac *= planetaryBody.RadiusMin;
				bc *= planetaryBody.RadiusMin;

				AddChild(a, ab, ac, ChildPosition.Top);
				AddChild(ab, b, bc, ChildPosition.Left);
				AddChild(ac, bc, c, ChildPosition.Right);
				AddChild(ab, bc, ac, ChildPosition.Middle);
			}
		}

		public void DeleteChildren()
		{
			if (childs.Count > 0)
			{
				foreach (var child in childs)
				{
					child.DeleteChildren();
					child.DestroyRenderer();
				}
				childs.Clear();
			}
		}

		public void DestroyRenderer()
		{
			renderer?.SetRenderingMode(MyRenderingMode.DontRender);
			planetaryBody.Entity.DestroyComponent(renderer);
			renderer = null;
		}

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

	}
}