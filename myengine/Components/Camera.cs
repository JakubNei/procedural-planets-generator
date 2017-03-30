using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

using MyEngine;
using MyEngine.Components;


namespace MyEngine.Components
{

	public class Camera : ComponentWithShortcuts
	{

		public float AspectRatio { get; set; } = 0.8f;
		public float FieldOfView { get; set; } = 45.0f;
		public float NearClipPlane { get; set; } = 0.5f;
		public float FarClipPlane { get; set; } = 5000;
		public bool Orthographic { get; set; } = false;
		public float OrthographicSize { get; set; } = 5;
		public int PixelWidth { get; private set; }
		public int PixelHeight { get; private set; }
		public Vector2 ScreenSize { get; private set; } = Vector2.Zero;

		public WorldPos ViewPointPosition => this.Entity.Transform.Position;


		public Vector3 ambientColor = Vector3.One * 0.1f;
		public List<IPostProcessEffect> postProcessEffects = new List<IPostProcessEffect>();


		public override void OnAddedToEntity(Entity entity)
		{
			base.OnAddedToEntity(entity);
		
			entity.EventSystem.On<Events.WindowResized>(e => SetSize(e.NewPixelWidth, e.NewPixelHeight));

			Transform.OnRotationChanged += () => RecalculateRotationMatrix();
			RecalculateRotationMatrix();
		}

		public void SetSize(int newPixelWidth, int newPixelHeight)
		{
			this.PixelHeight = newPixelHeight;
			this.PixelWidth = newPixelWidth;
			ScreenSize = new Vector2(newPixelWidth, newPixelHeight);
			AspectRatio = ScreenSize.X / ScreenSize.Y;
			RecalculateProjectionMatrix();
		}

		public void Recalculate()
		{
			RecalculateProjectionMatrix();
		}


		private Matrix4 cachedProjectionMatrix;
		public void RecalculateProjectionMatrix()
		{
			if (Orthographic) cachedProjectionMatrix = Matrix4.CreateOrthographic(OrthographicSize * 2, OrthographicSize * 2 / AspectRatio, NearClipPlane, FarClipPlane);
			cachedProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 180.0f * FieldOfView, AspectRatio, NearClipPlane, FarClipPlane);
		}
		public Matrix4 GetProjectionMatrix()
		{
			return cachedProjectionMatrix;
		}



		private Matrix4 cachedRotationMatrix;
		public void RecalculateRotationMatrix()
		{
			cachedRotationMatrix = Matrix4.CreateFromQuaternion(Quaternion.Invert(Transform.Rotation));
		}
		public Matrix4 GetRotationMatrix()
		{
			return cachedRotationMatrix;
		}

		public Vector3 WorldToScreenPos(WorldPos worldPos)
		{
			var mp = GetRotationMatrix() * GetProjectionMatrix();
			var p = Vector4.Transform(new Vector4(this.Transform.Position.Towards(worldPos).ToVector3(), 1), mp);
			return (p.Xyz / p.W) / 2 + Vector3.One / 2;
		}

		public void UploadCameraDataToUBO(UniformBlock ubo)
		{
			ubo.engine.viewMatrix = GetRotationMatrix();
			ubo.engine.projectionMatrix = GetProjectionMatrix();
			ubo.engine.viewProjectionMatrix = ubo.engine.viewMatrix * ubo.engine.projectionMatrix;
			ubo.engine.screenSize = this.ScreenSize;
			ubo.engine.nearClipPlane = this.NearClipPlane;
			ubo.engine.farClipPlane = this.FarClipPlane;
			ubo.engine.ambientColor = this.ambientColor;
			ubo.engine.cameraPosition = this.ViewPointPosition.ToVector3();
			GL.Viewport(0, 0, PixelWidth, PixelHeight); MyGL.Check();
			ubo.engineUBO.UploadToGPU();
		}

		public void AddPostProcessEffect(IPostProcessEffect shader)
		{
			postProcessEffects.Add(shader);
		}

		/// <summary>
		/// Returns frustum planes of rotation and projection matrix
		/// </summary>
		/// <returns></returns>
		public Frustum GetFrustum()
		{
			var frustum = new Frustum();
			frustum.CalculateFrustum(GetProjectionMatrix(), GetRotationMatrix());
			return frustum;
		}
	}
}
