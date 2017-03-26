using Neitri;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace MyEngine.Components
{
	[Flags]
	public enum MyRenderingMode
	{
		DontRender = 0,
		RenderGeometry = (1 << 1),
		CastShadows = (1 << 2),
		RenderGeometryAndCastShadows = (1 << 1) | (1 << 2),
	}

	public class MeshRenderer : Renderer
	{
		/// <summary>
		/// Offset position from parent entity.
		/// </summary>
		public WorldPos Offset { get; set; } = new WorldPos();

		Mesh mesh;

		public Mesh Mesh
		{
			set
			{
				if (mesh != value)
				{
					lock (this)
					{
						mesh = value;
					}
				}
			}
			get
			{
				return mesh;
			}
		}

		static readonly Vector3[] extentsTransformsToEdges = {
																 new Vector3( 1, 1, 1),
																 new Vector3( 1, 1,-1),
																 new Vector3( 1,-1, 1),
																 new Vector3( 1,-1,-1),
																 new Vector3(-1, 1, 1),
																 new Vector3(-1, 1,-1),
																 new Vector3(-1,-1, 1),
																 new Vector3(-1,-1,-1),
															 };

		public MeshRenderer(Entity entity) : base(entity)
		{
			Material = Dependency.Create<Material>();
			RenderingMode = MyRenderingMode.RenderGeometryAndCastShadows;
		}

		public override Bounds GetFloatingOriginSpaceBounds(WorldPos viewPointPos)
		{
			var relativePos = (viewPointPos - Offset).Towards(Entity.Transform.Position).ToVector3();
			if (Mesh == null) return new Bounds(relativePos);

			var boundsCenter = relativePos + Mesh.Bounds.Center;
			var bounds = new Bounds(boundsCenter);

			if (Transform.Rotation == Quaternion.Identity)
			{
				bounds.Extents = Mesh.Bounds.Extents * Entity.Transform.Scale;
			}
			else
			{
				var boundsExtents = (Mesh.Bounds.Extents * Entity.Transform.Scale).RotateBy(Entity.Transform.Rotation);
				for (int i = 0; i < 8; i++)
				{
					bounds.Encapsulate(boundsCenter + boundsExtents.CompomentWiseMult(extentsTransformsToEdges[i]));
				}
			}

			return bounds;
		}

		public override void UploadUBOandDraw(Camera camera, UniformBlock ubo)
		{
			var modelMatrix = this.Entity.Transform.GetLocalToWorldMatrix(camera.Transform.Position - Offset);
			var modelViewMatrix = modelMatrix * camera.GetRotationMatrix();
			ubo.model.modelMatrix = modelMatrix;
			ubo.model.modelViewMatrix = modelViewMatrix;
			ubo.model.modelViewProjectionMatrix = modelViewMatrix * camera.GetProjectionMatrix();
			ubo.modelUBO.UploadToGPU();
			Mesh.Draw(Material.GBufferShader.HasTesselation);
		}

		public override bool ShouldRenderInContext(Camera camera, RenderContext renderContext)
		{
			return Mesh != null && Material != null && Material.DepthGrabShader != null && base.ShouldRenderInContext(camera, renderContext);
		}

		public Matrix4 GetModelViewProjectionMatrix(Camera camera)
		{
			var modelMatrix = this.Entity.Transform.GetLocalToWorldMatrix(camera.Transform.Position - Offset);
			var modelViewMatrix = modelMatrix * camera.GetRotationMatrix();
			var modelViewProjectionMatrix = modelViewMatrix * camera.GetProjectionMatrix();
			return modelViewProjectionMatrix;
		}

		public override IEnumerable<Vector3> GetCameraSpaceOccluderTriangles(Camera camera)
		{
			return null;

			// bad way, rasterizing whole mesh is slow, we gotta rasterize some mesh approximation like bounding box, or convex hull instead
			// dont know how to create correct complete occluder and correct minimal approximation
			/*
			var mvp = GetModelViewProjectionMatrix(camera);
			for (int i = 0; i < mesh.TriangleIndicies.Count - 3; i += 3)
			{
				yield return mesh.Vertices[mesh.TriangleIndicies[i]].Multiply(ref mvp);
				yield return mesh.Vertices[mesh.TriangleIndicies[i + 1]].Multiply(ref mvp);
				yield return mesh.Vertices[mesh.TriangleIndicies[i + 2]].Multiply(ref mvp);
			}
			*/
		}

		public override CameraSpaceBounds GetCameraSpaceBounds(Camera camera)
		{
			var b = new CameraSpaceBounds();
			if (Mesh == null) return b;

			b.maxX = float.MinValue;
			b.maxY = float.MinValue;
			b.depthFurthest = float.MinValue;

			b.minX = float.MaxValue;
			b.minY = float.MaxValue;
			b.depthClosest = float.MaxValue;

			var mvp = GetModelViewProjectionMatrix(camera);

			for (int i = 0; i < 8; i++)
			{
				var meshSpaceBoundsVertex = Mesh.Bounds.Center + Mesh.Bounds.Extents.CompomentWiseMult(extentsTransformsToEdges[i]);
				var cameraSpaceBoundVertex = meshSpaceBoundsVertex.Multiply(ref mvp);

				b.maxX = Math.Max(b.maxX, cameraSpaceBoundVertex.X);
				b.minX = Math.Min(b.minY, cameraSpaceBoundVertex.X);

				b.maxY = Math.Max(b.maxY, cameraSpaceBoundVertex.Y);
				b.minY = Math.Min(b.minY, cameraSpaceBoundVertex.Y);

				b.depthFurthest = Math.Max(b.depthFurthest, cameraSpaceBoundVertex.Z);
				b.depthClosest = Math.Min(b.depthClosest, cameraSpaceBoundVertex.Z);
			}

			return b;
		}
	}
}