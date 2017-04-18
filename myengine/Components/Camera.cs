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

		public float AspectRatio { get { return currentData.AspectRatio; } set {currentData.AspectRatio = value; } }
		public float FieldOfView { get { return currentData.FieldOfView; } set {currentData.FieldOfView = value; } }
		public float NearClipPlane { get { return currentData.NearClipPlane; } set {currentData.NearClipPlane = value; } }
		public float FarClipPlane { get { return currentData.FarClipPlane; } set {currentData.FarClipPlane = value; } }
		public bool IsOrthographic { get { return currentData.IsOrthographic; } set {currentData.IsOrthographic = value; } }
		public float OrthographicSize { get { return currentData.OrthographicSize; } set {currentData.OrthographicSize = value; } }
		public int PixelWidth { get { return currentData.PixelWidth; } private set { currentData.PixelWidth = value; } }
		public int PixelHeight { get { return currentData.PixelHeight; } private set { currentData.PixelHeight = value; } }
		public Vector2 ScreenSize { get { return currentData.ScreenSize; } private set { currentData.ScreenSize = value; } }
		public Vector3 AmbientColor { get { return currentData.AmbientColor; } private set { currentData.AmbientColor = value; } }


		public WorldPos ViewPointPosition => this.Entity.Transform.Position;


		public List<IPostProcessEffect> postProcessEffects = new List<IPostProcessEffect>();


		CameraData currentData = new CameraData();

		public override void OnAddedToEntity(Entity entity)
		{
			base.OnAddedToEntity(entity);

			entity.EventSystem.On<Events.WindowResized>(e => SetSize(e.NewPixelWidth, e.NewPixelHeight));
		}

		public CameraData GetDataCopy()
		{
			var d = currentData.Clone();
			d.ViewPointPosition = this.ViewPointPosition;
			d.Rotation = this.Transform.Rotation;
			d.Recalculate();
			return d;
		}



		public void SetSize(int newPixelWidth, int newPixelHeight)
		{
			this.PixelHeight = newPixelHeight;
			this.PixelWidth = newPixelWidth;
			ScreenSize = new Vector2(newPixelWidth, newPixelHeight);
			AspectRatio = ScreenSize.X / ScreenSize.Y;
		}


		public Matrix4 GetProjectionMatrix()
		{
			if (IsOrthographic) return Matrix4.CreateOrthographic(OrthographicSize * 2, OrthographicSize * 2 / AspectRatio, NearClipPlane, FarClipPlane);
			return Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 180.0f * FieldOfView, AspectRatio, NearClipPlane, FarClipPlane);
		}

		public Matrix4 GetRotationMatrix()
		{
			return Matrix4.CreateFromQuaternion(Quaternion.Invert(Transform.Rotation));
		}

		public Vector3 WorldToScreenPos(WorldPos worldPos)
		{
			var mp = GetRotationMatrix() * GetProjectionMatrix();
			var p = Vector4.Transform(new Vector4(this.Transform.Position.Towards(worldPos).ToVector3(), 1), mp);
			return (p.Xyz / p.W) / 2 + Vector3.One / 2;
		}


		public void AddPostProcessEffect(IPostProcessEffect shader)
		{
			postProcessEffects.Add(shader);
		}

	}

	public class CameraData
	{
		public float AspectRatio { get; set; } = 0.8f;
		public float FieldOfView { get; set; } = 45.0f;
		public float NearClipPlane { get; set; } = 0.5f;
		public float FarClipPlane { get; set; } = 5000;
		public bool IsOrthographic { get; set; } = false;
		public float OrthographicSize { get; set; } = 5;
		public int PixelWidth { get; set; }
		public int PixelHeight { get; set; }
		public Vector2 ScreenSize { get; set; } = Vector2.Zero;
		public WorldPos ViewPointPosition { get; set; } = new WorldPos();
		public Quaternion Rotation { get; set; }
		public Vector3 AmbientColor { get; set; } = Vector3.One * 0.1f;

		public CameraData Clone()
		{
			return new CameraData()
			{
				AspectRatio = AspectRatio,
				FieldOfView = FieldOfView,
				NearClipPlane = NearClipPlane,
				FarClipPlane = FarClipPlane,
				IsOrthographic = IsOrthographic,
				OrthographicSize = OrthographicSize,
				PixelWidth = PixelWidth,
				PixelHeight = PixelHeight,
				ScreenSize = ScreenSize,
				ViewPointPosition = ViewPointPosition,
				Rotation = Rotation,
				AmbientColor = AmbientColor,
			};
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
			RecalculateRotationMatrix();
		}


		private Matrix4 cachedProjectionMatrix;
		public void RecalculateProjectionMatrix()
		{
			if (IsOrthographic) cachedProjectionMatrix = Matrix4.CreateOrthographic(OrthographicSize * 2, OrthographicSize * 2 / AspectRatio, NearClipPlane, FarClipPlane);
			cachedProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 180.0f * FieldOfView, AspectRatio, NearClipPlane, FarClipPlane);
		}
		public Matrix4 GetProjectionMatrix()
		{
			return cachedProjectionMatrix;
		}


		private Matrix4 cachedRotationMatrix;
		public void RecalculateRotationMatrix()
		{
			cachedRotationMatrix = Matrix4.CreateFromQuaternion(Quaternion.Invert(Rotation));
		}
		public Matrix4 GetRotationMatrix()
		{
			return cachedRotationMatrix;
		}



		public Vector3 WorldToScreenPos(WorldPos worldPos)
		{
			var mp = GetRotationMatrix() * GetProjectionMatrix();
			var p = Vector4.Transform(new Vector4(ViewPointPosition.Towards(worldPos).ToVector3(), 1), mp);
			return (p.Xyz / p.W) / 2 + Vector3.One / 2;
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


		public void UploadCameraDataToUBO(UniformBlock ubo)
		{
			ubo.engine.viewMatrix = GetRotationMatrix();
			ubo.engine.projectionMatrix = GetProjectionMatrix();
			ubo.engine.viewProjectionMatrix = ubo.engine.viewMatrix * ubo.engine.projectionMatrix;
			ubo.engine.screenSize = this.ScreenSize;
			ubo.engine.nearClipPlane = this.NearClipPlane;
			ubo.engine.farClipPlane = this.FarClipPlane;
			ubo.engine.ambientColor = this.AmbientColor;
			ubo.engine.cameraPosition = this.ViewPointPosition.ToVector3();
			GL.Viewport(0, 0, PixelWidth, PixelHeight); MyGL.Check();
			ubo.engineUBO.UploadToGPU();
		}
	}


}
