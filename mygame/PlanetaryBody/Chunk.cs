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
		/// Planet local position range
		/// </summary>
		public TriangleD NoElevationRange { get; private set; }
		/// <summary>
		/// Planet local position range
		/// </summary>
		TriangleD realVisibleRange;
		/// <summary>
		/// Planet local position range
		/// </summary>
		TriangleD rangeToCalculateScreenSizeOn;

		List<Vector3> occluderTringles = new List<Vector3>();

		public int meshGeneratedWithShaderVersion;

		public List<Chunk> Children { get; } = new List<Chunk>();
		public CustomChunkMeshRenderer Renderer { get; set; }

		public class CustomChunkMeshRenderer : MeshRenderer
		{
			public Chunk chunk;

			/*
			public override bool ShouldRenderInContext(Camera camera, RenderContext renderContext)
			{
				if (base.ShouldRenderInContext(camera, renderContext))
				{
					// 1 looking at it from top, 0 looking from side, -1 looking from bottom
					var dotToCamera = chunk.rangeToCalculateScreenSizeOn.Normal.Dot(
						-camera.ViewPointPosition.Towards(chunk.rangeToCalculateScreenSizeOn.CenterPos + chunk.planetaryBody.Transform.Position).ToVector3d().Normalized()
					);
					if (dotToCamera > -0.2f) return true;
					return false;
				}
				return false;
			}
			*/
			public override IEnumerable<Vector3> GetCameraSpaceOccluderTriangles(Camera camera)
			{
				if (chunk.occluderTringles.Count < 9 && chunk.isGenerationDone) throw new Exception("this should not happen");
				if (chunk.isGenerationDone && chunk.occluderTringles.Count == 9)
				{
					var mvp = GetModelViewProjectionMatrix(camera);
					return chunk.occluderTringles.Select(v3 => v3.Multiply(ref mvp));
				}
				else
				{
					return null;
				}
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
				meshTriangles = Renderer?.Mesh?.GetMeshTrianglesD();
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



		public double GetSizeOnScreen(Camera cam)
		{
			bool isVisible = true;

			var myPos = rangeToCalculateScreenSizeOn.CenterPos + planetaryBody.Transform.Position;
			var dirToCamera = myPos.Towards(cam.ViewPointPosition).ToVector3d();

			// 0 looking at it from side, 1 looking at it from top, -1 looking at it from behind
			var dotToCamera = rangeToCalculateScreenSizeOn.Normal.Dot(dirToCamera);

			var distanceToCamera = myPos.Distance(cam.ViewPointPosition);
			if (Renderer != null && Renderer.Mesh != null)
			{
				//var localCamPos = planetaryBody.Transform.Position.Towards(cam.ViewPointPosition).ToVector3();
				//distanceToCamera = renderer.Mesh.Vertices.FindClosest((v) => v.DistanceSqr(localCamPos)).Distance(localCamPos);
				//isVisible = cam.GetFrustum().VsBounds(renderer.GetCameraSpaceBounds(cam.ViewPointPosition));
				isVisible = Renderer.GetCameraRenderStatusFeedback(cam).HasFlag(RenderStatus.Rendered);
			}

			double radiusCameraSpace;
			{
				// this is world space, doesnt take into consideration rotation, not good
				var sphere = rangeToCalculateScreenSizeOn.ToBoundingSphere();
				var radiusWorldSpace = sphere.radius;
				var fov = cam.FieldOfView;
				radiusCameraSpace = radiusWorldSpace * MyMath.Cot(fov / 2) / distanceToCamera;
			}


			var weight = radiusCameraSpace * MyMath.SmoothStep(2, 1, MyMath.Clamp01(dotToCamera));
			if (isVisible == false) weight *= 0.3f;
			return weight;
		}

		public void CalculateRealVisibleRange()
		{
			if (occluderTringles.Count != 0) return;

			var a = Renderer.Mesh.Vertices[planetaryBody.AIndex];
			var b = Renderer.Mesh.Vertices[planetaryBody.BIndex];
			var c = Renderer.Mesh.Vertices[planetaryBody.CIndex];

			var o = Renderer.Offset.ToVector3d();
			realVisibleRange.a = a.ToVector3d() + o;
			realVisibleRange.b = b.ToVector3d() + o;
			realVisibleRange.c = c.ToVector3d() + o;

			rangeToCalculateScreenSizeOn = realVisibleRange;

			occluderTringles.Add(a);
			occluderTringles.Add(Vector3.Zero);
			occluderTringles.Add(b);

			occluderTringles.Add(b);
			occluderTringles.Add(Vector3.Zero);
			occluderTringles.Add(c);

			occluderTringles.Add(c);
			occluderTringles.Add(Vector3.Zero);
			occluderTringles.Add(a);
		}

		void AddChild(Vector3d a, Vector3d b, Vector3d c, ChildPosition cp)
		{
			var range = new TriangleD()
			{
				a = a,
				b = b,
				c = c
			};
			var child = new Chunk(planetaryBody, range, this, cp);
			Children.Add(child);
			child.subdivisionDepth = subdivisionDepth + 1;
			child.rangeToCalculateScreenSizeOn = range;
		}

		public void CreteChildren()
		{
			if (Children.Count <= 0)
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
			if (Children.Count > 0)
			{
				foreach (var child in Children)
				{
					child.DeleteChildren();
					child.DestroyRenderer();
				}
				Children.Clear();
			}
		}

		public void DestroyRenderer()
		{
			Renderer?.SetRenderingMode(MyRenderingMode.DontRender);
			planetaryBody.Entity.DestroyComponent(Renderer);
			Renderer = null;
		}



	}
}