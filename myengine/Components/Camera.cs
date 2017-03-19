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

        public float aspect = 0.8f;
        public float fieldOfView = 45.0f;
        public float nearClipPlane = 0.5f;
        public float farClipPlane = 5000;
        public bool orthographic = false;
        public float orthographicSize = 5;
        public int pixelWidth;
        public int pixelHeight;
        Vector2 screenSize = Vector2.Zero;

        public WorldPos ViewPointPosition => this.Entity.Transform.Position;


        public Vector3 ambientColor = Vector3.One * 0.1f;

        public List<IPostProcessEffect> postProcessEffects = new List<IPostProcessEffect>();

        public Camera(Entity entity) : base(entity)
        {
            entity.EventSystem.On<Events.WindowResized>(e => SetSize(e.NewPixelWidth, e.NewPixelHeight));
        }

        public void SetSize(int newPixelWidth, int newPixelHeight) {
            this.pixelHeight = newPixelHeight;
            this.pixelWidth = newPixelWidth;
            screenSize = new Vector2(newPixelWidth, newPixelHeight);
            aspect = screenSize.X / screenSize.Y;
        }

        public Matrix4 GetProjectionMat()
        {
            if (orthographic) return Matrix4.CreateOrthographic(orthographicSize * 2, orthographicSize * 2 / aspect, nearClipPlane, farClipPlane);
            return Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 180.0f * fieldOfView, aspect, nearClipPlane, farClipPlane);
        }
        

        public Matrix4 GetRotationMatrix()
        {
            return
                Matrix4.CreateFromQuaternion(Quaternion.Invert(Transform.Rotation));
        }

        public Vector3 WorldToScreenPos(WorldPos worldPos)
        {
            var mp = GetRotationMatrix() * GetProjectionMat();
            var p = Vector4.Transform(new Vector4(this.Transform.Position.Towards(worldPos).ToVector3(), 1), mp);
            return (p.Xyz / p.W) / 2 + Vector3.One / 2;
        }

        public void UploadDataToUBO(UniformBlock ubo)
        {
            ubo.engine.viewMatrix = GetRotationMatrix();
            ubo.engine.projectionMatrix = GetProjectionMat();
            ubo.engine.viewProjectionMatrix = ubo.engine.viewMatrix * ubo.engine.projectionMatrix;
            ubo.engine.screenSize = this.screenSize;
            ubo.engine.nearClipPlane = this.nearClipPlane;
            ubo.engine.farClipPlane = this.farClipPlane;
            ubo.engine.ambientColor = this.ambientColor;
            ubo.engine.cameraPosition = this.ViewPointPosition.ToVector3();
            GL.Viewport(0, 0, pixelWidth, pixelHeight); MyGL.Check();
            ubo.engineUBO.UploadData();
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
			frustum.CalculateFrustum(GetProjectionMat(), GetRotationMatrix());
			return frustum;
        }
    }
}
